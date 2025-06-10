using Mirror;
using UnityEngine;

public class ResetMetch : NetworkBehaviour
{
    public void OnClickReiniciar()//Apena para o botão de resdetar partida
    {
        if (!NetworkClient.ready) return;
        GameManager.Instance.CmdRequestReset();
        gameObject.SetActive(false);
    }
}
