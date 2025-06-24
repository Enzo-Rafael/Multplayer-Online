using UnityEngine;
using Mirror;
using TMPro;
using System;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private TMP_Text chatArea = null;
    [SerializeField] private string nameP;

    private static event Action<string> OnMessage;

    public override void OnStartAuthority()//Detecta se quem iniciou foi o real dono do codigo 
    {
        chatUI.SetActive(true);

        OnMessage += HandleNewMessage;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!isOwned) { return; }

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message)      
    {
        chatArea.text += message;
    }
    [Client]
    public void Send(string message)//Onde é chamado para enviar a mensagem
    {
        if (!Input.GetKeyDown(KeyCode.Return)) { return; }

        if (string.IsNullOrWhiteSpace(message)) { return; }

        CmdSendMessage(message);

        inputField.text = string.Empty;
    
    }

    [Command]
    private void CmdSendMessage(string message)//Onde a mensagem 
    {
        RpcHandleMessage($"[{connectionToClient.connectionId}-{nameP}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }
}

