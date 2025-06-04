using UnityEngine;
using Mirror;

public class bullet : NetworkBehaviour
{
    public float damage = 3;
    void Awake()
    {
        Destroy(gameObject, damage);
    }
    [Server]
    void OnCollisionEnter(Collision collision)
    {
        if (!authority)
        {
            Destroy(gameObject);
            return;
        } 
        
        if ( collision.gameObject.CompareTag("Player1")) GameManager.Instance.AddPoints(0);
        if (collision.gameObject.CompareTag("Player2"))GameManager.Instance.AddPoints(1);
        Destroy(gameObject);

    }
}
