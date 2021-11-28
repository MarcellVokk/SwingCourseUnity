using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform, verticalOrientation;
    public static bool isMounted = false;

    Vector3 lastPos;
    Quaternion lastRot, lastVerticalRot;

    private void Update()
    {
        if (!isMounted)
        {
            SendPositionUpdateToServer();
        }
    }

    private void FixedUpdate()
    {
        //SendInputToServer();
    }

    bool Approximately(Quaternion quatA, Quaternion value, float acceptableRange)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
    }

    /// <summary>Sends player input to the server.</summary>
    public void SendPositionUpdateToServer()
    {
        if (Vector3.Distance(lastPos, transform.position) > 0.001f || !Approximately(lastRot, camTransform.rotation, 0.00001f) || !Approximately(lastVerticalRot, verticalOrientation.rotation, 0.00001f))
		{
            lastPos = transform.position;
            lastRot = camTransform.rotation;
            lastVerticalRot = verticalOrientation.rotation;
            ClientSend.PlayerMovement(transform.position, camTransform.rotation, verticalOrientation.rotation, false, isMounted);
        }
    }
}
