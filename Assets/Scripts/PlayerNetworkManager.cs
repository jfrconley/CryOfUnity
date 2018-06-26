using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkManager : NetworkBehaviour
{
	[SyncVar (hook = nameof(UpdateName))]
	public string PlayerId = "";

	[SyncVar (hook = nameof(UpdateHealth))] public float Health = 100;

	public TextMesh DebugText;
	public new Collider collider;
	public new Rigidbody rigidbody;
	private bool IsInit = false;
	private GameNetworkManager _manager;

	// Use this for initialization
	void Start ()
	{		
		// If we are local, set the layer to allow object collisions
		if (isLocalPlayer)
		{
			GameCanvasManager.singleton.SetHealthMaximum(Health);
			gameObject.layer = LayerMask.NameToLayer("Player");
		}
		else
		{
			gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
		}
	}

	// Requests id from server when ready
	private void Init()
	{
		if (isLocalPlayer && _manager == null && GameNetworkManager.Singleton != null && !IsInit)
		{
			IsInit = true;
			Debug.Log("Requesting player name");
			_manager = GameNetworkManager.Singleton;
			CmdRequestName();
		}
	}
	
	// Handle initial state sync for late clients
	public override void OnStartClient()
	{
		base.OnStartClient();
		// If our name is already set, we need to update cause the hook probably didn't run
		if (PlayerId != "")
		{
			UpdateName(PlayerId);
		}
	}

	public void UpdateHealth(float health)
	{
		Health = health;
		if (isLocalPlayer)
		{
			GameCanvasManager.singleton.SetHealth(health);
		}
	}
	
	private void UpdateDebugText()
	{
		DebugText.text = "PlayerID: " + PlayerId;
	}
	
	private void OnCollisionEnter(Collision other)
	{
		// Check if we are local and we hit a prop
		if (isLocalPlayer && other.gameObject.layer == LayerMask.NameToLayer("SceneProps"))
		{
			// Make sure we don't already own it
			NeutralNetworkManager objectManager = other.gameObject.GetComponent<NeutralNetworkManager>();
			if (objectManager != null && objectManager.TouchingPlayer != PlayerId)
			{
				// Request box authority
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

	private void Update()
	{
		Init();
	}

	// Generate a uuid and tell players to update
	[Command]
	private void CmdRequestName()
	{
		Debug.Log("Setting player name");
		string name = "player:" + System.Guid.NewGuid();
		PlayerId = name;
	}

	// Set name and register with manager
	private void UpdateName(string id)
	{
		_manager = GameNetworkManager.Singleton;
		PlayerId = id;
		gameObject.name = id;
		_manager.RegisterPlayer(id, this);
		UpdateDebugText();
	}
	
	[ClientRpc]
	private void RpcSetPlayerId(string id)
	{
		UpdateName(id);
	}
	
	private void OnDestroy()
	{
		if (_manager != null)
			_manager.DeregisterPlayer(PlayerId);
	}
}
