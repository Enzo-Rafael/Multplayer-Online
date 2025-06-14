using System;
using System.Collections;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSteamIdUpdate))]
    private ulong steamID;

    [Header("UI")]
    [SerializeField] private RawImage profileImage = null;
    [SerializeField] private TMP_Text displayName = null;

    protected Callback<AvatarImageLoaded_t> avatarILoad;
    public override void OnStartAuthority()//Quando a autoridade comer sera executado
    {
        CmdSetSteamID((ulong)SteamUser.GetSteamID());
    }
    [Command]
    public void CmdSetSteamID(ulong id)
    {
        steamID = id; // Isso sincroniza para todos os clientes automaticamente
    }

    public override void OnStartClient()
    {
        avatarILoad = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        HandleSteamIdUpdate(0, steamID);
    }

    private void HandleSteamIdUpdate(ulong oldID, ulong newID)
    {
        if (!SteamManager.Initialized) return;

        var cSteamID = new CSteamID(newID);
        
        displayName.text = SteamFriends.GetFriendPersonaName(cSteamID);

        int imageID = SteamFriends.GetLargeFriendAvatar(cSteamID);
        if (imageID == -1) { return; }
        profileImage.texture = GetSteamImageAsTexture(imageID);
    }
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID != steamID) { return; }
        Texture2D avatarTexture = GetSteamImageAsTexture(callback.m_iImage);
        if (avatarTexture != null)
        {
            profileImage.texture = avatarTexture;
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool success = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (!success) return null;

        byte[] imageBytes = new byte[width * height * 4];

        success = SteamUtils.GetImageRGBA(iImage, imageBytes, (int)(width * height * 4));
        if (!success) return null;

        texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
        texture.LoadRawTextureData(imageBytes);
        texture.Apply();

        return texture;
    }
}
