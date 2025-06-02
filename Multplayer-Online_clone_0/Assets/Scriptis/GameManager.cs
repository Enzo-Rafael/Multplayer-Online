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
    [SyncVar(hook = nameof(AddPoints))]
    public int player1Pontos = 0;
    [SyncVar(hook = nameof(AddPoints))]
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
    }

    private void Update()
    {
        if (isServer)
        {
            if (timerActive == true)
            {
                matchTime = matchTime - Time.deltaTime;
                TimeSpan time = TimeSpan.FromSeconds(matchTime);
                menu[0].GetComponent<Text>().text = time.Minutes.ToString() + " : " + time.Seconds.ToString();
                if (matchTime <= 0)
                {
                    timerActive = false;
                    menu[5].SetActive(true);
                    if (player1Pontos > player2Pontos)
                    {
                        menu[5].GetComponent<Text>().text = "Parabens: P1";
                    }
                    else if (player2Pontos > player1Pontos)
                    {
                        menu[5].GetComponent<Text>().text = "Parabens: P2";
                    }
                    else
                    {
                        menu[5].GetComponent<Text>().text = "EMPATE";
                    }
                    menu[6].SetActive(true);
                }
            }
        }
    }

    [Server]
    public void ActiveTimer()
    {
        timerActive = true;
    }

    [Client]
    public void ActiveMneu()
    {
        menu[3].SetActive(true);
        menu[4].SetActive(true);
        menu[6].SetActive(true);
    }

    //[Server]
    public void DesActiveMneu()
    {
        menu[3].SetActive(false);
        menu[4].SetActive(false);
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
    public void SetIndexCurrent(int index)
    {
        charIndex = index;
    }

    [Client]
    public void AddPoints(int points, int indexPlayer)//Serve para adicionar os pontos 
    {
        if (indexPlayer == 1)
        {
            player1Pontos += points;
            menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        }
        if (indexPlayer == 2)
        {
            player2Pontos += points;
            menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
        }
    }
    [Server]
    public void RestMach()
    {
        menu[5].SetActive(false);
        matchTime = startTime * 60;
        player1Pontos = 0;
        player2Pontos = 0;
        menu[1].GetComponent<Text>().text = "P1: " + player1Pontos;
        menu[2].GetComponent<Text>().text = "P2: " + player2Pontos;
        ActiveTimer();
    
    }
    [Server]
    public void SetSpawnPos(Transform[] positions)
    {
        startPosition = positions;
    }
}
