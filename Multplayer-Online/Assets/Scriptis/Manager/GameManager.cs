using Mirror;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;


    [Header("References")]
    public Transform[] startPosition;
    public Transform[] players;
    public CSteamID steamIdGM;
    public Character[] characters;

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
            DontDestroyOnLoad(gameObject);
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
        UIManager.Instance?.UpdateTimerText(matchTime);
        UIManager.Instance?.UpdateScore(player1Pontos,player2Pontos);
    }

    [ServerCallback]
    private void Update()
    {
        if (!isServer) return;
        if(!timerActive) return;
        
        matchTime -= Time.deltaTime;
        RpcUpdateTimer(matchTime);

        if (matchTime <= 0)
        {
            timerActive = false;
            RpcMatchEnded(player1Pontos, player2Pontos);
            Debug.Log($"Timer ativo: {timerActive}, MatchTime: {matchTime}");
        }
    }
    // ----------------- Client Area ---------------------

    public override void OnStartClient()
    {
        base.OnStartClient();
        //UIManager.Instance?.InitializeMenus();

        if (timerActive)
        {
            UIManager.Instance.ActiveMenus();
        }
        else
        {
            UIManager.Instance.DesactiveMenus();
        }
    }
    [Client]
    void RpcUpdateTimer(float timeLeft)//atualiza a ui do cronometro
    {
        UIManager.Instance?.UpdateTimerText(timeLeft);
    }

    [Client]
    void RpcMatchEnded(int p1, int p2)//condição de vitoria e derrota
    {
        string result = "EMPATE";
        if (player1Pontos > player2Pontos) result = "Parabéns: P1";
        else if (player2Pontos > player1Pontos) result = "Parabéns: P2";

        UIManager.Instance?.ShowMatchResult(result);
        UIManager.Instance?.ShowStart();
    }
    [Client]
    void ResetCanvas()
    {
        UIManager.Instance?.UpdateScore(player1Pontos, player2Pontos);
        UIManager.Instance?.HideMatchResult();
        UIManager.Instance?.startButton.SetActive(false);
    }
    
    [Client]
    public void ShowStart()//Mostrar o btn de startar a partida
    {
        if (isServer) UIManager.Instance.ShowStart();
    }
    // ----------------- Server Area ----------------------

    [Server]
    public void ActiveTimer()//ativa o cronometro da partida
    {
        timerActive = true;
    }

    [Server]
    public void UpdatePlayerSlots()//Atualiza os personagens disponiveis
    {
        player01 = GameObject.FindWithTag("Player1") != null;
        player02 = GameObject.FindWithTag("Player2") != null;
    }

    [Server]
    public void SetIndexCurrent(int index)//ajusta o atual id do caracter a mostra
    {
        charIndex = index;
    }

    [Server]
    public void ResetMatch()//Reseta a partida
    {
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        timerActive = false;

        ResetCanvas();
        RpcUpdatePoints(player1Pontos, player2Pontos);
        RpcUpdateTimer(matchTime);
        UIManager.Instance?.HideMatchResult();
    }

    [Server]
    public void AddPoints(int index)//Adiciona os pontos feitos
    {
        if (index == 0) player1Pontos++;
        if (index == 1) player2Pontos++;

        RpcUpdatePoints(player1Pontos, player2Pontos);
    }

    [Client]
    void RpcUpdatePoints(int p1, int p2)//Altera a hud dos pontos
    {
        UIManager.Instance?.HideMatchResult();
        UIManager.Instance?.UpdateScore(player1Pontos, player2Pontos);
    }

    // ----------------- Sync ----------------------

    [TargetRpc]
    public void TargetSyncState(NetworkConnectionToClient target)//Synca a UI
    {
        UIManager.Instance?.UpdateScore(player1Pontos, player2Pontos);

        if (timerActive)
        {
            UIManager.Instance?.ActiveMenus();
        }
        else
        {
            UIManager.Instance?.DesactiveMenus();
        }
    }

    // ----------------- Command ----------------------

    [Server]
    public void RequestReset()//Reseta a partida
    {
        if (!isServer) return;

        ResetMatch();
        ActiveTimer();
    }
}