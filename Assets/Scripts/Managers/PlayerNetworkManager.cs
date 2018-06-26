using System.Collections;
using System.Collections.Generic;
using Smooth;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SmoothSync))]
public class PlayerNetworkManager : NetworkBehaviour
{
	[SyncVar (hook = nameof(UpdateName))]
	public string PlayerId = "";

	[SyncVar (hook = nameof(UpdateHealth))] public float Health = GameNetworkManager.MaxHealth;

	[SyncVar (hook = nameof(SetDeathState))] public bool IsDead;

	public TextMesh DebugText;
	public new Collider collider;
	public new Rigidbody rigidbody;
	private bool IsInit;
	private GameNetworkManager _manager;
	private SmoothSync _smoothSync;
	[SerializeField] private GameObject _graphics;

	private void Awake()
	{
		_smoothSync = gameObject.GetComponent<SmoothSync>();
	}

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

		if (isServer)
		{
			DeathCheck();
		}
	}

	[Server]
	public void DeathCheck()
	{
		if (Health <= 0)
		{
			ServerDie();
		}
	}

	[Server]
	public void ServerDie()
	{
		Debug.Log($"Player {PlayerId} died");
		IsDead = true;
		LocalDie();
		Invoke(nameof(ServerUnDie), GameNetworkManager.RespawnTimer);
		Vector3 spawnPosistion = _manager.GetSpawnPoint(PlayerId).GetSpawnPosistion();
		RpcTeleport(spawnPosistion);
//		Debug.Log($"Teleporting player {PlayerId}");
//		transform.position = new Vector3(spawnPosistion.x, 0, spawnPosistion.z);
//		_smoothSync.teleport();
	}

	[ClientRpc]
	public void RpcTeleport(Vector3 position)
	{
		if (isLocalPlayer)
		{
			Debug.Log($"Teleporting player {PlayerId}");
			transform.position = new Vector3(position.x, 0, position.z);
//			_smoothSync.teleport();
		}
	}
	
	[Server]
	public void ServerUnDie()
	{
		Debug.Log($"Respawning player {PlayerId}");
		Health = GameNetworkManager.MaxHealth;
		IsDead = false;
		LocalUnDie();
	}
	
	public void SetDeathState(bool deathState)
	{
		IsDead = deathState;
		if (IsDead)
		{
			LocalDie();
		}
		else
		{
			LocalUnDie();
		}
	}

	// Local effects of death set here
	public void LocalDie()
	{
		Debug.Log($"Killing local player {PlayerId}");
		_smoothSync.stopLerping();
		_graphics.SetActive(false);
		collider.enabled = false;
		rigidbody.isKinematic = true;
	}
	
	// Local effects of death unset here
	public void LocalUnDie()
	{
		Debug.Log($"Unkilling local player {PlayerId}");
		_graphics.SetActive(true);
		collider.enabled = true;
		rigidbody.isKinematic = false;
		_smoothSync.restartLerping();
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
