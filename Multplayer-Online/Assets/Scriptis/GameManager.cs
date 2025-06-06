using Mirror;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    //Variaveis para os indices e posicoes pra seicronização
    [Header("References")]
    public GameObject[] menu;
    public Transform[] startPosition;
    public Transform[] players;
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
    private void Update()
    {
        if (timerActive == true)
        {
            matchTime = matchTime - Time.deltaTime;
            CanvasUpdate(matchTime);
        }

    }
    //-------------------------------------------------------Client-Area-------------------------------------------------------------
    [Client]
    public void ActiveMneu()//Serve para ativar parte do canvas
    {
        menu[3].SetActive(true);
        menu[4].SetActive(true);
    }
  
    [Client]
    public void DesActiveMneu()//Serve para desativar parte do canvas
    {
        menu[3].SetActive(false);
        //menu[4].SetActive(false);
        menu[5].SetActive(false);
        menu[6].SetActive(false);
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
        menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
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
        TimeSpan time = TimeSpan.FromSeconds(match);
        menu[0].GetComponent<Text>().text = time.Minutes.ToString() + " : " + time.Seconds.ToString();
        if (matchTime <= 0)
        {
            timerActive = false;
            menu[5].SetActive(true);
            if (player1Pontos > player2Pontos)
            {
                timerActive = false;
                menu[5].GetComponentInChildren<Text>().text = "Parabens: P1";
            }
            else if (player2Pontos > player1Pontos)
            {
                timerActive = false;
                menu[5].GetComponentInChildren<Text>().text = "Parabens: P2";
            }
            else
            {
                timerActive = false;
                menu[5].GetComponentInChildren<Text>().text = "EMPATE";
            }
            menu[6].SetActive(true);
        }
        ShowPoints();
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
        if (GameObject.FindGameObjectWithTag("Player1") == false)
        {
            player01 = false;
        }
        if (GameObject.FindGameObjectWithTag("Player2") == false)
        {
            player02 = false;
        }
    }

    [Server]
    public void SetIndexCurrent(int index)//Serve para ajustar o index do personagem atalmente selecionado
    {
        charIndex = index;
    }

    [Server]
    public void ResetMach()//Serve para resetar a partida
    {
        menu[5].SetActive(false);
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
        PosicionAjust();
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
        ShowPoints();
    }
    [Server]
    public void ShowStart()
    {
        menu[6].SetActive(true);
    }
}
