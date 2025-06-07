using System;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSteamIdUpdate))]
    private ulong steamID;

    [SerializeField] private RawImage profileImage = null;
    [SerializeField] private TMP_Text displayName = null;

    public void SetID(ulong steamID)//Serve para setar o ID do server
    {
        this.steamID = steamID;
    }


    private void HandleSteamIdUpdate(ulong oldID, ulong newID)
    {
        var cSteamID = new CSteamID(newID);

        displayName.text = SteamFriends.GetFriendPersonaName(cSteamID);

        int imageID = SteamFriends.GetLargeFriendAvatar(cSteamID);
        if (imageID == -1) { return; }
        profileImage.texture = GetSteamImageAsTexture(imageID);
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;


        return texture;
    }

}
