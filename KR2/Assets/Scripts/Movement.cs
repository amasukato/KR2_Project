using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public enum State
    {
        Idle,
        IdleRecovery,
        Moving,
        Running, 
        Dodging,
        Jumping,
    }

    public State MovementState = State.Idle;
    public Animator animator;
    [SerializeField] public CharacterController controller;
    public Transform cam;
    public Transform groundCheck;
    private float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("BaseMovement")]
    public float speed = 6f;
    private float gravity = -19.62f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float jumpHeight = 3f;

    Vector3 movDir;
    Vector3 velocity;
    public bool IsGrounded;
    public bool EnabledGravity;

    [Header("Dodge")]

    public float dodgeSpeed = 50f;
    public float dodgeTime = 0.25f;
    public float dodgeCD = 0.5f;
    public float dodgeMaxCD = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
       

    }

    // Update is called once per frame
    void Update()
    {
        switch (MovementState)
        {
            default:
            case State.Idle:
                Moving();

                break;

            case State.IdleRecovery:
                Moving();

                break;

            case State.Moving:
                Moving();
                Dodge();

                break;
            case State.Running:
                break;
            case State.Dodging:
                break;
            case State.Jumping:
                break;
        }

        //Moving();

        IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (IsGrounded && movDir.y <= 0)
        {
            movDir.y = -2f;
        }

        if(Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }

        if(EnabledGravity) velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);


        dodgeCD -= Time.deltaTime;
    }

    void Moving()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        if(horizontal == 0 && vertical == 0)
        {
            animator.SetBool("Walk", false);
            MovementState = State.Idle;
        }
        else
        {
            animator.SetBool("Walk", true);
            MovementState = State.Moving;
        }
    }

    void Dodging()
    {
        if(Input.GetButtonDown("Dodge"))
        {
            if(dodgeCD <= 0)
            {
                StartCoroutine(Dodge());

                MovementState = State.Dodging;
            }
        }
    }

    IEnumerator Dodge()
    {
        float startTime = Time.time;


        while(Time.time < startTime + dodgeTime)
        {
            controller.Move(movDir * dodgeSpeed * Time.deltaTime);

            dodgeCD = dodgeMaxCD;
            yield return null;
        }

        MovementState = State.Idle;
    }
}
