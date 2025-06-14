using UnityEngine;
using Mirror;

public class bullet : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float lifetime = 3f;
    public int playerIndex; // 0 = P1, 1 = P2

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            if (playerIndex == 1) GameManager.Instance.AddPoints(1);
        }

        if (other.CompareTag("Player2"))
        {
            if (playerIndex == 0) GameManager.Instance.AddPoints(0);
        }

        NetworkServer.Destroy(gameObject);
    }
}