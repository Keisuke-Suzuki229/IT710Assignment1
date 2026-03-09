using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    private Rigidbody rb;
    public float delay = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Fall());
        }
    }

    IEnumerator Fall()
    {
        yield return new WaitForSeconds(delay);

        rb.isKinematic = false;
        rb.useGravity = true;

        Destroy(gameObject, 3f);
    }

    public void TriggerFall()
    {
        StartCoroutine(Fall());
    }
}
