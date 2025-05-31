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
        Destroy(collision.gameObject);
        Destroy(gameObject);
    }
}
