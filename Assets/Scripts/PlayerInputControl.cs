using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInputControl : MonoBehaviour
{
    //Component References
    private PlayerGunControl gunControl;
	private new Rigidbody2D rigidbody;
	
	//Serialized
	[SerializeField] private float moveSpeed = 1;
	
	private void Awake () {
        gunControl = gameObject.GetComponent<PlayerGunControl>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
	}
	
	private void Update ()
	{
        //Movement
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
		rigidbody.velocity = input * moveSpeed * 100 * Time.deltaTime;

        //Weapon Input
        if (Input.GetButtonDown("Fire1"))
        {
            gunControl.TriggerDown();
        }
        else if (Input.GetButton("Fire1"))
        {
            gunControl.TriggerHeld();
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
