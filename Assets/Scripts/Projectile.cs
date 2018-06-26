﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Projectile : MonoBehaviour {

    [SerializeField] GameObject bulletImpactPrefab;
    public int damage = 1;

    public string BulletId;
    public string PlayerId;
    private PlayerGunControl _gunControl;

    private int _remotePlayerLayer;
    private int _playerLayer;
    private int _propLayer;
    float bulletSpeed = 60f;
    float physicsHitMultiplier = 1f;

    private void Awake()
    {
        _remotePlayerLayer = LayerMask.NameToLayer("RemotePlayer");
        _playerLayer = LayerMask.NameToLayer("Player");
        _propLayer = LayerMask.NameToLayer("SceneProps");
    }

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
        gameObject.name = bulletId;
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

        int otherLayer = hit.collider.gameObject.layer;
        //get health, do damage
        if (otherLayer == _remotePlayerLayer)
        {
            if (_gunControl != null && _gunControl.isLocalPlayer)
            {
                string otherPlayerId = hit.collider.gameObject.GetComponent<PlayerNetworkManager>().PlayerId;
                Debug.Log($"Local bullet {BulletId} hit player {otherPlayerId}");
                _gunControl.CmdSendBulletHit(BulletId, otherPlayerId);
            }
        }
        else if (otherLayer == _propLayer)
        {
            Rigidbody r = hit.collider.gameObject.GetComponent<Rigidbody>();
            if (r != null)
                r.AddForceAtPosition(transform.forward * (bulletSpeed * physicsHitMultiplier), hit.point);

            GameObject bulletHit = Instantiate(bulletImpactPrefab, hit.point, Quaternion.identity, hit.collider.transform);
            bulletHit.transform.forward = hit.normal;
            Destroy(bulletHit, 5f);
        }

        if (otherLayer != _playerLayer)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_gunControl != null && _gunControl.isActiveAndEnabled && _gunControl.isLocalPlayer)
        {
            _gunControl.CmdDeregisterBullet(BulletId, false);
        }
    }
}
