using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 1.8f;
    public float runSpeed = 7f;
    public float sprintSpeed = 15f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;
    private bool _isGrounded;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpRequested;
    private RaycastHit hit;
    private bool isHanging;
    private bool isClimbing;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }
    public void OnJump(InputValue value)
    {
        if (value.isPressed) jumpRequested = true;
    }

    public void StartHanging()
    {
        if (isHanging) return;
        isHanging = true;
        anim.SetTrigger("GrabTrigger");
        anim.SetBool("IsHanging", true);
        velocity = Vector3.zero;

        Vector3 safePos = hit.point + hit.normal * 0.5f;
        float yOffset = 1.8f;
        transform.position = new Vector3(safePos.x, hit.point.y - yOffset, safePos.z);

        transform.forward = -hit.normal;

    }

    public void ClimbUp()
    {
        if (isClimbing) return;

        anim.SetTrigger("Climb");
        isHanging = false;
        anim.SetBool("IsHanging", false);

        isClimbing = true;
        velocity = Vector3.zero;

        Invoke("FinalizeClimb", 0.7f);

    }
    void FinalizeClimb()
    {
        controller.enabled = false;
        //Vector3.Lerp(transform.position, transform.position + transform.forward * 0.3f + Vector3.up * 1.8f, 0.5f);
        transform.position += transform.forward * 0.3f + Vector3.up * 1.8f;
        controller.enabled = true;

        isClimbing = false;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _isGrounded = controller.isGrounded;

        if (isHanging || isClimbing)
        {
            velocity = Vector3.zero;
            if(isHanging)
            {
                if (moveInput.y > 0.1f)
                {
                    ClimbUp ();
                }

                if (moveInput.y < -0.1f)
                {
                    isHanging = false;
                    anim.SetBool("IsHanging", false);
                }
                
            }
            return;
        }

        

        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        //Calculate inputed strength
        float inputMagnitude = moveDirection.magnitude;

        //set current target speed
        float targetSpeed = 0f;
        if(inputMagnitude > 0.1f)
        {
            if (isSprinting) targetSpeed = sprintSpeed;
            else targetSpeed = runSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        //execute movement
        
        controller.Move(moveDirection * targetSpeed * Time.deltaTime);

        //pass the value to Animator
        
        anim.SetFloat("Speed", inputMagnitude * targetSpeed, 0.1f, Time.deltaTime);

        // --- Jump processing ---
        if (_isGrounded && velocity.y < 0) velocity.y = -2f;

        // input space key
        if(jumpRequested && _isGrounded)
        {
            velocity.y = Mathf.Sqrt(1.5f * -2f * gravity);
            anim.SetTrigger("Jump");
        }

        jumpRequested = false;

        //processing gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        anim.SetBool("IsGrounded", _isGrounded);

        if(!_isGrounded && velocity.y < 0 && !isHanging && !isClimbing)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.4f;
            if(Physics.Raycast( rayOrigin, transform.forward, out hit, 0.6f))
            {
                if(!Physics.Raycast(rayOrigin + Vector3.up * 0.4f, transform.forward, 0.6f))
                {
                    StartHanging();
                }
            }
        }

       


    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.CompareTag("Platform"))
        {
            FallingPlatform fallingPlatform = hit.gameObject.GetComponent<FallingPlatform>();

            if(fallingPlatform != null)
            {
                fallingPlatform.TriggerFall();
            }
        }
    }
}
