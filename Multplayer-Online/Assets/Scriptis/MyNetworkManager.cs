using UnityEngine;
using Unity.VisualScripting;
using Mirror;
using Steamworks;
public class MyNetworkManager : NetworkManager
{
    /*public override void OnStartHost()//Acontece quando o Host Inicia
    {
        base.OnStartHost();
        //GameManager.Instance.menu[6].SetActive(true);
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
        GameManager.Instance.ShowStart();
    }
    public override void OnClientDisconnect()//Acontece quando o Cliente desconecta
    {

        Debug.Log("Desonectei");
        GameManager.Instance.DesActiveMneu();
        GameManager.Instance.CheckCharactersDisponibility();
        base.OnClientDisconnect();
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)//Serve para pegar o Steam ID do jogador
    {
        base.OnServerAddPlayer(conn);
        CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.iD,
        numPlayers - 1);

        var playerName = conn.identity.GetComponent<PlayerName>();
        if(playerName != null)playerName.SetID(steamID.m_SteamID);

        
    }
}
