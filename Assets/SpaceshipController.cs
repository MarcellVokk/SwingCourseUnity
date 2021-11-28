using UnityEngine;
using System.Collections;
using System;

public class SpaceshipController : MonoBehaviour
{
	public float sensitivity = 0.4f;

	public GameObject target, spaceshipUI, camera, redLight;
	public RectTransform horizon;
	Entity self;
	public Transform platformSpawn;

	void Start()
	{
		StartCoroutine(RedBlink());
		self = GetComponent<Entity>();

		self.onPutInCharge += () =>
		{
			gameObject.GetComponent<Rigidbody>().isKinematic = false;
			spaceshipUI.SetActive(true);
			camera.SetActive(true);

			self.MountPlayer();

			//GameManager.instance.localPlayer.GetComponent<PlayerMovement>().enabled = false;
			//GameManager.instance.localPlayer.GetComponent<PlayerMovement>().playerCam.gameObject.SetActive(false);
			//GameManager.instance.localPlayer.GetComponent<Rigidbody>().isKinematic = true;
			//GameManager.instance.localPlayer.GetComponent<Rigidbody>().useGravity = false;
			//GameManager.instance.localPlayer.GetComponentInChildren<BoxCollider>(true).enabled = false;

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;
			UIManager.instance.crosshair.SetActive(false);
		};

		self.onLostInCharge += () =>
		{
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
			spaceshipUI.SetActive(false);
			camera.SetActive(false);

			self.DismountPlayer();

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			UIManager.instance.crosshair.SetActive(true);
		};


	}

	IEnumerator RedBlink()
    {
		for (; ; )
		{
			redLight.SetActive(true);
			yield return new WaitForSeconds(0.05f);
			redLight.SetActive(false);
			yield return new WaitForSeconds(1.5f);
		}
	}

	public float maxSpeed = 400000;
	public float minSpeed = 5000;
	public float warpSpeed = 200000;

	public float moveSpeed = 100000;

	private void FixedUpdate()
	{
		if (!self.remote && !UIManager.blockUserInput)
		{
			if (Input.GetKey(KeyCode.S))
			{
				GetComponent<Rigidbody>().AddForce(-(transform.forward * moveSpeed) * Time.fixedDeltaTime);
			}

			if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.W))
			{
				GetComponent<Rigidbody>().AddForce(transform.forward * (moveSpeed + warpSpeed) * Time.fixedDeltaTime);
			}
			else if (Input.GetKey(KeyCode.W))
			{
				GetComponent<Rigidbody>().AddForce((transform.forward) * moveSpeed * Time.fixedDeltaTime);
			}
			else if (Input.GetKey(KeyCode.Space))
			{
				GetComponent<Rigidbody>().AddForce(transform.up * moveSpeed * Time.fixedDeltaTime);
			}

			if (Input.GetKey(KeyCode.LeftShift))
			{
				GetComponent<Rigidbody>().AddForce(-(transform.up * moveSpeed) * Time.fixedDeltaTime);
			}

			if (Input.GetKey(KeyCode.L))
			{
				if (GameManager.players[Client.instance.myId] != null && GameManager.players[Client.instance.myId].playerController != null)
				{
					GameManager.players[Client.instance.myId].playerController.transform.position = transform.position + transform.right * 10f;
				}

				self.RemoveSelfFromCharge();
			}

			target.transform.position = Input.mousePosition;
		}
	}

	void Update()
	{
		if (!self.remote && !UIManager.blockUserInput)
        {
			horizon.rotation = Quaternion.Euler(new Vector3(0, 0, -GetComponent<Rigidbody>().rotation.eulerAngles.z));

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;

			if (Mathf.Abs(target.transform.localPosition.y) > 20f)
			{
				GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(-target.transform.localPosition.y * sensitivity * Time.deltaTime, 0, 0));
			}

			if (Mathf.Abs(target.transform.localPosition.x) > 20f)
			{
				//transform.Rotate(new Vector3(0, target.transform.localPosition.x * sensitivity * Time.deltaTime, 0));
				GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0, target.transform.localPosition.x * sensitivity * Time.deltaTime, 0));
				//GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(GetComponent<Rigidbody>().rotation.eulerAngles + new Vector3(0, target.transform.localPosition.x * sensitivity * Time.deltaTime, 0)));
			}

			if (Input.GetKey(KeyCode.Q))
			{
				GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0, 0, 70f * sensitivity * Time.deltaTime));
			}

			if (Input.GetKey(KeyCode.E))
			{
				GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0, 0, -70f * sensitivity * Time.deltaTime));
			}

			if(Input.GetAxis("Mouse ScrollWheel") > 0)
            {
				moveSpeed += 10000;

				if (moveSpeed > maxSpeed)
				{
					moveSpeed = maxSpeed;
				}
			}

			if (Input.GetAxis("Mouse ScrollWheel") < 0)
			{
				moveSpeed -= 10000;

				if (moveSpeed < minSpeed)
				{
					moveSpeed = minSpeed;
				}
			}

			target.transform.position = Input.mousePosition;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.tag == "Player")
		{
			self.PutSelfInCharge();
		}
	}
}
