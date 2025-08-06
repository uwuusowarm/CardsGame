using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player_Animationen : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator anim;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && anim.GetBool("IsBoxing") == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
                anim.SetBool("IsWalking", true);
            }
        }

        bool isWalking = agent.velocity.magnitude > 0f;
        anim.SetBool("IsWalking", isWalking);

        if (Input.GetMouseButton(1))
        {
            anim.SetBool("IsBoxing", true);
        }

        if (Input.GetMouseButton(0))
        {
            anim.SetBool("IsBoxing", false);
        }
    }

    
} // Added this closing brace to fix the CS1513 error
