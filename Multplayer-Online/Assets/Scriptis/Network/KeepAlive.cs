using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class KeepAlive : NetworkBehaviour
{
    /*float timer;
    private bool canPing = false;

     public override void OnStartClient()
     {
     base.OnStartClient();
     canPing = true;
     }

     void Update()
     {
         if (!canPing || !authority || !NetworkClient.isConnected || !NetworkClient.ready)
         {
             //Debug.LogWarning("Tentando enviar PingMessage sem estar conectado.");
             return;
         }
         if (NetworkClient.isConnected)
         {
             timer += Time.deltaTime;
             if (timer > 5f)
             {

                 NetworkClient.Send(new PingMessage());
                 timer = 0;
             }
         }
     }*/
    private float timer;

    void Update()
    {
        if (!isLocalPlayer || !NetworkClient.isConnected || !NetworkClient.ready) return;

        timer += Time.deltaTime;
        if (timer >= 5f) // A cada 5 segundos
        {
            NetworkClient.Send(new PingMessage());
            timer = 0f;
        }
    }
    
}