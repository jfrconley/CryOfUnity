using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInputControl : MonoBehaviour
{
    //Component References
    private PlayerGunControl _gunControl;
	private new Rigidbody2D _rigidbody;
	private PlayerNetworkManager _networkManager;
	
	//Serialized
	[SerializeField] private float moveSpeed = 1;
	
	private void Awake () {
        _gunControl = gameObject.GetComponent<PlayerGunControl>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
		_networkManager = gameObject.GetComponent<PlayerNetworkManager>();
	}
	
	private void Update ()
	{
		// Check if we are a local player
		if (_networkManager.isLocalPlayer)
		{
			//Movement
			Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
			_rigidbody.velocity = input * moveSpeed * 100 * Time.deltaTime;

			//Weapon Input
			if (Input.GetButtonDown("Fire1"))
			{
				_gunControl.TriggerDown();
			}
			else if (Input.GetButton("Fire1"))
			{
				_gunControl.TriggerHeld();
			}

			//Look Rotation
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			{

				Vector3 target = hit.point;
				target.z = 0;
				Vector3 dir = (transform.position - target).normalized;
				transform.up = dir;
			}
		}
    }
}
