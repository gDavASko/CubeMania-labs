using UnityEngine;
using UnityEngine.InputSystem;

namespace GDB.Meshes
{
    public class CharController : MonoBehaviour, GDB.BaseInputActions.IPlayerActions
    {
        [Header("Movement Settings")] 
        public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float sensitivity = 2f;

        [Header("Block Interaction Settings")]
        public float blockDestroyFrequency = 0.01f; // Time between block destroy actions when held (seconds)
        public float blockCreateFrequency = 0.01f;  // Time between block create actions when held (seconds)

        [Header("References")] 
        public Transform cameraTransform; // Ссылка на камеру
        public GameWorld gameWorld; // Reference to the GameWorld
        
        private Rigidbody rb;
        private BaseInputActions inputActions;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalRotation = 0f;

        // Block interaction timers
        private float lastDestroyTime = 0f;
        private float lastCreateTime = 0f;
        private bool isDestroyPressed = false;
        private bool isCreatePressed = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Настройка Input System
            inputActions = new BaseInputActions();
            inputActions.Player.SetCallbacks(this);
            inputActions.Enable();

            // Find GameWorld if not assigned
            if (gameWorld == null)
                gameWorld = FindObjectOfType<GameWorld>();

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
            HandleContinuousBlockInteractions();
        }

        private void HandleContinuousBlockInteractions()
        {
            // Handle continuous block destruction
            if (isDestroyPressed && Time.time >= lastDestroyTime + blockDestroyFrequency)
            {
                lastDestroyTime = Time.time;
                DestroyBlockAtCrosshair();
            }

            // Handle continuous block creation
            if (isCreatePressed && Time.time >= lastCreateTime + blockCreateFrequency)
            {
                lastCreateTime = Time.time;
                CreateBlockAtCrosshair();
            }
        }

        private void DestroyBlockAtCrosshair()
        {
            if (gameWorld == null) return;
            
            var ray = cameraTransform.GetComponent<Camera>().ViewportPointToRay(Vector3.one * 0.5f);

            if (Physics.Raycast(ray, out var hit))
            {
                Vector3 blockPos = hit.point - hit.normal * MeshBuilder.BlockScale * 0.5f;
                Vector3Int blockWPos = Vector3Int.FloorToInt(blockPos / MeshBuilder.BlockScale);
                Vector2Int chunkPos = gameWorld.GetChunkContaisBlock(blockWPos);

                if (gameWorld.ChunkDatas.TryGetValue(chunkPos, out var chunkData))
                {
                    Vector3Int chunkOrig = new Vector3Int(chunkPos.x, 0, chunkPos.y) * MeshBuilder.ChunkWidth;
                    chunkData.Renderer.DestroyBlock(blockWPos - chunkOrig);
                }
            }
        }

        private void CreateBlockAtCrosshair()
        {
            if (gameWorld == null) return;
            
            var ray = cameraTransform.GetComponent<Camera>().ViewportPointToRay(Vector3.one * 0.5f);

            if (Physics.Raycast(ray, out var hit))
            {
                Vector3 blockPos = hit.point + hit.normal * MeshBuilder.BlockScale * 0.5f;
                Vector3Int blockWPos = Vector3Int.FloorToInt(blockPos / MeshBuilder.BlockScale);
                Vector2Int chunkPos = gameWorld.GetChunkContaisBlock(blockWPos);

                if (gameWorld.ChunkDatas.TryGetValue(chunkPos, out var chunkData))
                {
                    Vector3Int chunkOrig = new Vector3Int(chunkPos.x, 0, chunkPos.y) * MeshBuilder.ChunkWidth;
                    chunkData.Renderer.SpawnBlock(blockWPos - chunkOrig);
                }
            }
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
            // Track button state for continuous destruction
            if (context.started)
            {
                isDestroyPressed = true;
                lastDestroyTime = Time.time - blockDestroyFrequency; // Allow immediate first action
                DestroyBlockAtCrosshair(); // Immediate first action
            }
            else if (context.canceled)
            {
                isDestroyPressed = false;
            }
        }

        public void OnSwitchCam(InputAction.CallbackContext context)
        {
        } // Заглушка

        public void OnSit(InputAction.CallbackContext context)
        {
        } // Заглушка

        public void OnChangeCursorLock(InputAction.CallbackContext context)
        {
            // Заглушка
        }

        public void OnCreate(InputAction.CallbackContext context)
        {
            // Track button state for continuous creation
            if (context.started)
            {
                isCreatePressed = true;
                lastCreateTime = Time.time - blockCreateFrequency; // Allow immediate first action
                CreateBlockAtCrosshair(); // Immediate first action
            }
            else if (context.canceled)
            {
                isCreatePressed = false;
            }
        }

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
            
            transform.Rotate(Vector3.up * mouseX);

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