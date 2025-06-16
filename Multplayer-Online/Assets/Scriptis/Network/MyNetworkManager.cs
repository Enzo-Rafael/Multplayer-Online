using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    [Header("Custom Prefabs")]
    //public GameObject gameManagerPrefab;
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

        /*if (gameManagerInstance == null)
        {
            GameObject gm = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gm);
            gameManagerInstance = gm.GetComponent<GameManager>();
            GameManager.Instance = gameManagerInstance;
        }*/
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
        StartCoroutine(WaitForGameManager());
    }

    System.Collections.IEnumerator WaitForGameManager()
    {
        float timeout = 5f;
        while (GameManager.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(GameManager.Instance.SetCanvasSafe());
        }
        else
        {
            Debug.LogWarning("GameManager.Instance não foi encontrado no cliente.");
        }
    }

    public override void OnClientDisconnect()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DesactiveMenus();
            GameManager.Instance.UpdatePlayerSlots();
        }
        base.OnClientDisconnect();
    }
}
