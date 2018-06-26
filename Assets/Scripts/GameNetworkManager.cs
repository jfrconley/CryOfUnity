using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Singleton;
    public static readonly float MaxHealth = 100f;
    public static readonly float RespawnTimer = 10f;
    
    private readonly Dictionary<string, PlayerNetworkManager> _playerMap = new Dictionary<string, PlayerNetworkManager>();
    private readonly Dictionary<string, NeutralNetworkManager> _objectMap = new Dictionary<string, NeutralNetworkManager>();
    private readonly Dictionary<string, Projectile> _bulletMap = new Dictionary<string, Projectile>();

    private void Awake()
    {
        if (Singleton != null)
        {
            Debug.Log("GameManager already exists!");
            return;
        }

        Debug.Log("GameManager created");
        Singleton = this;
    }

    public void RegisterBullet(string id, Projectile bullet)
    {
        Debug.Log($"Registering bullet {bullet.BulletId} for player {bullet.PlayerId}");
        _bulletMap[id] = bullet;
    }

    public void DeregisterBullet(string id)
    {
        if (_bulletMap.ContainsKey(id))
        {
            Debug.Log($"Deregistering bullet {id}");
            _bulletMap.Remove(id);
        }
    }

    public Projectile GetBullet(string id)
    {
        return _bulletMap[id];
    }
    
    public void RegisterPlayer(string id, PlayerNetworkManager manager)
    {
        try
        {
            _playerMap.Add(id, manager);
        }
        catch (ArgumentException)
        {
            Debug.Log($"Player {id} already registered");
        }
    }

    public void DeregisterPlayer(string id)
    {
        _playerMap.Remove(id);
    }

    public PlayerNetworkManager GetPlayer(string id)
    {
        return _playerMap[id];
    }
    
    public void RegisterObject(string id, NeutralNetworkManager manager)
    {
        try
        {
            _objectMap.Add(id, manager);
        }
        catch (ArgumentException)
        {
            Debug.Log($"Object {id} already registered");
        }
    }

    public void DeregisterObject(string id)
    {
        _objectMap.Remove(id);
    }

    public NeutralNetworkManager GetObject(string id)
    {
        return _objectMap[id];
    }

    [Server]
    public void SetObjectAuthority(string playerId, string objectId)
    {
        Debug.Log($"Granting authority for {objectId} to {playerId}");
        NeutralNetworkManager objectManager = GetObject(objectId);
        RemoveObjectAuthority(objectId);
        objectManager.NetworkIdentity.AssignClientAuthority(GetPlayer(playerId).connectionToClient);
        objectManager.TouchingPlayer = playerId;
    }

    [Server]
    public void RemoveObjectAuthority(string objectId)
    {
        NeutralNetworkManager objectManager = GetObject(objectId);
        if (objectManager.TouchingPlayer != "")
        {
            Debug.Log($"Removing Authority for {objectId} from {objectManager.TouchingPlayer}");
            objectManager.NetworkIdentity.RemoveClientAuthority(GetPlayer(objectManager.TouchingPlayer).connectionToClient);
            objectManager.TouchingPlayer = "";
        }
    }
}
