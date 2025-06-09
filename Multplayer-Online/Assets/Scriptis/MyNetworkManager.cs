using UnityEngine;
using Unity.VisualScripting;
using Mirror;
using Steamworks;
public class MyNetworkManager : NetworkManager
{
    [Header("Custom Prefabs")]
    public GameObject gameManagerPrefab;
    private GameManager gameManagerInstance;

    public override void Awake()
    {
        base.Awake();
        NetworkClient.RegisterHandler<PingMessage>(OnPingMessage);
    }

    void OnPingMessage(PingMessage msg)
    {
        // apenas recebe, nada a fazer
    }
    public override void OnStartHost()//Acontece quando o Host Inicia
    {
        base.OnStartHost();
        NetworkServer.disconnectInactiveConnections = false;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        gameManagerInstance = Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        NetworkServer.Spawn(gameManagerInstance.gameObject);
        GameManager.Instance = gameManagerInstance;
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)//Acontece quando o Server Inicia
    {
        base.OnServerConnect(conn);
        Debug.Log("Ola, conectei ");
        //GameManager.Instance.startPos = NetworkManager.startPositions;

        GameManager.Instance?.CheckCharactersDisponibility();

    }
    public override void OnClientConnect()//Acontece quando o Cliente conecta
    {

        base.OnClientConnect();
        GameManager.Instance.StartCoroutine(GameManager.Instance.SetCanvasSafe());
    }
    public override void OnClientDisconnect()//Acontece quando o Cliente desconecta
    {

        Debug.Log("Desonectei");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DesactiveMenus();
            GameManager.Instance.CheckCharactersDisponibility();
        }
        base.OnClientDisconnect();
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)//Chamada quando o server adiciona um player
    {
        base.OnServerAddPlayer(conn);
        if (GameManager.Instance != null)GameManager.Instance.TargetSyncState(conn);
    }
}
