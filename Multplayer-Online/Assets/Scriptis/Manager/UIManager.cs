using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    /*public enum UIMenuType
    {
        TimerText,
        ScoreP1,
        ScoreP2,
        GameplayPanel,
        HUD,
        WinPanel,
        StartButton
    }

    private Dictionary<UIMenuType, GameObject> menuDict = new();*/
    //Referencias setadas de forma mais bruta
    [Header("UI Elements")]
    public GameObject gameplayPanel;
    public GameObject hudPanel;
    public GameObject winPanel;
    public GameObject startButton;
    public Text timerText;
    public Text scoreP1;
    public Text scoreP2;
    public Text resultText;

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

    /* public IEnumerator SetCanvasSafe()//Chamada de inicialização sergura dos menus
    {
        yield return new WaitForSeconds(0.1f);
        InitializeMenus();
    }
    
    public void InitializeMenus()//Registra a UI
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

    public void ToggleMenu(UIMenuType type, bool active)//Controle da UI
    {
        if (menuDict.TryGetValue(type, out var go))
        {
            go.SetActive(active);
        }
    }*/

    public void ActiveMenus()//Ativa parte da UI
    {
        //ToggleMenu(UIMenuType.GameplayPanel, true);
        // ToggleMenu(UIMenuType.HUD, true);
        gameplayPanel.SetActive(true);
        hudPanel.SetActive(true);
        
    }

    public void DesactiveMenus()//Desativa parte da UI
    {
        /*ToggleMenu(UIMenuType.GameplayPanel, false);
        ToggleMenu(UIMenuType.HUD, false);
        ToggleMenu(UIMenuType.WinPanel, false);
        ToggleMenu(UIMenuType.StartButton, false);*/
        gameplayPanel.SetActive(false);
        hudPanel.SetActive(false);
        winPanel.SetActive(false);
        startButton.SetActive(false);
    }

    public void UpdateTimerText(float timeLeft)//Atualiza UI do cronometro
    {
        /*if (menuDict.TryGetValue(UIMenuType.TimerText, out var timerText))
        {
            TimeSpan time = TimeSpan.FromSeconds(timeLeft);
            timerText.GetComponent<Text>().text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }*/
        TimeSpan time = TimeSpan.FromSeconds(timeLeft);
        timerText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
    }

    public void UpdateScore(int p1, int p2)//Atualiza UI dos pontos
    {
        //if (menuDict.TryGetValue(UIMenuType.ScoreP1, out var scoreP1))
            scoreP1.text = $"P1: {p1}";

        //if (menuDict.TryGetValue(UIMenuType.ScoreP2, out var scoreP2))
            scoreP2.text = $"P2: {p2}";
    }

    public void ShowMatchResult(string result)//Mostra na ui o resultado
    {
        //ToggleMenu(UIMenuType.WinPanel, true);
        winPanel.SetActive(true);
        /*if (menuDict.TryGetValue(UIMenuType.WinPanel, out var winPanel))
        {
            
        }*/
        winPanel.GetComponentInChildren<Text>().text = result;

        //ToggleMenu(UIMenuType.StartButton, true);
        startButton.SetActive(true);
    }

    public void HideMatchResult()//Desativa a UI do resultado e Btn de restart
    {
        //ToggleMenu(UIMenuType.WinPanel, false);
        //ToggleMenu(UIMenuType.StartButton, false);
        winPanel.SetActive(false);
        startButton.SetActive(false);
    }

    public void ShowStart()//Mostra o btn de restart
    {
        //ToggleMenu(UIMenuType.StartButton, true);
        startButton.SetActive(true);
    }
}

