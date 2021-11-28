using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Cube : MonoBehaviour
{
    Entity self;

    void Start()
    {
        self = GetComponentInParent<Entity>();

        self.onPutInCharge += () =>
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        };

        self.onLostInCharge += () =>
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        };
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != null && collision.gameObject.transform.parent != null && collision.gameObject.transform.parent.gameObject == GameManager.instance.localPlayer.gameObject)
        {
            self.PutSelfInCharge();
        }
    }
}
