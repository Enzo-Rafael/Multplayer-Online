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
        NetworkServer.RegisterHandler<CharacterSelectMessage>(OnCharacterSelected);
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
        //if (GameManager.Instance != null) GameManager.Instance.TargetSyncState(NetworkClient.connection);
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

    private void OnCharacterSelected(NetworkConnectionToClient conn, CharacterSelectMessage msg)
    {
        int index = msg.characterIndex;

        if (index == 0 && GameManager.Instance.player01) return;
        if (index == 1 && GameManager.Instance.player02) return;

        if (index == 0) GameManager.Instance.player01 = true;
        if (index == 1) GameManager.Instance.player02 = true;

        GameManager.Instance.SetIndexCurrent(index);

        GameObject characterPrefab = GameManager.Instance.characters[index].GameplayCharacterPrefab;
        Transform spawn = GameManager.Instance.startPosition[index];

        GameObject playerObj = Instantiate(characterPrefab, spawn.position, spawn.rotation);
        NetworkServer.Spawn(playerObj, conn);

        GameManager.Instance.UpdatePlayerSlots();
        GameManager.Instance.TargetSyncState(conn);

        UIManager.Instance.ActiveMenus();
        
    }
}
