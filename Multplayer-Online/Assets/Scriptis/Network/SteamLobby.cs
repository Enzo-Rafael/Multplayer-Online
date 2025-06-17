using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

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
    [SerializeField]private MyNetworkManager networkManager;
    public static CSteamID ID { get; private set; }

    private void Awake()
    {
        if (networkManager == null) networkManager = FindAnyObjectByType<MyNetworkManager>();
    }

    private void Start()
    {
       
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

    private void OnLobbyCreated(LobbyCreated_t callback)//Cria o Lobby Steam + inicia o servidor Mirror
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            Debug.LogError("Falha ao criar lobby.");
            return;
        }

        ID = new CSteamID(callback.m_ulSteamIDLobby);
        StartCoroutine(SetupLobby());
    }

    private IEnumerator SetupLobby()
    {
        networkManager.StartHost();
        yield return new WaitUntil(() => NetworkServer.active);
        if (!NetworkServer.active)
        {
            Debug.Log("StartHost falhou");
            yield break;
        }

        SteamMatchmaking.SetLobbyData(ID, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(ID, "name", SteamFriends.GetPersonaName() + "'s Lobby");
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)//Serve para solicitar entrada no lobby
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)//Entra no lobby Steam + conecta ao servidor Mirror
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
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam não está inicializado.");
            yield break;
        }
        yield return new WaitForSeconds(1f); 

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
        yield return new WaitUntil(() => NetworkServer.active);
        networkManager.StartClient();
        yield return new WaitUntil(() => NetworkClient.isConnected);
        if (GameManager.Instance == null)
        {
            GameManager.Instance = NetworkClient.spawned.Values
            .Select(go => go.GetComponent<GameManager>())
            .FirstOrDefault(gm => gm != null);
        }
        StartCoroutine(WaitForGameManager());
    }
    private IEnumerator WaitForGameManager()
    {
        float timeout = 5f;
        while (GameManager.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        if (GameManager.Instance == null)
        {
            TryFindGameManager();
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager encontrado no cliente.");
            UIManager.Instance.DesactiveMenus();
        }
        else
        {
            Debug.LogError("GameManager não encontrado no cliente.");
        }
    }
    private void TryFindGameManager()
    {
        foreach (var obj in NetworkClient.spawned.Values)
        {
            GameManager.Instance = NetworkClient.spawned.Values
            .Select(go => go.GetComponent<GameManager>())
            .FirstOrDefault(gm => gm != null);
            if (GameManager.Instance != null)
            {
                Debug.LogWarning("GameManager localizado via NetworkClient.spawned.");
                break;
            }
        }
    }
}
