using UnityEngine;

public class LaserBolt : MonoBehaviour
{
    public Transform laserSpawnPoint;
    public GameObject projectilePrefab;
    public float velocity = 1000f;
    public float mass = .5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject projectile = Instantiate(projectilePrefab, laserSpawnPoint.transform);
            Rigidbody bullet = projectile.GetComponent<Rigidbody>();
            bullet.AddForce(laserSpawnPoint.forward * velocity, ForceMode.Impulse);
            bullet.mass = mass;
        }
    }
}