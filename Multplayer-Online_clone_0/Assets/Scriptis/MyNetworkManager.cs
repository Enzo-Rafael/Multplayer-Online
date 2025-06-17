using UnityEngine;
using Unity.VisualScripting;
using Mirror;
public class MyNetworkManager : NetworkManager
{
    public override void Awake()
    {
        base.Awake();
        NetworkClient.RegisterHandler<ChatMessage>(OnMessageReceivedClient);
    }
    void OnMessageReceivedClient( ChatMessage msg)
    {
        string finalText = $"<b>{msg.sender}</b>: {msg.content}\n";
    }
    /*public override void OnStartHost()//Acontece quando o Host Inicia
    {
        base.OnStartHost();
        GameManager.Instance.SetSpawnPos(NetworkManager.startPositions.ToArray());
    }*/

    public override void OnServerConnect(NetworkConnectionToClient conn)//Acontece quando o Server Inicia
    {
        base.OnServerConnect(conn);
        Debug.Log("Ola, conectei ");
        //GameManager.Instance.startPos = NetworkManager.startPositions;

        GameManager.Instance.CheckCharactersDisponibility();

    }
    public override void OnClientConnect()//Acontece quando o Cliente conecta
    {
        
        base.OnClientConnect();
        GameManager.Instance.SetCanvas();
    }
    public override void OnClientDisconnect()//Acontece quando o Cliente desconecta
    {

        Debug.Log("Desonectei");
        GameManager.Instance.DesActiveMneu();
        GameManager.Instance.CheckCharactersDisponibility();
        base.OnClientDisconnect();
    }
}
