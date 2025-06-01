using UnityEngine;
using Unity.VisualScripting;
using Mirror;
public class MyNetworkManager : NetworkManager
{
    public override void OnStartHost()//Acontece quando o Host Inicia
    {
        base.OnStartHost();
        GameManager.Instance.SetSpawnPos(NetworkManager.startPositions.ToArray());
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
    }
    public override void OnServerConnect(NetworkConnectionToClient conn)//Acontece quando o Serve Inicia
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
