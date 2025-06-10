using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using System.Collections;
using Telepathy;

public class SteamLobby : MonoBehaviour
{
    //GameObjects
    [SerializeField] private GameObject buttons = null;
    [SerializeField] private Text uiTxt = null;
    //Callblack
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    //Variables
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private MyNetworkManager networkManager;
    public static CSteamID iD { get; private set; }

    private void Start()
    {
        networkManager = FindAnyObjectByType<MyNetworkManager>();
        Debug.Log(networkManager);
        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()//Inicia o host
    {
        buttons.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)//Serve para criar o Lobby
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            Debug.LogError("Falha ao criar lobby.");
            return;
        }

        iD = new CSteamID(callback.m_ulSteamIDLobby);
        StartCoroutine(WaitForServerStartAndSetupLobby());
    } 

    private IEnumerator WaitForServerStartAndSetupLobby()
    {
        networkManager.StartHost();

        // Espera até o servidor estar ativo
        float timeout = 5f;
        while (!NetworkServer.active && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!NetworkServer.active)
        {
            Debug.LogError("StartHost falhou! NetworkServer não está ativo.");
            yield break;
        }

        SteamMatchmaking.SetLobbyData(iD, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(iD, "name", SteamFriends.GetPersonaName() + "'s Lobby");

        Debug.Log("Servidor iniciado e lobby configurado.");
    }
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)//Serve para solicitar entrada no lobby
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        buttons.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        uiTxt.gameObject.SetActive(true);
        uiTxt.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        if (NetworkServer.active) return;

        StartCoroutine(WaitAndConnectToHost());
    }
    private IEnumerator WaitAndConnectToHost()
    {
        string hostAddress = "";

        for (int i = 0; i < 10; i++)
        {
            hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey);
            if (!string.IsNullOrEmpty(hostAddress))
                break;
            yield return new WaitForSeconds(0.5f); // espera meio segundo
        }

        if (!string.IsNullOrEmpty(hostAddress))
        {
            Debug.Log("1");
            networkManager.networkAddress = hostAddress;
            networkManager.StartClient();
            NetworkClient.Ready();//!!!
            StartCoroutine(WaitForGameManager());
        }
        else
        {
            Debug.LogError("Erro ao conectar: HostAddress vazio.");
        }
    }
    private IEnumerator WaitForGameManager()
    {
       yield return new WaitUntil(() => GameManager.Instance != null);

       GameManager.Instance.InitializeMenus(); // ou TargetSyncState se for necessário
    }
}
