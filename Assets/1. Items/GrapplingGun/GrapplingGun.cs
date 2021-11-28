using System;
using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
	private LineRenderer lr;
	private Vector3 grapplePoint;
	private SpringJoint joint;

	public LayerMask whatIsGrappleable;
	public Transform gunTip;
	public float maxDistance = 100f;
	public float spring = 4.5f;
	public float damper = 7f;
	public float massScale = 4.5f;

	private void OnEnable()
	{
		transform.GetComponent<Item>().OnBeginUseItem += (object sender, EventArgs e) =>
		{
			if (joint != null)
			{
				StopGrapple();
			}

			StartGrapple();
		};

		transform.GetComponent<Item>().OnEndUseItem += (object sender, EventArgs e) =>
		{
			StopGrapple();
		};

		transform.GetComponent<Item>().OnDrop += (object sender, EventArgs e) =>
		{
			StopGrapple();
		};

		lr = GetComponent<LineRenderer>();
	}

	GameObject grappleObj;
	public GameObject grapplePointObj;

	private void Update()
	{
		if(IsGrappling())
		{
			transform.GetComponent<Item>().SetDesiredRotation(Quaternion.LookRotation(GetGrapplePoint() - transform.position));
			joint.connectedAnchor = GetGrapplePoint();
			//desiredRotation = Quaternion.LookRotation(GetGrapplePoint() - transform.position);
		}
	}

	private void LateUpdate()
	{
		DrawRope();
	}

	private void StartGrapple()
	{
		if (Physics.Raycast(transform.GetComponent<Item>().ownerCamera.position, transform.GetComponent<Item>().ownerCamera.forward, out var hitInfo, maxDistance, whatIsGrappleable))
		{
			grapplePoint = hitInfo.point;

			if (!gameObject.GetComponent<Item>().isRemote)
			{
				grappleObj = Instantiate(grapplePointObj, hitInfo.transform);
				grappleObj.transform.position = grapplePoint;

				float num;
				joint = transform.GetComponent<Item>().ownerPlayer.gameObject.AddComponent<SpringJoint>();
				joint.autoConfigureConnectedAnchor = false;
				joint.connectedAnchor = grappleObj.transform.position;
				num = Vector3.Distance(transform.GetComponent<Item>().ownerPlayer.position, grapplePoint);
				joint.maxDistance = 0.4f;
				joint.minDistance = 0f;
				joint.spring = spring;
				joint.damper = damper;
				joint.massScale = massScale;
			}

			lr.positionCount = 2;
		}
	}

	private void DrawRope()
	{
		if (joint != null || gameObject.GetComponent<Item>().isUsing && gameObject.GetComponent<Item>().isRemote && lr.positionCount > 0)
		{
			lr.SetPosition(0, gunTip.position);
			lr.SetPosition(1, GetGrapplePoint());
		}
	}

	private void StopGrapple()
	{
		lr.positionCount = 0;
		Destroy(joint);
		Destroy(grappleObj);
	}

	public bool IsGrappling()
	{
		return joint != null;
	}

	public Vector3 GetGrapplePoint()
	{
		if(grappleObj != null)
        {
			return grappleObj.transform.position;
		}
        else
        {
			return Vector3.zero;
        }
	}
}
