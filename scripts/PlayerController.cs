using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Start() variables
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;
    
    //FSM
    private enum State { idle, running, jumping, falling, hurt, climb }
    private State state = State.idle;

    //Ladder Variables
    [HideInInspector]public bool canClimb = false;
    [HideInInspector] public bool bottomLadder = false;
    [HideInInspector] public bool topLadder = false;
    public Ladder ladder;
    private float naturalGravity;
    [SerializeField] float climbSpeed = 3f;

    // Inspector variables
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int cherries = 0;
    [SerializeField] private TextMeshProUGUI cherryCounter;
    [SerializeField] private float hurtForce = 10f;
    [SerializeField] private AudioSource cherry;
    [SerializeField] private AudioSource footstep;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        naturalGravity = rb.gravityScale;
    }
    void Update()
    {
        if (state == State.climb)
        {
            Climb();
        }
        else if (state != State.hurt)
        {
            Movement();
        }
        AnimationState();
        anim.SetInteger("state", (int)state); // Sets animation based on Enumerator State
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable")
        {
            cherry.Play();
            Destroy(collision.gameObject);
            cherries += 1;
            cherryCounter.text = cherries.ToString();
        }
        if (collision.tag == "House")
        {
            SceneManager.LoadScene("Ending Scene");
        }
        if (collision.tag == "PowerUp")
        {
            Destroy(collision.gameObject);
            jumpForce = 40f;
            GetComponent<SpriteRenderer>().color = Color.blue;
            StartCoroutine(ResetPower());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (state == State.falling)
            {
                enemy.JumpedOn();
                Jump();
            }

            else //if (other.gameObject.tag == "Enemy" && state != State.falling)
            {
                ReduceHealth();
                state = State.hurt;
                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    // Enemy is to my right therefore i should be damaged and moved left
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    // Enemy is to my left therefore i should be damaged and moved right
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxis("Horizontal");
        if(canClimb && Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            state = State.climb;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            transform.position = new Vector3(ladder.transform.position.x, rb.position.y);
            rb.gravityScale = 0f;
            
        }
        //Moving Left
              if (hDirection < 0)
              {
                  rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rb.velocity.y);
                  transform.localScale = new Vector2(-1, 1);
              }
              //Moving Right
              else if (hDirection > 0)
              {
                 rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rb.velocity.y);
                  transform.localScale = new Vector2(1, 1);
              }
        //Jumping
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
    }

    private void AnimationState()
    {
        if(state == State.climb)
        {

        }
        else if (state == State.jumping)
        {
           if( rb.velocity.y < 0.1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if (state == State.hurt)
        {
            if (Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f)
        {
            // Moving
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }

    private void Footstep()
    {
        footstep.Play();
    }
    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(10);
        jumpForce = 25f;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void ReduceHealth()
    {
        GameObject healthbar = GameObject.Find("Healthbar");
        Transform health = healthbar.GetComponent<Transform>();
        health.localScale = new Vector2(health.localScale.x - 1, health.localScale.y);
        if (health.localScale.x == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Climb()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = naturalGravity;
            anim.speed = 1f;
            Jump();
            return;
        }
        float vDirection = Input.GetAxis("Vertical");
        //Climbing up
        if (vDirection > 0.1f && !topLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }
        //Climbing down
        else if (vDirection < - 0.1f && !bottomLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }
        //Not moving
        else
        {
            anim.speed = 0f;
            rb.velocity = Vector2.zero;
        }
    }
}

