using System.Collections;
using System.Collections.Generic;
using Smooth;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(SmoothSync))]
//[RequireComponent(typeof(TextMesh))]
public class NeutralNetworkManager : NetworkBehaviour
{
    [SyncVar (hook = "UpdateTouchingPlayer")] public string TouchingPlayer = "";
    [SyncVar (hook = "SetObjectId")] public string ObjectId = "";

    public TextMesh DebugText;
    public new Collider collider;
    public new Rigidbody rigidbody;
    public NetworkIdentity NetworkIdentity;
    private GameNetworkManager _manager;
    private SmoothSync _smoothSync;
    
    private void Awake()
    {
        _smoothSync = gameObject.GetComponent<SmoothSync>();
        NetworkIdentity = gameObject.GetComponent<NetworkIdentity>();
//        DebugText = gameObject.GetComponent<TextMesh>();
    }

    private void UpdateDebugText()
    {
        DebugText.text = "ObjectID: " + ObjectId + "\nTouchingPlayer: " + TouchingPlayer;
    }

//    private void OnCollisionExit2D(Collision2D other)
//    {
//        if (!isServer && hasAuthority && other.gameObject.layer == LayerMask.NameToLayer("Player"))
//        {
//            PlayerNetworkManager playerManager = other.gameObject.GetComponent<PlayerNetworkManager>();
//            if (playerManager.PlayerId == TouchingPlayer)
//            {
//                CmdRemoveObjectAuthority();
//            }
//        }
//    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SceneProps") && hasAuthority && TouchingPlayer != "")
        {
            NeutralNetworkManager otherProp = other.gameObject.GetComponent<NeutralNetworkManager>();
            if (otherProp.TouchingPlayer != TouchingPlayer)
            {
                CmdRequestObjectAuthority(otherProp.ObjectId);
            }
        }
    }

    [Command]
    public void CmdRequestObjectAuthority(string objectId)
    {
        _manager.SetObjectAuthority(TouchingPlayer, objectId);
    }
    
    [Command]
    public void CmdRemoveObjectAuthority()
    {
        _manager.RemoveObjectAuthority(ObjectId);
    }

    public override void OnStartAuthority()
    {
        if (ObjectId == "")
        {
            Debug.Log("Requesting object id");
            CmdRequestId();
        }
    }

    [Command]
    private void CmdRequestId()
    {
        Debug.Log("Setting object id");
        ObjectId = "object:" + System.Guid.NewGuid();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (ObjectId != "")
        {
            SetObjectId(ObjectId);
        }
    }
    
    private void SetObjectId(string id)
    {
        _manager = GameNetworkManager.Singleton;
        ObjectId = id;
        gameObject.name = id;
        _manager.RegisterObject(id, this);
        UpdateDebugText();
    }

    private void UpdateTouchingPlayer(string playerId)
    {
        TouchingPlayer = playerId;
        _smoothSync.clearBuffer();
        UpdateDebugText();
    }
    
    private void OnDestroy()
    {
        if (_manager != null)
            _manager.DeregisterObject(ObjectId);
    }
}
