using UnityEngine;
using Mirror;

public class bullet : NetworkBehaviour
{
    public float damage = 3;
    void Awake()
    {
        Destroy(gameObject, damage);
    }
 
    void OnCollisionEnter(Collision collision)
    { 
       
        if (collision.gameObject.CompareTag("Player1"))
        {
            GameManager.Instance.player1Pontos++;
            Destroy(gameObject);
        }else
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Player2"))
        {
            //
            GameManager.Instance.player2Pontos++;
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
}
