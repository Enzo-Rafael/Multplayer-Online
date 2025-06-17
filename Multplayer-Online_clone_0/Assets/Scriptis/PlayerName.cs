using Mirror;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    [SyncVar]
    public string playerName = "Player";

    public override void OnStartLocalPlayer()
    {
        string chosenName = "Player_" + Random.Range(100, 999);
        CmdSetName(chosenName);
    }

    [Command]
    void CmdSetName(string name)
    {
        playerName = name;
    }
}
