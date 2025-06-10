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
        if (!authority || !NetworkClient.ready) return;

        timer += Time.deltaTime;
        if (timer > 5f)
        {
            NetworkClient.Send(new PingMessage());
            timer = 0;
        }
    }
}