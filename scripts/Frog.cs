using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : Enemy
{
    [SerializeField] private float leftCap;
    [SerializeField] private float rightCap;
    [SerializeField] private float jumpLength = 5f;
    [SerializeField] private float jumpHight = 10f;
    [SerializeField] private LayerMask ground;

    private bool facingLeft = true;
    private Collider2D coll;

    protected override void Start()
    {
        base.Start();
        coll = GetComponent<Collider2D>();
    }

    private void Update()
    {
        //Transition from Jump to Fall
        if (anim.GetBool("jumping"))
        {
            if (rb.velocity.y < 0.1f)
            {
                anim.SetBool("falling", true);
                anim.SetBool("jumping", false);
            }
        }

        //Transition from Fall to Idle
        if (anim.GetBool("falling"))
        {
            if (rb.velocity.y == 0f)
            {
                anim.SetBool("falling", false);
            }
        }
    }

    private void Move()
    {
        if (facingLeft == true)
        {
            // Test to see if we are beyond the leftCap
            if (transform.position.x > leftCap)
            {
                // Make sure Sprite is facing correct location and if it is not then face the correct direction
                if (transform.localScale.x != 1)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                // Test to see if I am on the ground. If so then Jump
                if (coll.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(-jumpLength, jumpHight);
                    anim.SetBool("jumping", true);
                }
            }
            // If it is not we are going to face right
            else
            {
                facingLeft = false;
            }
        }
        else
        {
            // Test to see if we are beyond the rightCap
            if (transform.position.x < rightCap)
            {
                // Make sure Sprite is facing correct location and if it is not then face the correct direction
                if (transform.localScale.x != -1)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                // Test to see if I am on the ground. If so then Jump
                if (coll.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(jumpLength, jumpHight);
                    anim.SetBool("jumping", true);
                }
            }
            // If it is not we are going to face right
            else
            {
                facingLeft = true;
            }
        }
    }

    

}
