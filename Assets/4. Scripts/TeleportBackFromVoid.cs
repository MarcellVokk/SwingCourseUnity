using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBackFromVoid : MonoBehaviour
{
	public float voidStartY = 200f;

	private void Start()
	{
		transform.position = new Vector3(0, voidStartY, 0);
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag != "IgnoreCollision")
		{
			other.transform.position = new Vector3(0, 10, 0);
		}
	}
}
