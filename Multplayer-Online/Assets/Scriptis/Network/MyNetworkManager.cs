using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

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
    // Apenas recebe para manter a conexão viva, não precisa de resposta.
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (gameManagerInstance == null)
        {
            GameObject gm = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gm);
            gameManagerInstance = gm.GetComponent<GameManager>();
            GameManager.Instance = gameManagerInstance;
        }
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        GameManager.Instance?.TargetSyncState(conn);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        if (!NetworkClient.ready) NetworkClient.Ready();
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
        while ((GameManager.Instance == null || UIManager.Instance == null)&& timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.DesactiveMenus();
        }else
        {
            Debug.LogError("UIManager.Instance está nulo!");
        }
           
        yield return new WaitUntil(() => GameManager.Instance != null);
        UIManager.Instance.DesactiveMenus();
        if (GameManager.Instance != null) GameManager.Instance.TargetSyncState(NetworkClient.connection);
    }

    public override void OnClientDisconnect()
    {
        if (GameManager.Instance != null)
        {
            if (UIManager.Instance != null)UIManager.Instance.DesactiveMenus();
            GameManager.Instance.UpdatePlayerSlots();
        }
        base.OnClientDisconnect();
    }
}
