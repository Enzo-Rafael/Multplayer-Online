using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections;
public class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bullet = null;
    [SerializeField] private Transform bulletSpawnPoint;
    public float bulletSpeed = 10;
    private bool canShoot = true;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;

    [ClientCallback]
    private void Update()
    {
        if (!authority) { return; }
        if (Mouse.current.leftButton.isPressed)
        {
            if (canShoot == true)
            {
                canShoot = false;
                CmdSpawnBullet();
                StartCoroutine(Wait());
                
            }
        }

    }
    [Command]
    void CmdSpawnBullet()
    {
        GameObject bulletClone = Instantiate(bullet, bulletSpawnPoint.transform.position, transform.rotation);
        bulletClone.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.transform.up * bulletSpeed;
        NetworkServer.Spawn(bulletClone);
    }
    
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        canShoot = true;
    }
}
