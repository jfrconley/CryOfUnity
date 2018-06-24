using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(GunLibrary))]
[RequireComponent(typeof(NetworkIdentity))]
public class GunLocker : NetworkBehaviour
{
    public struct GunState
    {
        public string Type;
        public int CurrentAmmo;
        public string Owner;
        public readonly string Id;

        public GunState(string type, string owner = "", int currentAmmo = 0)
        {
            Id = $"gun:{System.Guid.NewGuid()}";
            Type = type;
            Owner = owner;
            CurrentAmmo = currentAmmo;
        }
    }

    private Dictionary<string, GunState> GunRegistry = new Dictionary<string, GunState>();
    public static GunLocker singleton;

    GunLibrary library;

    void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            library = GetComponent<GunLibrary>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GunSettings GetGunInfo(string type)
    {
        return library.GetGunFromName(type);
    }

    [Server]
    public string ManufactureGun(string type)
    {
        GunSettings info = GetGunInfo(type);
        GunState gun = new GunState(type, currentAmmo: info.magazineSize);
        GunRegistry.Add(gun.Id, gun);
        Debug.Log($"Manufactured new gun {type} with id {gun.Id}");
        return gun.Id;
    }

    [Server]
    public GunState RetrieveGun(string id)
    {
        return GunRegistry[id];
    }

    [Server]
    public void ChangeOwnership(string id, string playerId)
    {
        GunState gun = GunRegistry[id];
        Debug.Log($"Changing ownership of {id} from {gun.Owner} to {playerId}");
        gun.Owner = playerId;
    }
}