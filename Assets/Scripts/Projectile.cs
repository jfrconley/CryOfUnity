using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    [SerializeField] GameObject bulletImpactPrefab;
    int damage = 1;

    public string BulletId;
    public string PlayerId;
    private PlayerGunControl _gunControl;
    
    float bulletSpeed = 60f;
    float physicsHitMultiplier = 1f;

    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    public void SetBulletVars(string type, string bulletId, string playerId, PlayerGunControl gunControl)
    {
        _gunControl = gunControl;
        GunSettings gunInfo = GunLocker.singleton.GetGunInfo(type);
        BulletId = bulletId;
        PlayerId = playerId;
        damage = gunInfo.damage;
        bulletSpeed = gunInfo.bulletSpeed;
        physicsHitMultiplier = gunInfo.physicsHitMultiplier;
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        transform.position += transform.forward * bulletSpeed * delta;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, bulletSpeed * delta))
        {
            Hit(hit);
        }
    }

    private void Hit (RaycastHit hit)
    {
        //get health, do damage
        Rigidbody r = hit.collider.gameObject.GetComponent<Rigidbody>();
        if (r != null)
            r.AddForceAtPosition(transform.forward * (bulletSpeed * physicsHitMultiplier), hit.point);

        GameObject bulletHit = Instantiate(bulletImpactPrefab, hit.point, Quaternion.identity);
        bulletHit.transform.forward = hit.normal;
        Destroy(bulletHit, 5f);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_gunControl.isLocalPlayer)
        {
            _gunControl.CmdDeregisterBullet(BulletId);
        }
    }
}
