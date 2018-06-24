using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerInputControl : MonoBehaviour
{
    //Component References
    private PlayerGunControl _gunControl;
	private new Rigidbody rigidbody;
	private PlayerNetworkManager _networkManager;
	
	//Serialized
	[SerializeField] private float moveSpeed = 1;
	
	private void Awake () {
        _gunControl = gameObject.GetComponent<PlayerGunControl>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
		_networkManager = gameObject.GetComponent<PlayerNetworkManager>();
	}
	
	private void Update ()
	{
		// Check if we are a local player
		if (_networkManager.isLocalPlayer)
		{
			//Movement
			Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
			rigidbody.velocity = input * moveSpeed * 100 * Time.deltaTime;

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
				target.y = 0;
				Vector3 dir = (target - transform.position).normalized;
				transform.forward = dir;
			}
		}
    }
}
