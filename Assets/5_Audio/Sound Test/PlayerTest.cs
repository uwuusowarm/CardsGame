using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerTest : MonoBehaviour
{
    public float Movespeed = 5f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Movespeed * Time.deltaTime);
            
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Movespeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Movespeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Movespeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Untagged"))
        {
            FindAnyObjectByType<Sound_Manager>().Play("Coin");
            Destroy(other.gameObject);
        }
       
    }
}
