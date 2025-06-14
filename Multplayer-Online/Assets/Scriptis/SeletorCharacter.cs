using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Steamworks;
[CreateAssetMenu(fileName = "SeletorCharacter", menuName = "Scriptable Objects/SeletorCharacter")]
public class SeletorCharacter : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject characterSelectDisplay;
    [SerializeField] private Transform characterPreviewParent;
    [SerializeField] private Text characterNameText;
    [SerializeField] private float turnSpeed = 90f;

    [Header("Characters")]
    [SerializeField] private Character[] characters;

    private int currentCharacterIndex = 0;
    private List<GameObject> characterInstances = new List<GameObject>();

    private NetworkIdentity identity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnStartClient()
    {
        identity = GetComponent<NetworkIdentity>();

        if (characterPreviewParent.childCount == 0)
        {
            foreach (var character in characters)
            {
                GameObject characterInstance = Instantiate(character.CharacterPreviewPrefab, characterPreviewParent);
                characterInstance.SetActive(false);
                characterInstances.Add(characterInstance);
            }
        }

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;

        characterSelectDisplay.SetActive(true);
    }

    private void Update()
    {
        characterPreviewParent.RotateAround(characterPreviewParent.position, characterPreviewParent.up, turnSpeed * Time.deltaTime);
    }

    public void Select()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager não encontrado.");
            return;
        }

        CmdCheckCharactersDisponibility();

        if ((currentCharacterIndex == 0 && GameManager.Instance.player01 == false) ||
            (currentCharacterIndex == 1 && GameManager.Instance.player02 == false))
        {
            CmdSelect(currentCharacterIndex);
        }
        else
        {
            BtnChangeLeft();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdCheckCharactersDisponibility()
    {
        GameManager.Instance.CheckCharactersDisponibility();
    }

    [Command(requiresAuthority = false)]
    public void CmdSelect(int characterIndex)
    {
        var sender = connectionToClient;

        if (characterIndex == 0) GameManager.Instance.player01 = true;
        if (characterIndex == 1) GameManager.Instance.player02 = true;

        GameManager.Instance.SetIndexCurrent(characterIndex);

        GameObject characterInstance = Instantiate(
            characters[characterIndex].GameplayCharacterPrefab,
            GameManager.Instance.startPosition[characterIndex].position,
            GameManager.Instance.startPosition[characterIndex].rotation
        );

        NetworkServer.Spawn(characterInstance, sender);
        //NetworkServer.SetClientReady(sender);

        // Manda para o cliente ocultar a seleção e ativar a UI
        GameManager.Instance.TargetSyncState(sender);
    }

    public void BtnChangeRight()
    {
        characterInstances[currentCharacterIndex].SetActive(false);
        currentCharacterIndex = (currentCharacterIndex + 1) % characterInstances.Count;
        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;
    }

    public void BtnChangeLeft()
    {
        characterInstances[currentCharacterIndex].SetActive(false);
        currentCharacterIndex = (currentCharacterIndex - 1 + characterInstances.Count) % characterInstances.Count;
        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;
    }
}
