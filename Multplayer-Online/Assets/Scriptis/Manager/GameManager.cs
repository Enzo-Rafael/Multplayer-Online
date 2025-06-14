using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public enum UIMenuType
    {
        TimerText,
        ScoreP1,
        ScoreP2,
        GameplayPanel,
        HUD,
        WinPanel,
        StartButton
    }

    [Header("References")]
    public Transform[] startPosition;
    public Transform[] players;
    public CSteamID steamIdGM;
    private Dictionary<UIMenuType, GameObject> menuDict = new();

    [Header("Settings")]
    [SyncVar] public bool player01 = false;
    [SyncVar] public bool player02 = false;
    [SyncVar] public int player1Pontos = 0;
    [SyncVar] public int player2Pontos = 0;
    [SyncVar] public bool timerActive = false;
    [SyncVar] public float matchTime = 0f;
    [SyncVar] public float startTime = 3f;
    [SyncVar] public int charIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        startPosition = NetworkManager.startPositions.ToArray();
    }

    [ServerCallback]
    private void Update()
    {
        if (!isServer || !timerActive) return;

        matchTime -= Time.deltaTime;
        RpcUpdateTimer(matchTime);

        if (matchTime <= 0)
        {
            timerActive = false;
            RpcMatchEnded();
        }
    }

    [ClientRpc]
    void RpcUpdateTimer(float timeLeft)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeLeft);
        if (menuDict.TryGetValue(UIMenuType.TimerText, out var timerText))
        {
            timerText.GetComponent<Text>().text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }

    [ClientRpc]
    void RpcMatchEnded()
    {
        string result = "EMPATE";
        if (player1Pontos > player2Pontos) result = "Parabéns: P1";
        else if (player2Pontos > player1Pontos) result = "Parabéns: P2";

        ToggleMenu(UIMenuType.WinPanel, true);

        if (menuDict.TryGetValue(UIMenuType.WinPanel, out var winPanel))
        {
            winPanel.GetComponentInChildren<Text>().text = result;
        }

        ToggleMenu(UIMenuType.StartButton, true);
    }

    // ----------------- UI ----------------------

    [Client]
    public IEnumerator SetCanvasSafe()
    {
        yield return new WaitForSeconds(0.1f);
        InitializeMenus();
    }

    [Client]
    public void InitializeMenus()
    {
        menuDict[UIMenuType.TimerText] = GameObject.FindGameObjectWithTag("M1");
        menuDict[UIMenuType.ScoreP1] = GameObject.FindGameObjectWithTag("M2");
        menuDict[UIMenuType.ScoreP2] = GameObject.FindGameObjectWithTag("M3");
        menuDict[UIMenuType.GameplayPanel] = GameObject.FindGameObjectWithTag("M4");
        menuDict[UIMenuType.HUD] = GameObject.FindGameObjectWithTag("M5");
        menuDict[UIMenuType.WinPanel] = GameObject.FindGameObjectWithTag("M6");
        menuDict[UIMenuType.StartButton] = GameObject.FindGameObjectWithTag("M7");

        DesactiveMenus();
    }

    [Client]
    public void ToggleMenu(UIMenuType type, bool active)
    {
        if (menuDict.TryGetValue(type, out var go))
        {
            go.SetActive(active);
        }
    }

    [Client]
    public void ActiveMenus()
    {
        ToggleMenu(UIMenuType.GameplayPanel, true);
        ToggleMenu(UIMenuType.HUD, true);
    }

    [Client]
    public void DesactiveMenus()
    {
        ToggleMenu(UIMenuType.GameplayPanel, false);
        ToggleMenu(UIMenuType.HUD, false);
        ToggleMenu(UIMenuType.WinPanel, false);
        ToggleMenu(UIMenuType.StartButton, false);
    }

    [Client]
    public void ShowPoints()
    {
        if (menuDict.TryGetValue(UIMenuType.ScoreP1, out var p1))
            p1.GetComponent<Text>().text = $"P1: {player1Pontos}";

        if (menuDict.TryGetValue(UIMenuType.ScoreP2, out var p2))
            p2.GetComponent<Text>().text = $"P2: {player2Pontos}";
    }

    // ----------------- Server Area ----------------------

    [Server]
    public void ActiveTimer()
    {
        timerActive = true;
    }

    [Server]
    public void UpdatePlayerSlots()
    {
        player01 = GameObject.FindWithTag("Player1") != null;
        player02 = GameObject.FindWithTag("Player2") != null;
    }

    [Server]
    public void SetIndexCurrent(int index)
    {
        charIndex = index;
    }

    [Server]
    public void ResetMatch()
    {
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        timerActive = true;

        RpcResetCanvas();
    }

    [ClientRpc]
    void RpcResetCanvas()
    {
        ShowPoints();
        ToggleMenu(UIMenuType.WinPanel, false);
        ToggleMenu(UIMenuType.StartButton, false);
    }

    [Server]
    public void AddPoints(int index)
    {
        if (index == 0) player1Pontos++;
        if (index == 1) player2Pontos++;

        RpcUpdatePoints(player1Pontos, player2Pontos);
    }

    [ClientRpc]
    void RpcUpdatePoints(int p1, int p2)
    {
        player1Pontos = p1;
        player2Pontos = p2;
        ShowPoints();
    }

    // ----------------- Sync ----------------------

    [TargetRpc]
    public void TargetSyncState(NetworkConnection target)
    {
        ShowPoints();
        if (timerActive)
        {
            ActiveMenus();
        }
    }

    // ----------------- Command ----------------------

    [Command(requiresAuthority = false)]
    public void CmdRequestReset()
    {
        if (isServer) ResetMatch();
    }

    [Client]
    public void ShowStart()
    {
        if (isServer) ToggleMenu(UIMenuType.StartButton, true);
    }
}