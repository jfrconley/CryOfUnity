using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInputControl : MonoBehaviour
{
	//Component References
	private new Rigidbody2D rigidbody;
	
	//Serialized
	[SerializeField] private float moveSpeed = 1;
	
	private void Awake () {
		rigidbody = gameObject.GetComponent<Rigidbody2D>();
	}
	
	private void Update ()
	{
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
		rigidbody.velocity = input * moveSpeed * 100 * Time.deltaTime;
	}
}
