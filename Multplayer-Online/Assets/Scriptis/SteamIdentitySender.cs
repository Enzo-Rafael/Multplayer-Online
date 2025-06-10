using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class SteamIdentitySender : NetworkBehaviour
{
    public override void OnStartAuthority()
    {
        base.OnStartLocalPlayer();
        CmdSendSteamID(SteamUser.GetSteamID().m_SteamID);
        Debug.Log("id: "+SteamUser.GetSteamID().m_SteamID);
    }

    [Command]
    void CmdSendSteamID(ulong id)
    {
        GameManager.Instance.steamIdGM = new CSteamID(id);
    }
}
