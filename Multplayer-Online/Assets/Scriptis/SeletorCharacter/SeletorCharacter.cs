using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Steamworks;

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

        if (characterSelectDisplay != null)characterSelectDisplay.SetActive(true);
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
    [Command(requiresAuthority = false)]
    public void CmdCheckCharactersDisponibility()
    {
        GameManager.Instance.UpdatePlayerSlots();
    }

    [Command(requiresAuthority = false)]
    public void CmdSelect(int characterIndex, NetworkConnectionToClient sender = null)
    {

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
        GameManager.Instance.UpdatePlayerSlots();
        // Manda para o cliente ocultar a seleção e ativar a UI
        
        TargetOnCharacterSelected(sender);
    }
    //---------TargetRpc--------------------------------------------------------------
    [TargetRpc]
    void TargetOnCharacterSelected(NetworkConnection target)
    {
    Debug.Log("Personagem selecionado com sucesso no cliente.");
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager não encontrado no cliente.");
            return;
        }

        if (characterSelectDisplay != null) characterSelectDisplay.SetActive(false);

        GameManager.Instance.ActiveMenus();
        GameManager.Instance.ShowPoints();

        if (isServer)GameManager.Instance.ShowStart();
    }

}
