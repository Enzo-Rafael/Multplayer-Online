using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class KeepAlive : NetworkBehaviour
{
   float timer;

    void Update()
    {
        if (!isLocalPlayer) return;

        timer += Time.deltaTime;
        if (timer > 10f)
        {
            NetworkClient.Send(new PingMessage());
            timer = 0;
        }
    }
}

public struct PingMessage : NetworkMessage {}