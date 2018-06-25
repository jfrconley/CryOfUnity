using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(PlayerNetworkManager))]
public class PlayerGunControl : NetworkBehaviour
{
    [SyncVar (hook = nameof(UpdateGun))] private GunLocker.GunState currentGun;
    GunSettings currentGunSetting;
    private PlayerNetworkManager _networkManager;
    [SerializeField] Transform bulletSpawn;
    private float cooldown = 0.4f;
    private bool _isInit;

    private void Awake()
    {
        _networkManager = gameObject.GetComponent<PlayerNetworkManager>();
    }

    private void Init()
    {
        if (isLocalPlayer && _networkManager.PlayerId != "" && !_isInit)
        {
            Debug.Log("Requesting Starting gun Pistol");
            _isInit = true;
            CmdRequestStartingGun("Pistol");
        }
        else if (!isLocalPlayer && currentGun.Type != currentGunSetting?.name)
        {
            UpdateGun(currentGun);
        }
    }

    [Command]
    private void CmdRequestStartingGun(string type)
    {
        string id = GunLocker.singleton.ManufactureGun(type);
        GunLocker.singleton.ChangeOwnership(id, _networkManager.PlayerId);
        currentGun = GunLocker.singleton.RetrieveGun(id);
    }

    private void UpdateGun(GunLocker.GunState gun)
    {
        Debug.Log($"Updating gun to {gun.Id} for player {_networkManager.PlayerId}");
        currentGun = gun;
        currentGunSetting = GunLocker.singleton.GetGunInfo(gun.Type);
    }

    private void Update()
    {
        Init();
        cooldown -= Time.deltaTime;
    }

    public void TriggerDown()
    {
        if (cooldown > 0)
            return;
        Shoot();
    }

    public void TriggerHeld()
    {
        if (currentGunSetting.fireMode == GunSettings.GunFireMode.Semi)
            return;
        if (cooldown > 0)
            return;
        Shoot();
    }

    [Command]
    public void CmdSendBulletHit(string bulletId, string hitPlayerId)
    {
        Projectile bullet = GameNetworkManager.Singleton.GetBullet(bulletId);
        GameNetworkManager.Singleton.DeregisterBullet(bulletId);
        GameNetworkManager.Singleton.GetPlayer(hitPlayerId).Health -= bullet.damage;
        RpcDeregisterBullet(bulletId, false);
        Debug.Log($"Player {bullet.PlayerId} hit {hitPlayerId} with {bulletId}");
    }

    [Command]
    public void CmdDeregisterBullet(string id, bool destroy)
    {
        GameNetworkManager.Singleton.DeregisterBullet(id);
        RpcDeregisterBullet(id, destroy);
    }

    [ClientRpc]
    public void RpcDeregisterBullet(string id, bool destroy)
    {
        if (destroy)
        {
            Destroy(GameNetworkManager.Singleton.GetBullet(id).gameObject);
        }
        GameNetworkManager.Singleton.DeregisterBullet(id);
    }
    
    [Command]
    public void CmdRequestFire(string id, Vector3 position, Quaternion rotation, Quaternion offset)
    {
        RpcDoFire(id, position, rotation, offset);
    }

    [ClientRpc]
    public void RpcDoFire(string id, Vector3 position, Quaternion rotation, Quaternion offset)
    {
        if (!isLocalPlayer)
        {
            DoFire(id, position, rotation, offset);
        }
    }

    public void DoFire(string id, Vector3 position, Quaternion rotation, Quaternion offset)
    {
        GameObject flash = Instantiate(currentGunSetting.muzzleFlashPrefab, bulletSpawn.position, bulletSpawn.rotation, bulletSpawn);
        Destroy(flash, currentGunSetting.muzzleTimer);

        GameObject go = Instantiate(currentGunSetting.bulletPrefab, position, rotation * offset);
        Projectile bullet = go.GetComponent<Projectile>();
        bullet.SetBulletVars(currentGun.Type, id, _networkManager.PlayerId, this);
        GameNetworkManager.Singleton.RegisterBullet(id, bullet);
    }
    
    void Shoot()
    {
        if (currentGun.Type != "" && isLocalPlayer)
        {
            cooldown = currentGunSetting.fireRate;
            //ammo

            //MuzzleFlash
//            GameObject flash = Instantiate(currentGunSetting.muzzleFlashPrefab, bulletSpawn.position,
//                bulletSpawn.rotation, bulletSpawn);
//            Destroy(flash, currentGunSetting.muzzleTimer);

            Quaternion rotOffset =
                Quaternion.AngleAxis(
                    Random.Range(-currentGunSetting.bulletSpread / 2, currentGunSetting.bulletSpread / 2),
                    Vector3.up); //adjust upward rotation Quaternion.AngleAxis(Random.Range(-gun.bulletSpread / 2, gun.bulletSpread / 2), Vector3.right)
//            GameObject go = Instantiate(currentGunSetting.bulletPrefab, bulletSpawn.position,
//                bulletSpawn.rotation * rotOffset);

            string bulletId = $"bullet:{System.Guid.NewGuid()}";
            CmdRequestFire(bulletId, bulletSpawn.position, bulletSpawn.rotation, rotOffset);
            DoFire(bulletId, bulletSpawn.position, bulletSpawn.rotation, rotOffset);
        }
    
    //gun.shootSound;
    //gun.emptySound;
    }
}
