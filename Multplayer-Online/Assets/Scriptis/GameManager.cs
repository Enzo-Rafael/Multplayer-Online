using Mirror;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System.Collections;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    //Variaveis para os indices e posicoes pra seicronização
    [Header("References")]
    public GameObject[] menu;
    public Transform[] startPosition;
    public Transform[] players;
    [NonSerialized] public CSteamID steamIdGM;
    [SyncVar]
    public int charIndex;

    [Header("Settings")]
    [SyncVar]
    public bool player01 = false;
    [SyncVar]
    public bool player02 = false;
    [SyncVar]
    public int player1Pontos = 0;
    [SyncVar]//(hook = nameof(AddPoints))
    public int player2Pontos = 0;
    [SyncVar]
    public bool timerActive = false;
    [SyncVar]
    public float matchTime = 0f;
    [SyncVar]
    public float startTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        startPosition = NetworkManager.startPositions.ToArray();
        menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
    }
    [ServerCallback] 
    private void Update()
    {
        if (!isServer || !timerActive) return;
        matchTime = matchTime - Time.deltaTime; 
        RpcMatchEnded(matchTime);
        

    }
    [ClientRpc]
    void RpcMatchEnded(float match)
    {
    TimeSpan time = TimeSpan.FromSeconds(match);
    if (menu[0] != null)menu[0].GetComponent<Text>().text = time.Minutes.ToString() + " : " + time.Seconds.ToString();
    CanvasUpdate(0);
    }
    //-------------------------------------------------------Client-Area-------------------------------------------------------------
    [Client]
    public void ActiveMneu()//Serve para ativar parte do canvas
    {
        if (menu[3] != null)menu[3].SetActive(true);
        if (menu[4] != null)menu[4].SetActive(true);
    }

    [Client]
    public void DesActiveMneu()//Serve para desativar parte do canvas
    {
        //menu[3].SetActive(false);
        //menu[4].SetActive(false);
        if (menu[5] != null) menu[5].SetActive(false);
        if (menu[6] != null) menu[6].SetActive(false);
        if(isServer)ShowStart();
    }
    [Client]
    public IEnumerator SetCanvasSafe()
    {
    yield return new WaitForSeconds(0.1f); // ou até WaitUntil(() => GameObject.FindWithTag(...) != null)
    SetCanvas();
    }
    [Client]
    public void SetCanvas()//Serve para setar as referencias
    {
        menu[0] = GameObject.FindGameObjectWithTag("M1");
        menu[1] = GameObject.FindGameObjectWithTag("M2");
        menu[2] = GameObject.FindGameObjectWithTag("M3");
        menu[3] = GameObject.FindGameObjectWithTag("M4");
        menu[4] = GameObject.FindGameObjectWithTag("M5");
        menu[5] = GameObject.FindGameObjectWithTag("M6");
        menu[6] = GameObject.FindGameObjectWithTag("M7");
        DesActiveMneu();
    }

    [Client]
    public void ShowPoints()//Serve para alterar os textos relacionados a pontuação
    {
        if (menu[1] != null)menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        if (menu[2] != null)menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
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
    public void CanvasUpdate(float match)//Serve para alterar os textos dos canvas e rodar os cronometros
    {
        if (matchTime <= 0)
        {
            timerActive = false;
            if (menu[5] != null)menu[5].SetActive(true);
            if (player1Pontos > player2Pontos)
            {
                timerActive = false;
                if (menu[5] != null)menu[5].GetComponentInChildren<Text>().text = "Parabens: P1";
            }
            else if (player2Pontos > player1Pontos)
            {
                timerActive = false;
                if (menu[5] != null)menu[5].GetComponentInChildren<Text>().text = "Parabens: P2";
            }
            else
            {
                timerActive = false;
                if (menu[5] != null)menu[5].GetComponentInChildren<Text>().text = "EMPATE";
            }
            if (menu[6] != null)menu[6].SetActive(true);
        }
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
        /*if (GameObject.FindGameObjectWithTag("Player1") == false)
        {

        }
        if (GameObject.FindGameObjectWithTag("Player2") == false)
        {
            
        }*/
    }

    [Server]
    public void SetIndexCurrent(int index)//Serve para ajustar o index do personagem atalmente selecionado
    {
        charIndex = index;
    }

    [Server]
    public void ResetMach()//Serve para resetar a partida
    {
        if (menu[5] != null)menu[5].SetActive(false);
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        if (menu[1] != null)menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        if (menu[2] != null)menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
        //PosicionAjust();
        ActiveTimer();
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
        if (menu[6] != null && isServer)menu[6].SetActive(true);
    }
}
