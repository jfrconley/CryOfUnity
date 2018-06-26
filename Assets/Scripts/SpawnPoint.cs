using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	[SerializeField] private MeshRenderer _renderer;
	
	private void Awake()
	{
		_renderer.enabled = false;
		transform.position.Set(transform.position.x, 0, transform.position.z);
	}

	public Vector3 GetSpawnPosistion()
	{
		return transform.position;
	}
}
