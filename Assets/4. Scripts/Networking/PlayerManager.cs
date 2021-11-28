using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public bool remote;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public GameObject model;
    public Vector3 targetTransform;
    public PlayerController playerController;
    public Transform verticalRotation;
    public bool isMounted = false;
    public string attachedEntity = "";

    private void Update()
	{
        if (remote && !isMounted)
        {
            float maxM = (Vector3.Distance(targetTransform, transform.position) / 3f) * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, targetTransform, maxM * 40f);

            if (Vector3.Distance(targetTransform, transform.position) > 100f)
            {
                transform.position = targetTransform;
            }
        }
	}

	private void OnDrawGizmos()
	{
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(targetTransform, new Vector3(1f, 2f, 1f));
	}

	public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.SetActive(false);
    }

    public void Respawn()
    {
        model.SetActive(true)
            ;
        SetHealth(maxHealth);
    }
}
