using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [Header("Settings")]
    [SyncVar] public int characterType;

    public override void OnStartAuthority()
    {
        // Ativa a câmera e controles apenas para o jogador local
        GetComponent<PlayerMuve>().enabled = true;
        GetComponent<PlayerName>().enabled = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.Instance.UpdatePlayerSlots();
    }

    public override void OnStopClient()
    {
        GameManager.Instance.UpdatePlayerSlots();
        base.OnStopClient();
    }
}
