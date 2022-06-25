using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : DestructibleCleanup
{
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
