using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [Header("Settings")]
    [SyncVar] public int characterType;
    public int indexPlayer;
    public float bulletSpeed = 10; //Velocidade base do projetil
    private bool canShoot = true;
    [Header("References")]
    [SerializeField] private GameObject bullet = null;   //GameObject do projetil disparado pelo player
    [SerializeField] private Transform bulletSpawnPoint; //Posição onde sera instanciado o projetil
    [SerializeField] private GameObject myCam = default;  //Camera do player

    public override void OnStartAuthority()
    {
        // Ativa a câmera e controles apenas para o jogador local
        GetComponent<PlayerMuve>().enabled = true;
        GetComponent<PlayerName>().enabled = true;
        myCam.SetActive(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.Instance.UpdatePlayerSlots();
    }

    public override void OnStopClient()
    {
        GameManager.Instance.UpdatePlayerSlots();
        base.OnStopClient();
    }
    
    [ClientCallback]
    private void Update()
    {
        if (!isOwned || !NetworkClient.isConnected || !NetworkClient.ready) { return; }
        if (Mouse.current.leftButton.isPressed && GameManager.Instance.timerActive == true)//Disparo
        {
            if (canShoot == true)
            {
                canShoot = false;
                
                StartCoroutine(Wait());

            }
        }

    }
    /*[Client]
    public void Awake()
    {
        if (!authority) { return; }
        //GameManager.Instance.ActiveMneu();
    }*/
   
    /*[Command]
    void CmdSpawnBullet() //comando que instncia o projétil
    {
        GameObject bulletClone = Instantiate(bullet, bulletSpawnPoint.transform.position, transform.rotation);
        bulletClone.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.transform.up * bulletSpeed;
        NetworkServer.Spawn(bulletClone);
    }*/

    IEnumerator Wait()//Cooldown para o proximo disparo
    {
        GameObject bulletClone = Instantiate(bullet, bulletSpawnPoint.transform.position, transform.rotation);
        bulletClone.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.transform.up * bulletSpeed;
        NetworkServer.Spawn(bulletClone);
        yield return new WaitForSeconds(1f);
        canShoot = true;
    }
}
