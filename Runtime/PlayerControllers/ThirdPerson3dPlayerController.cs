using UnityEngine;
using UnityEngine.InputSystem;

namespace QuietSam.Common
{

    public class ThirdPerson3dPlayerController : MonoBehaviour
    {
        private InputManager playerControls;

        private float verticalVelocity = 0f;

        [Header("References")]

        [SerializeField] private CharacterController controller;
        [SerializeField] private Camera boundCamera;

        [SerializeField] private LayerMask collisionLayer = ~0;

        [Header("Configuration")]

        [SerializeField] private float xSpeed = 10f;

        [SerializeField] private float gravity = -9.81f;

        [Header("Jump")]
        [SerializeField] private bool canJump = true;
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float airMultiplier = .1f;

        [SerializeField] private bool _grounded = false;
        private bool Grounded => controller != null && controller.isGrounded;

        private void Start()
        {

            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (boundCamera == null)
                boundCamera = Camera.main;

        }


        private void Update()
        {
            _grounded = Grounded;

            HandleRotation();

            ApplyGravity();
            HandleInput();
        }

        private void HandleRotation()
        {
            var move = playerControls.Player.Move.ReadValue<Vector2>();
            if (move.magnitude == 0) return;

            // --- Camera-relative WASD ---
            Vector3 camF = boundCamera ? Vector3.ProjectOnPlane(boundCamera.transform.forward, Vector3.up).normalized : transform.forward;
            Vector3 camR = boundCamera ? Vector3.ProjectOnPlane(boundCamera.transform.right, Vector3.up).normalized : transform.right;

            Vector3 input = Vector3.ClampMagnitude(camF * move.y + camR * move.x, 1f);

            Quaternion look = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 12f * Time.deltaTime);
        }

        private void HandleInput()
        {
            Move(playerControls.Player.Move.ReadValue<Vector2>());
            Debug.Log("Move: " + playerControls.Player.Move.ReadValue<Vector2>());

            if (playerControls.Player.Jump.WasPressedThisFrame())
                Jump();
        }

        private void ApplyGravity()
        {
            if (Grounded && verticalVelocity < 0f)
                verticalVelocity = -2f; // keep grounded

            verticalVelocity += gravity * Time.deltaTime;
        }

        public void Move(Vector2 direction)
        {
            var tf = transform;

            Vector3 camF = boundCamera ? Vector3.ProjectOnPlane(boundCamera.transform.forward, Vector3.up).normalized : tf.forward;
            Vector3 camR = boundCamera ? Vector3.ProjectOnPlane(boundCamera.transform.right, Vector3.up).normalized : tf.right;

            Vector3 input = Vector3.ClampMagnitude(camF * direction.y + camR * direction.x, 1f);

            float speedMultiplier = Grounded ? 1f : airMultiplier;
            Vector3 horizontal = input * xSpeed * speedMultiplier;

            Vector3 motion = new Vector3(horizontal.x, verticalVelocity, horizontal.z);
            controller.Move(motion * Time.deltaTime);
        }

        public void Jump()
        {
            if (!canJump || !Grounded)
                return;

            verticalVelocity = jumpForce;
        }

        public bool GroundCheck(out RaycastHit hitInfo)
        {
            var distance = controller.height * .5f + .2f;
            var groundCheck = Physics.Raycast(transform.position, Vector3.down, out hitInfo, distance, collisionLayer);
            return groundCheck;
        }
    }
}