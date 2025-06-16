using Mirror;
using UnityEngine;

public class ResetMetch : MonoBehaviour
{
    public void OnClickReiniciar()//Apena para o botão de resdetar partida
    {
        if (GameManager.Instance != false)
        {
            GameManager.Instance.CmdRequestReset();
            gameObject.SetActive(false);
        }else
    {
        Debug.LogError("GameManager não encontrado!");
    }
    }
}
