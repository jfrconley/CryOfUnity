using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunLibrary : MonoBehaviour {
    
    [SerializeField] private GunSettings[] gunSettings;

    Dictionary<string, GunSettings> gunDictionary = new Dictionary<string, GunSettings>();

    void Awake()
    {
        foreach (GunSettings gun in gunSettings)
        {
            gunDictionary.Add(gun.name, gun);
        }
    }

    public GunSettings GetGunFromName(string name)
    {
        if (gunDictionary.ContainsKey(name))
        {
            return gunDictionary[name];
        }
        return null;
    }
}

[Serializable]
public class GunSettings
{
    public string name;
    public enum GunFireMode { Semi, Auto };
    public GunFireMode fireMode = GunFireMode.Semi;
    public MeshRenderer mesh;
    public Material material;
    //public Transform bulletSpawn; Vector3 instead

    [Header("Bullet")]
    public int damage = 1;
    public float fireRate = 0.5f;
    public float bulletSpeed = 60f;
    public float physicsHitMultiplier = 1f;
    public float bulletSpread = 5f;

    [Header("Ammo")]
    public int magazineSize = 6;
    public float reloadTime = 1f;

    [Header("Effects")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public float muzzleTimer;
    public float recoil;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip emptySound;
    public AudioClip reloadSound;
    public AudioClip[] movementSound;
}
