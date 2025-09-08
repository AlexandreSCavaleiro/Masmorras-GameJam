using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    Rigidbody2D rb;
    float horizontal, vertical;
    public float speed = 5;
    Animator animator;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        animarChar(horizontal,vertical);
        MovePlayer();
        
    }

    void GetInputs()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    void MovePlayer()
    {
        Vector2 movement = new Vector2(horizontal, vertical);
        if(movement.magnitude > 1)
                movement = movement.normalized;
        
        rb.linearVelocity = movement * speed;
    }

    void animarChar(float horizontal, float vertical)
    {
        if (vertical == 0 || horizontal == 0)
        {
            animator.SetBool("IDLE", true);
            animator.SetBool("WALKING", false);
        }
        if (vertical != 0 || horizontal != 0)
        {
            animator.SetBool("IDLE", false);
            animator.SetBool("WALKING", true);
        }

        /*
         * 0 up
         * 1 down
         * 2 left
         * 3 rigth
         */

        if (vertical > 0) //up
        {
            animator.SetInteger("Direction", 0);
        }
        if (vertical < 0) {
            animator.SetInteger("Direction", 1);
        }
        if (horizontal < 0)
        {
            animator.SetInteger("Direction", 2);
        }
        if (horizontal > 0)
        {
            animator.SetInteger("Direction", 3);
        }

        //Debug.Log($"direction {animator.GetInteger("Direction")} | {horizontal}/{vertical} | idle: {animator.GetBool("IDLE")} | Walking: {animator.GetBool("WALKING")}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        if (collision.gameObject.name.Contains("Sala"))
        {
            //GameObject.FindWithTag("MainCamera").gameObject.GetComponent
        } 
    }

}
    