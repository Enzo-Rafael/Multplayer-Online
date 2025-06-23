using UnityEngine;
using Mirror;
using TMPro;
public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI chatArea;
    GameObject dono;

    void Start()
    {
        if (NetworkServer.active)
            NetworkServer.RegisterHandler<ChatMessage>(OnMessageReceived);
        if (NetworkClient.isConnected)
        {
            NetworkClient.RegisterHandler<ChatMessage>(OnMessageReceivedClient);
        }

    }
   
    public void OnSendButton()
    {
        if (!isClient) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        if (GameObject.FindGameObjectWithTag("Player1").GetComponent<NetworkIdentity>().isOwned)
        {
            dono = GameObject.FindGameObjectWithTag("Player1");
            
        }
        else if (GameObject.FindGameObjectWithTag("Player2").GetComponent<NetworkIdentity>().isOwned)
        {
            dono = GameObject.FindGameObjectWithTag("Player2");
        }
        else
        {
            Debug.LogError("Player não encontrado");
        }
        Debug.Log(dono.name);

        ChatMessage msg = new()
        {
            sender = dono.GetComponent<PlayerName>().playerName,
            content = inputField.text
        };

        NetworkClient.Send(msg); // Envia para o servidor
        inputField.text = "";
    }

    void OnMessageReceived(NetworkConnection conn, ChatMessage msg)
    {
        string finalText = $"<b>{msg.sender}</b>: {msg.content}\n";
        chatArea.text += finalText;

        // Se for servidor, repassa para todos
        if (NetworkServer.active)
            NetworkServer.SendToAll(msg);
    }
    void OnMessageReceivedClient( ChatMessage msg)
    {
        string finalText = $"<b>{msg.sender}</b>: {msg.content}\n";
        chatArea.text += finalText;

        // Se for servidor, repassa para todos
        if (isServer)
        {
            if (NetworkServer.active)
            NetworkServer.SendToAll(msg);
        }
        
    }
}

