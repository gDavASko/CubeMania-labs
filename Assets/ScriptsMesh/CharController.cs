using UnityEngine;
using UnityEngine.InputSystem;

namespace GDB.Meshes
{
    public class CharController : MonoBehaviour, GDB.BaseInputActions.IPlayerActions
    {
        [Header("Movement Settings")] public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float sensitivity = 2f;

        [Header("References")] public Transform cameraTransform; // Ссылка на камеру
        private Rigidbody rb;
        private BaseInputActions inputActions;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalRotation = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Настройка Input System
            inputActions = new BaseInputActions();
            inputActions.Player.SetCallbacks(this);
            inputActions.Enable();

            // Прячем курсор
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void Update()
        {
            RotateCamera();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        public void OnPointerPosition(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && IsGrounded())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
        } // Заглушка

        public void OnSwitchCam(InputAction.CallbackContext context)
        {
        } // Заглушка

        public void OnSit(InputAction.CallbackContext context)
        {
        } // Заглушка

        public void OnChangeCursorLock(InputAction.CallbackContext context)
        {
        } // Заглушка

        void MovePlayer()
        {
            Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection *= moveSpeed;
            rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
        }

        void RotateCamera()
        {
            float mouseX = lookInput.x * sensitivity;
            float mouseY = lookInput.y * sensitivity;

            transform.Rotate(Vector3.up * mouseX + Vector3.left * mouseY);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }

        private void OnDestroy()
        {
            inputActions.Disable();
        }
    }
}