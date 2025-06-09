using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;
using Mirror.Examples.Common.Controllers.Player;


public class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bullet = null;   //GameObject do projetil disparado pelo player
    [SerializeField] private Transform bulletSpawnPoint; //Posição onde sera instanciado o projetil
    [SerializeField] private GameObject myCam = default;  //Camera do player
    [Header("Settings")]
    public int indexPlayer;
    public float bulletSpeed = 10; //Velocidade base do projetil
    private bool canShoot = true;  //Limitador dos tiros
    public int characterType;

    [ClientCallback]
    private void Update()
    {
        if (!isOwned) { return; }
        if (Mouse.current.leftButton.isPressed && GameManager.Instance.timerActive == true)//Disparo
        {
            if (canShoot == true)
            {
                canShoot = false;
                CmdSpawnBullet();
                StartCoroutine(Wait());

            }
        }

    }
    [Client]
    public void Awake()
    {
        if (!authority) { return; }
        //GameManager.Instance.ActiveMneu();
    }
    public override void OnStartAuthority()
    {
        myCam.SetActive(true);
    }
   
    [Command]
    void CmdSpawnBullet() //comando que instncia o projétil
    {
        GameObject bulletClone = Instantiate(bullet, bulletSpawnPoint.transform.position, transform.rotation);
        bulletClone.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.transform.up * bulletSpeed;
        NetworkServer.Spawn(bulletClone);
    }

    IEnumerator Wait()//Cooldown para o proximo disparo
    {
        yield return new WaitForSeconds(1f);
        canShoot = true;
    }
}
