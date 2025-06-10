using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
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
    //Variaveis para os indices e posicoes pra seicronização
    [Header("References")]
    public Transform[] startPosition;
    public Transform[] players;
    [NonSerialized] public CSteamID steamIdGM;
    private Dictionary<UIMenuType, GameObject> menuDict = new(); 
    [SyncVar] public int charIndex;

    [Header("Settings")]
    [SyncVar]public bool player01 = false;
    [SyncVar]public bool player02 = false;
    [SyncVar]public int player1Pontos = 0;
    [SyncVar]public int player2Pontos = 0;
    [SyncVar]public bool timerActive = false;
    [SyncVar]public float matchTime = 0f;
    [SyncVar]public float startTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
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
        if (isClient && timerActive)
        {
            if (menuDict.TryGetValue(UIMenuType.TimerText, out var timerText))
            {
                TimeSpan time = TimeSpan.FromSeconds(matchTime);
                timerText.GetComponent<Text>().text = $"{time.Minutes:D2}:{time.Seconds:D2}";
            }
        }
        matchTime = matchTime - Time.deltaTime;
        RpcMatchEnded(matchTime);
    }
    [ClientRpc]
    void RpcMatchEnded(float match)
    {
        TimeSpan time = TimeSpan.FromSeconds(match);
        if (menuDict.TryGetValue(UIMenuType.TimerText, out var timerText))
        {
            timerText.GetComponent<Text>().text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }
        CanvasUpdate();
    }
    //-------------------------------------------------------Client-Area-------------------------------------------------------------
    [Client]
    public IEnumerator SetCanvasSafe()
    {
        yield return new WaitForSeconds(0.1f); // ou até WaitUntil(() => GameObject.FindWithTag(...) != null)
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
    public void ShowPoints()//Serve para alterar os textos relacionados a pontuação
    {
        if (menuDict.TryGetValue(UIMenuType.ScoreP1, out var p1))
            p1.GetComponent<Text>().text = $"P1: {player1Pontos}";

        if (menuDict.TryGetValue(UIMenuType.ScoreP2, out var p2))
            p2.GetComponent<Text>().text = $"P2: {player2Pontos}";
    }

    [Client]
    public void PosicionAjust()//Serve para ajustar as posições dos jogadores
    {
        if (isOwned && GameObject.FindGameObjectWithTag("Player1") == true)
        {
            GameObject.FindGameObjectWithTag("Player1").transform.position = startPosition[0].position;
            GameObject.FindGameObjectWithTag("Player1").transform.rotation = startPosition[0].rotation;
        }
        if (isOwned && GameObject.FindGameObjectWithTag("Player2") == true)
        {
            GameObject.FindGameObjectWithTag("Player2").transform.position = startPosition[1].position;
            GameObject.FindGameObjectWithTag("Player2").transform.rotation = startPosition[1].rotation;
        }
    }

    [Client]
    public void CanvasUpdate()//Serve para alterar os textos dos canvas e rodar os cronometros
    {
         if (matchTime > 0) return;

        timerActive = false;

        ToggleMenu(UIMenuType.WinPanel, true);

        string result = "EMPATE";
        if (player1Pontos > player2Pontos) result = "Parabéns: P1";
        else if (player2Pontos > player1Pontos) result = "Parabéns: P2";

        if (menuDict.TryGetValue(UIMenuType.WinPanel, out var winPanel))
        {
            winPanel.GetComponentInChildren<Text>().text = result;
        }

        ToggleMenu(UIMenuType.StartButton, true);
    }
    //-------------------------------------------------------Server-Area-------------------------------------------------------------
    [Server]
    public void ActiveTimer()//Server para ativar o cronometro
    {
        timerActive = true;
    }

    [Server]
    public void CheckCharactersDisponibility()//Controle de qual jogador vai ser qual
    {
        player01 = false;
        player02 = false;
        foreach (var conn in NetworkServer.connections)
        {
            if (conn.Value.identity != null)
            {
                var player = conn.Value.identity.GetComponent<Player>();
                if (player != null)
                {
                    if (player.characterType == 1) player01 = true;
                    if (player.characterType == 2) player02 = true;
                }
            }
        }
    }

    [Server]
    public void SetIndexCurrent(int index)//Serve para ajustar o index do personagem atalmente selecionado
    {
        charIndex = index;
    }

    [Server]
    public void ResetMatch()//Serve para resetar a partida
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
        ToggleMenu(UIMenuType.WinPanel, false);// Oculta resultado
        ToggleMenu(UIMenuType.StartButton, false);// Oculta botão "Reiniciar" 
    }

    [Server]
    public void SetSpawnPos(Transform[] positions)//Serve para setar as posições iniciais dos players
    {
        startPosition = positions;
    }

    [Server]
    public void AddPoints(int index)//Serve para alterar os valores relacionados a pontuação
    {
        Debug.Log("Bullet");
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
    [Client]
    public void ShowStart()
    {
        if (isServer) ToggleMenu(UIMenuType.StartButton, true);
    }
    /*----------------------Target-Area--------------------------------------------------------------------------------*/
    [TargetRpc]
    public void TargetSyncState(NetworkConnection target)//Quando um jogador entra/reentra essa função é chamada
    {
        ShowPoints();
        CanvasUpdate();
        if (timerActive)
        {
            ActiveMenus();
        }
    }
    /*-------------------------------------------Command-Area-------------------------------------------------------------*/
    [Command(requiresAuthority = false)]
    public void CmdRequestReset()
    {
        if(isServer)ResetMatch();
    }
}
