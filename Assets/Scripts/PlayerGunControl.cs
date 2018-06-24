using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunControl : MonoBehaviour {

    GunSettings gun;
    [SerializeField] Transform bulletSpawn;
    private float cooldown = 0.4f;

    private void Start()
    {
        gun = GunLocker.singleton.GetGun("Pistol");
    }

    private void Update()
    {
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
        if (gun.fireMode == GunSettings.GunFireMode.Semi)
            return;
        if (cooldown > 0)
            return;
        Shoot();
    }

    void Shoot()
    {
        cooldown = gun.fireRate;
        //ammo

        //MuzzleFlash
        GameObject flash = Instantiate(gun.muzzleFlashPrefab, bulletSpawn.position, bulletSpawn.rotation, bulletSpawn);
        Destroy(flash, gun.muzzleTimer);

        Quaternion rotOffset = Quaternion.AngleAxis(Random.Range(-gun.bulletSpread / 2, gun.bulletSpread / 2), Vector3.up);//adjust upward rotation Quaternion.AngleAxis(Random.Range(-gun.bulletSpread / 2, gun.bulletSpread / 2), Vector3.right)
        GameObject go = Instantiate(gun.bulletPrefab, bulletSpawn.position, bulletSpawn.rotation * rotOffset);
    
    //gun.shootSound;
    //gun.emptySound;
}

    public class GunVariables
    {
        public string name;
        public int ammo = 0;
    }
}
