using Mirror;
using UnityEngine;

public class ResetMetch : MonoBehaviour
{
    public void OnClickReiniciar()//Apena para o botão de resdetar partida
    {
        GameManager.Instance.CmdRequestReset();
        gameObject.SetActive(false);
    }
}
