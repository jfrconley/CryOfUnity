using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkManager : NetworkBehaviour
{
	[SyncVar (hook = "SetPlayerId")]
	public string PlayerId;
	
	public Collider2D collider;
	public Rigidbody2D rigidbody;
	private GameNetworkManager _manager = GameNetworkManager.Singleton;
	
	// Use this for initialization
	void Start ()
	{		
		if (isLocalPlayer)
		{
			gameObject.layer = LayerMask.NameToLayer("Player");
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (isLocalPlayer && other.gameObject.layer == LayerMask.NameToLayer("SceneProps"))
		{
			NeutralNetworkManager objectManager = other.gameObject.GetComponent<NeutralNetworkManager>();
			if (objectManager != null && objectManager.TouchingPlayer != PlayerId)
			{
				Debug.Log("Requesting Box authority for local player");
				CmdRequestObjectAuthority(objectManager.ObjectId);
			}
		}
	}

	[Command]
	public void CmdRequestObjectAuthority(string objectId)
	{
		_manager.SetObjectAuthority(PlayerId, objectId);
	}
	
	public override void OnStartLocalPlayer()
	{
		Debug.Log("Requesting name");
		CmdRequestName();
	}

	[Command]
	private void CmdRequestName()
	{
		Debug.Log("Setting player name");
		PlayerId = "player:" + System.Guid.NewGuid();
	}
	
	private void SetPlayerId(string id)
	{
		_manager = GameNetworkManager.Singleton;
		PlayerId = id;
		gameObject.name = id;
		_manager.RegisterPlayer(id, this);
	}

	private void OnDestroy()
	{
		_manager.DeregisterPlayer(PlayerId);
	}
}
