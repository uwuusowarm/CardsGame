using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Sound_Manager.instance.Play("Coin");
            Destroy(gameObject);
        }

    }
}
