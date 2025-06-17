using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ResetMetch : MonoBehaviour
{
    private Button startButton;

    private void Awake()
    {
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnEnable()
    {
        // Só deixa o botão visível se for Host (Server ativo e Client ativo)
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnStartClicked()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Tentou iniciar, mas não é Host.");
            return;
        }

        GameManager.Instance?.RequestReset();
        UIManager.Instance?.HideMatchResult();
    }
}
