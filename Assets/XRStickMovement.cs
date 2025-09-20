using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class XRStickMovement : MonoBehaviour
{
    [Header("References")]
    public Transform leftStickEnd;
    public Transform rightStickEnd;
    public LayerMask groundLayer;

    [Header("Movement Settings")]
    public float pushPower = 2f;
    public float turnPower = 60f; // degrees per second
    public float velocityThreshold = 0.1f;
    public float groundCheckRadius = 0.1f;

    [Header("Sliding Physics")]
    [Range(0.8f, 1.0f)]
    public float friction = 0.98f;
    public float minMomentum = 0.01f;

    private CharacterController controller;
    private Vector3 momentum;

    private Vector3 prevLeftStickPos;
    private Vector3 prevRightStickPos;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (leftStickEnd) prevLeftStickPos = leftStickEnd.position;
        if (rightStickEnd) prevRightStickPos = rightStickEnd.position;
    }

    void Update()
    {
        // --- Ground Check ---
        bool leftTouchingGround = Physics.CheckSphere(leftStickEnd.position, groundCheckRadius, groundLayer);
        bool rightTouchingGround = Physics.CheckSphere(rightStickEnd.position, groundCheckRadius, groundLayer);

        // --- Velocity Calculation ---
        Vector3 leftVelocity = (leftStickEnd.position - prevLeftStickPos) / Time.deltaTime;
        Vector3 rightVelocity = (rightStickEnd.position - prevRightStickPos) / Time.deltaTime;
        prevLeftStickPos = leftStickEnd.position;
        prevRightStickPos = rightStickEnd.position;

        // --- Convert to local space ---
        Vector3 localLeftVel = transform.InverseTransformDirection(leftVelocity);
        Vector3 localRightVel = transform.InverseTransformDirection(rightVelocity);

        bool leftPushing = leftTouchingGround && localLeftVel.magnitude > velocityThreshold;
        bool rightPushing = rightTouchingGround && localRightVel.magnitude > velocityThreshold;

        // --- Movement Logic ---
        if (leftPushing && rightPushing)
        {
            // Both sticks push = straight forward
            float avgZ = -(localLeftVel.z + localRightVel.z) / 2f;
            if (avgZ > 0)
            {
                momentum += transform.forward * avgZ * pushPower * Time.deltaTime;
            }
        }
        else if (leftPushing && !rightPushing)
        {
            // Left stick only = turn left (NO forward push)
            transform.Rotate(Vector3.up, -turnPower * Time.deltaTime);
        }
        else if (rightPushing && !leftPushing)
        {
            // Right stick only = turn right (NO forward push)
            transform.Rotate(Vector3.up, turnPower * Time.deltaTime);
        }

        // --- Apply momentum and friction ---
        if (!controller.isGrounded)
        {
            momentum.y += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            // Only apply friction if the player is not actively pushing or turning
            bool isActivelyMoving = leftPushing || rightPushing;
            if (!isActivelyMoving)
            {
                if (momentum.magnitude > minMomentum)
                    momentum *= friction;
                else
                    momentum = Vector3.zero;
            }
        }

        controller.Move(momentum * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (leftStickEnd) Gizmos.DrawWireSphere(leftStickEnd.position, groundCheckRadius);
        if (rightStickEnd) Gizmos.DrawWireSphere(rightStickEnd.position, groundCheckRadius);
    }
}
