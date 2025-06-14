using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class Gun : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 10;

    public void OnFire(GameObject bullet)
    {
        if (!authority) return;
        var bulletS = Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * bulletSpeed;
    }
}
