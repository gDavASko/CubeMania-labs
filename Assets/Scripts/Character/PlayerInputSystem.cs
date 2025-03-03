using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GDB.Character
{
    [BurstCompile]
    public partial class PlayerInputSystem : SystemBase
    {
        private BaseInputActions playerInput;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction shootAction;
        private InputAction sitAction;
        private InputAction toggleViewAction;

        protected override void OnCreate()
        {
            base.OnCreate();
            playerInput = new BaseInputActions();
            moveAction = playerInput.Player.Move;
            lookAction = playerInput.Player.PointerPosition;
            jumpAction = playerInput.Player.Jump;
            shootAction = playerInput.Player.Shoot;
            sitAction = playerInput.Player.Sit;
            toggleViewAction = playerInput.Player.SwitchCam;

            playerInput.Enable();
        }

        protected override void OnUpdate()
        {
            foreach (var input in SystemAPI.Query<RefRW<PlayerInputData>>())
            {
                input.ValueRW.Move = moveAction.ReadValue<Vector2>();
                input.ValueRW.Look = lookAction.ReadValue<Vector2>();
                input.ValueRW.Jump = jumpAction.WasPressedThisFrame();
                input.ValueRW.ShootButton = shootAction.WasPressedThisFrame();
                input.ValueRW.Sit = sitAction.IsPressed();
            
                if (toggleViewAction.WasPressedThisFrame())
                {
                    input.ValueRW.CamChangeNeedProcess = true;
                }
            }
        }
    }
}