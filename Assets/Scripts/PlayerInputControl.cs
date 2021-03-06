﻿using System.Collections;
using System.Collections.Generic;
using Smooth;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SmoothSync))]
public class PlayerInputControl : MonoBehaviour
{
    //Component References
    private PlayerGunControl _gunControl;
	private Rigidbody rigidbody;
	private PlayerNetworkManager _networkManager;
	private SmoothSync _smoothSync;
    private Camera cam;
    private Renderer renderer;

    //Serialized
    [SerializeField] private Material[] damageFlashes;
	[SerializeField] private float moveSpeed = 1;
    [SerializeField] private float footstepTimer = 0.4f;
    private float footstep;
    private Material baseMaterial;
	
	private void Awake ()
	{
		_smoothSync = gameObject.GetComponent<SmoothSync>();
        _gunControl = gameObject.GetComponent<PlayerGunControl>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
		_networkManager = gameObject.GetComponent<PlayerNetworkManager>();
        renderer = gameObject.GetComponentInChildren<Renderer>();
        baseMaterial = renderer.material;
        cam = Camera.main;

        footstep = footstepTimer;
	}

    private void Start()
    {
        if (_networkManager.isLocalPlayer)
        {
            CameraControl.singleton.PlayerTarget = transform;
        }
    }

    private void Update ()
	{
        if (Input.GetKeyDown(KeyCode.P))
            RunDamageFlash();

		// Check if we are a local player
		if (_networkManager.isLocalPlayer)
		{
			if (!_networkManager.IsDead)
			{
				float delta = Time.deltaTime;
				//Movement
				Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
				rigidbody.velocity = input * moveSpeed * 100 * delta;
				if (input != Vector3.zero)
				{
					footstep -= delta;
					if (footstep <= 0)
					{
						AudioManager.singleton.PlayFootstep(transform.position);
						footstep = footstepTimer;
					}
				}

				//Weapon Input
				if (Input.GetButtonDown("Fire1"))
				{
					_gunControl.TriggerDown();
				}
				else if (Input.GetButton("Fire1"))
				{
					_gunControl.TriggerHeld();
				}

				if (Input.GetButton("Submit"))
				{
					_networkManager.UpdateHealth(0);
				}

				//Look Rotation
				//Screenpos method
				Vector3 v = Input.mousePosition;
				v.z = cam.transform.position.y;
				Vector3 target = cam.ScreenToWorldPoint(v);
				Vector3 dir = (target - transform.position).normalized;
				transform.forward = dir;
			}
		}
		else
		{
			if (_smoothSync.latestReceivedVelocity != Vector3.zero)
			{
				footstep -= Time.deltaTime;
				if (footstep <= 0)
				{
					AudioManager.singleton.PlayFootstep(transform.position);
					footstep = footstepTimer;
				}
			}
		}
    }

    public void RunDamageFlash()
    {
        StartCoroutine(DamageFlash());
    }

    IEnumerator DamageFlash()
    {
        int i = 0;
        while(i < damageFlashes.Length)
        {
            renderer.material = damageFlashes[i];
            i++;
            yield return new WaitForSeconds(0.03f);
        }
        renderer.material = baseMaterial;
    }
}
