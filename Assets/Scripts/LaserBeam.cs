using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public GameObject laserBeamPrefab;
    public GameObject laserSpawnPoint;
    public GameObject deInstancingArea;
    public float force;
    public float range = 500;

    private GameObject spawnedLaser;
    private RaycastHit hitPoint;

    // Start is called before the first frame update
    void Start()
    {
        spawnedLaser = Instantiate(laserBeamPrefab, laserSpawnPoint.transform) as GameObject;
        spawnedLaser.SetActive(false);
        spawnedLaser.transform.localScale = new Vector3(spawnedLaser.transform.localScale.x, spawnedLaser.transform.localScale.y, range);
        deInstancingArea.transform.localScale = new Vector3(deInstancingArea.transform.localScale.x, deInstancingArea.transform.localScale.y, deInstancingArea.transform.localPosition.z + range);
        deInstancingArea.transform.localPosition = new Vector3(deInstancingArea.transform.localPosition.x, deInstancingArea.transform.localPosition.y, deInstancingArea.transform.localPosition.z + range / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            spawnedLaser.SetActive(true);
            if (Physics.Raycast(spawnedLaser.transform.position, spawnedLaser.transform.forward, out hitPoint, range))
                hitPoint.rigidbody.AddForceAtPosition(spawnedLaser.transform.forward * force, hitPoint.point, ForceMode.Impulse);
        }
        if (Input.GetMouseButton(1))
        {
            if (laserSpawnPoint != null)
                spawnedLaser.transform.position = laserSpawnPoint.transform.position;
            if (Physics.Raycast(spawnedLaser.transform.position, spawnedLaser.transform.forward, out hitPoint, range))
                hitPoint.rigidbody.AddForceAtPosition(spawnedLaser.transform.forward * force, hitPoint.point, ForceMode.Impulse);
        }
        if (Input.GetMouseButtonUp(1))
            spawnedLaser.SetActive(false);
    }
}
