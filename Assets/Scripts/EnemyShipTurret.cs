using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipTurret : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float velocity;
    public float mass;
    public float fireRate = 0.5f;
    public float timer = 0;
    public GameObject playerShip;
    public Vector3 playerShipPosition;

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;
        timer = fireRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerShip == null) // Deactivate all turrets if player ship has been destroyed
            enabled = false;

        playerShipPosition = playerShip.transform.position;
        
        if (timer >= fireRate)
        {
            timer = 0;
            GameObject projectile = Instantiate(projectilePrefab, transform);
            projectile.transform.LookAt(playerShipPosition);
            Rigidbody bullet = projectile.GetComponent<Rigidbody>();
            bullet.AddForce(projectile.transform.forward * velocity, ForceMode.Impulse);
            bullet.mass = mass;
        }

        timer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerShip")
        {
            enabled = true;
            playerShip = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PlayerShip")
            enabled = false;
    }
}
