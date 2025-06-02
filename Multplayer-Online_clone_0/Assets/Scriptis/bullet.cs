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
        if (collision.gameObject.layer == 6)
        {
            GameManager.Instance.AddPoints(1, 1);
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == 7)
        {
            GameManager.Instance.AddPoints(1, 2);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
}
