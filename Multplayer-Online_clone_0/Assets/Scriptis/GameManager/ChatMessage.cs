using UnityEngine;
using Mirror;
public struct ChatMessage : NetworkMessage
{
    public string sender;
    public string content;

}
