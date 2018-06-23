using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Singleton;
    private readonly Dictionary<string, PlayerNetworkManager> _playerMap = new Dictionary<string, PlayerNetworkManager>();
    private readonly Dictionary<string, NeutralNetworkManager> _objectMap = new Dictionary<string, NeutralNetworkManager>();

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

    public void RegisterPlayer(string id, PlayerNetworkManager manager)
    {
        _playerMap.Add(id, manager);
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
        _objectMap.Add(id, manager);
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
        Debug.Log("Granting authority for " + objectId + " to " + playerId);
        NeutralNetworkManager objectManager = GetObject(objectId);
        if (objectManager.TouchingPlayer != "")
        {
            Debug.Log("Revoking " + objectManager.TouchingPlayer + " authority for " + objectId);
            objectManager.NetworkIdentity.RemoveClientAuthority(GetPlayer(objectManager.TouchingPlayer).connectionToClient);
        }
        objectManager.NetworkIdentity.AssignClientAuthority(GetPlayer(playerId).connectionToClient);
        objectManager.TouchingPlayer = playerId;
    }
}
