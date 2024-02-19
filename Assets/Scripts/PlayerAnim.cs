using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    public bool rbVelocity;
    public Vector2 CurrentVelocity;
    public bool GoingBack;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 move = CurrentVelocity;
        if (rbVelocity)
        move = rb.velocity;
        //Debug.Log(move+" "+ move.magnitude);
        if (move.magnitude > 0)
        {
            if (rb.velocity.y > 0.5)
            {
                GoingBack = true;
            }

            if (rb.velocity.y < -0.5)
            {
                GoingBack = false;
            }

            if (GoingBack)
            {
                anim.CrossFade("Walk_Back", 0, 0);
            }
            else
            {
                anim.CrossFade("Walk", 0, 0);
            }
        }
        else
        {
            if (GoingBack == true)
            {
                anim.CrossFade("Idle_Back", 0, 0);
            }
            else
            {
                anim.CrossFade("Idle", 0, 0);
            }
        }
    }
}
