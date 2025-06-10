using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Steamworks;

[CreateAssetMenu(fileName = "SeletorCharacter", menuName = "Scriptable Objects/SeletorCharacter")]
public class SeletorCharacter : NetworkBehaviour
{
    /*Cretitos: 
        Dapper Dino:https://www.youtube.com/@DapperDinoCodingTutorials
        */
    //Variaveis
    [SerializeField] private GameObject characterSelectDisplay;//Menu de seleção de personagem
    [SerializeField] private Transform characterPreviewParent;//tranforme de onde o personagem que esta sendo mostrado vai estar 
    [SerializeField] private Text characterNameText;//onde ficara o nome do persongem
    [SerializeField] private float turnSpeed = 90f;//Velocidade da troca de seleção de personagens
    [SerializeField] private Character[] characters;//Lista ScriptableObjects dos personageens

    private int currentCharacterIndex = 0;//Index dos persongens
    private List<GameObject> characterInstances = new List<GameObject>();//Lista da preview dos personagens
    private NetworkIdentity identity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public override void OnStartClient()
    {
        //GameManager.Instance?.DesactiveMenus();
        if (characterPreviewParent.childCount == 0)
        {
            foreach (var character in characters)
            {
                GameObject characterInstance =
                    Instantiate(character.CharacterPreviewPrefab, characterPreviewParent);

                characterInstance.SetActive(false);

                characterInstances.Add(characterInstance);
            }
        }
        if (characterInstances.Count > 0)
        {
            characterInstances[currentCharacterIndex].SetActive(true);
            characterNameText.text = characters[currentCharacterIndex].CharacterName;
        }

        if (characterSelectDisplay != null) characterSelectDisplay.SetActive(true);
        identity = GetComponent<NetworkIdentity>();
    }
    

    private void Update()
    {
       if (characterPreviewParent != null)characterPreviewParent.RotateAround(characterPreviewParent.position,
        characterPreviewParent.up, turnSpeed * Time.deltaTime);
    }
   
    public void Select()
    {//Confirma a opção excolhida de qual persongem vai ser jogado
        if (!NetworkClient.ready) return;
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager ainda não foi sincronizado.");
            return;
        }
        if (characterInstances.Count <= currentCharacterIndex || characterInstances[currentCharacterIndex] == null)
        {
            Debug.LogError("Preview de personagem não encontrado.");
            return;
        }//Faz uma chamada no gama manager para ver se alguns do personagens esta em cena
        // Verifica os SyncVars diretamente (já sincronizados do servidor)
        bool isTaken = (currentCharacterIndex == 0 && GameManager.Instance.player01) ||
                       (currentCharacterIndex == 1 && GameManager.Instance.player02);

        if (!isTaken)
        {
            CmdSelect(currentCharacterIndex, identity.connectionToClient);
        }
        else
        {
            BtnChangeLeft();
        }

    }
    [Command(requiresAuthority = false)]
    public void CmdSelect(int characterIndex, NetworkConnectionToClient sender)
    {//O jogo nescessita dos dois persongens para funcionar 
        if (characterIndex == 0) GameManager.Instance.player01 = true;
        if (characterIndex == 1) GameManager.Instance.player02 = true;

        GameManager.Instance.SetIndexCurrent(characterIndex);

        GameObject characterInstance = Instantiate(
            characters[characterIndex].GameplayCharacterPrefab,
            GameManager.Instance.startPosition[characterIndex].position,
            GameManager.Instance.startPosition[characterIndex].rotation
        );

        NetworkServer.Spawn(characterInstance, sender);
        NetworkServer.SetClientReady(sender);//!!! isso pode gerar um conflito
        //----------------------------------------------
        GameManager.Instance.UpdatePlayerSlots();
        // Atribui autoridade explicitamente
        //characterInstance.GetComponent<NetworkIdentity>().AssignClientAuthority(sender);// Atribui autoridade ao cliente
        TargetOnCharacterSpawned(sender, characterIndex);// Informa ao cliente para ocultar seleção e ativar menus
        GameManager.Instance.TargetSyncState(sender);
    }
    [TargetRpc]
    private void TargetOnCharacterSpawned(NetworkConnection target, int characterIndex)
    {
        // Oculta menu de seleção local
        if (characterSelectDisplay != null)characterSelectDisplay.SetActive(false);

        // Ativa UI do jogador
        GameManager.Instance.ActiveMenus();
        GameManager.Instance.ShowPoints();

        // Se for o host (servidor + cliente), ativa botão iniciar
        if (isServer)GameManager.Instance.ShowStart();
    }

    public void BtnChangeRight(){//Vai fazer a troca de personagem levando o a auteração de valores para a direita
        if (characterInstances.Count == 0) return;
        characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex = (currentCharacterIndex + 1) % characterInstances.Count;

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;
    }

    public void BtnChangeLeft(){//Vai fazer a troca de personagem levando o a auteração de valores para a esquerda
        if (characterInstances.Count == 0) return;
        characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex --;
        if(currentCharacterIndex < 0){
            currentCharacterIndex += characterInstances.Count;
        }

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;
    }

}
