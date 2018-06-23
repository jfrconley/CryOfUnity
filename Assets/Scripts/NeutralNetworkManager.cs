using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class NeutralNetworkManager : NetworkBehaviour
{
    [SyncVar] public string TouchingPlayer = "";
    [SyncVar (hook = "SetObjectId")] public string ObjectId;
    
    public Collider2D collider;
    public Rigidbody2D rigidbody;
    public NetworkIdentity NetworkIdentity;
    private GameNetworkManager _manager;
    
    private void Awake()
    {
        NetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
    }
    
    public override void OnStartServer()
    {
        Debug.Log("Requesting object id");
        CmdRequestId();
    }

    [Command]
    private void CmdRequestId()
    {
        Debug.Log("Setting object id");
        ObjectId = "object:" + System.Guid.NewGuid();
    }
	
    private void SetObjectId(string id)
    {
        _manager = GameNetworkManager.Singleton;
        ObjectId = id;
        gameObject.name = id;
        _manager.RegisterObject(id, this);
    }

    private void OnDestroy()
    {
        _manager.DeregisterObject(ObjectId);
    }
}
