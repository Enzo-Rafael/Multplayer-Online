using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Collections;

public class SteamLobby : MonoBehaviour
{
    //references
    [SerializeField] private GameObject buttons;
    [SerializeField] private Text uiTxt;
    //Calbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    //Configs
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private MyNetworkManager networkManager;
    public static CSteamID iD { get; private set; }

    private void Start()
    {
        networkManager = FindAnyObjectByType<MyNetworkManager>();
        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        buttons.SetActive(false);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            Debug.LogError("Falha ao criar lobby.");
            return;
        }

        iD = new CSteamID(callback.m_ulSteamIDLobby);
        StartCoroutine(SetupLobby());
    }

    IEnumerator SetupLobby()
    {
        networkManager.StartHost();
        yield return new WaitUntil(() => NetworkServer.active);
        if (!NetworkServer.active)
        {
            Debug.Log("StartHost falhou");
            yield break;
        }

        SteamMatchmaking.SetLobbyData(iD, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(iD, "name", SteamFriends.GetPersonaName() + "'s Lobby");
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
        StartCoroutine(ConnectClient());
    }

    private IEnumerator ConnectClient()
    {
        string hostAddress = "";

        for (int i = 0; i < 10; i++)
        {
            hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey);
            if (!string.IsNullOrEmpty(hostAddress)) break;
            yield return new WaitForSeconds(0.5f);
        }

        if (string.IsNullOrEmpty(hostAddress))
        {
            Debug.LogError("HostAddress vazio.");
            yield break;
        }

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
        yield return new WaitUntil(() => NetworkClient.isConnected);
    }
}
