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
    [SerializeField] private GameObject characterSelectDisplay = default;//Menu de seleção de personagem
    [SerializeField] private Transform characterPreviewParent = default;//tranforme de onde o personagem que esta sendo mostrado vai estar 
    [SerializeField] private Text characterNameText;//onde ficara o nome do persongem
    [SerializeField] private float turnSpeed = 90f;//Velocidade da troca de seleção de personagens
    [SerializeField] private Character[] characters = default;//Lista ScriptableObjects dos personageens
    private NetworkIdentity identity = default;
    private int currentCharacterIndex = 0;//Index dos persongens
    private List<GameObject> characterInstances = new List<GameObject>();//Lista da preview dos personagens

    private void Start()
    {
         Cursor.lockState = CursorLockMode.None;
         Cursor.visible = true;
    }
    public override void OnStartClient()
    {
        GameManager.Instance.DesactiveMenus();
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
        
        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;

        characterSelectDisplay.SetActive(true);
        identity = GetComponent<NetworkIdentity>();
    }
    

    private void Update()
    {
        characterPreviewParent.RotateAround(characterPreviewParent.position, characterPreviewParent.up, turnSpeed * Time.deltaTime);
    }
   
    public void Select()
    {//Confirma a opção excolhida de qual persongem vai ser jogado
        /*if((currentCharacterIndex == 0 && P1Selected == false) || (currentCharacterIndex == 1 && P2Selected == false)){//Trava de um personagem por jogador
            CmdSelect(currentCharacterIndex);
            characterSelectDisplay.SetActive(false); 
        }else{
            BtnChangeLeft();
        }*/
        GameManager.Instance.CheckCharactersDisponibility();//Faz uma chamada no gama manager para ver se alguns do personagens esta em cena
        if ((currentCharacterIndex == 0 && GameManager.Instance.player01 == false) || (currentCharacterIndex == 1 && GameManager.Instance.player02 == false))
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
        GameObject characterInstance = Instantiate(characters[characterIndex].GameplayCharacterPrefab,
         GameManager.Instance.startPosition[characterIndex].position, GameManager.Instance.startPosition[characterIndex].rotation);
        NetworkServer.Spawn(characterInstance, sender);
        //----------------------------------------------
        // Atribui autoridade explicitamente
        characterInstance.GetComponent<NetworkIdentity>().AssignClientAuthority(sender);// Atribui autoridade ao cliente
        TargetOnCharacterSpawned(sender, characterIndex);// Informa ao cliente para ocultar seleção e ativar menus
    }
    [TargetRpc]
    void TargetOnCharacterSpawned(NetworkConnection target, int characterIndex)
    {
        // Oculta menu de seleção local
        characterSelectDisplay.SetActive(false);

        // Ativa UI do jogador
        GameManager.Instance.ActiveMenus();
        GameManager.Instance.ShowPoints();

        // Se for o host (servidor + cliente), ativa botão iniciar
        if (isServer)GameManager.Instance.ShowStart();
    }

    public void BtnChangeRight(){//Vai fazer a troca de personagem levando o a auteração de valores para a direita
        characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex = (currentCharacterIndex + 1) % characterInstances.Count;

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;
    }

    public void BtnChangeLeft(){//Vai fazer a troca de personagem levando o a auteração de valores para a esquerda
        characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex --;
        if(currentCharacterIndex < 0){
            currentCharacterIndex += characterInstances.Count;
        }

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;
    }

}
