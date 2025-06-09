using UnityEngine;
using Steamworks; 

public class SteamManagerCallbacks : MonoBehaviour
{
    void Update()
    {
        if (SteamManager.Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

}
