using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    [SerializeField] GameObject bulletImpactPrefab;
    int damage = 1;
    
    float bulletSpeed = 60f;
    float physicsHitMultiplier = 1f;

    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        transform.position += transform.forward * bulletSpeed * Time.deltaTime;

        float delta = Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, bulletSpeed * delta);
        if (hit.collider == null)
            return;
        Hit(hit);
    }

    private void Hit (RaycastHit2D hit)
    {
        //get health, do damage
        Rigidbody2D r = hit.collider.gameObject.GetComponent<Rigidbody2D>();
        if (r != null)
            r.AddForceAtPosition(transform.forward * (bulletSpeed * physicsHitMultiplier), hit.point);

        GameObject bulletHit = Instantiate(bulletImpactPrefab, hit.point, Quaternion.Euler(-hit.normal));
        //Destroy(bulletHit, 3f);

        Destroy(gameObject);
    }
}
