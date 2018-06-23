using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkManager : NetworkBehaviour
{
//	[SyncVar (hook = "SetPlayerId")]
	public string PlayerId;

	public TextMesh DebugText;
	public Collider2D collider;
	public Rigidbody2D rigidbody;
	private bool IsInit = false;
	private GameNetworkManager _manager;

	private void Awake()
	{
//		DebugText = gameObject.GetComponent<TextMesh>();
	}

	// Use this for initialization
	void Start ()
	{		
		if (isLocalPlayer)
		{
			gameObject.layer = LayerMask.NameToLayer("Player");
		}
	}

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
	
	private void UpdateDebugText()
	{
		DebugText.text = "PlayerID: " + PlayerId;
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

//	public override void OnStartAuthority()
//	{
//		Debug.Log("Requesting player name");
//		_manager = GameNetworkManager.Singleton;
//		CmdRequestName();
//	}
	private void Update()
	{
		Init();
	}

	[Command]
	private void CmdRequestName()
	{
		Debug.Log("Setting player name");
		string name = "player:" + System.Guid.NewGuid();
		UpdateName(name);
		RpcSetPlayerId(name);
	}

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
