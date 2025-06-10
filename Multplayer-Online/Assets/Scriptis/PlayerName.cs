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

    [SerializeField] private RawImage profileImage = null;
    [SerializeField] private TMP_Text displayName = null;

    protected Callback<AvatarImageLoaded_t> avatarILoad;
    public override void OnStartAuthority()//Quando a autoridade comer sera executado
    {
        StartCoroutine(SetSteamIdWithDelay());
    }

    private IEnumerator SetSteamIdWithDelay()
{
    yield return new WaitUntil(() => SteamManager.Initialized);
    yield return new WaitForSeconds(0.1f);
    CmdSetSteamID(SteamUser.GetSteamID().m_SteamID);
    }

    [Command]
    public void CmdSetSteamID(ulong id)
    {
        steamID = id; // Isso sincroniza para todos os clientes automaticamente
    }

    public override void OnStartClient()
    {
        avatarILoad = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
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
        profileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        return texture;
    }
}
