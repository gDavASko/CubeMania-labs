using GDB.Character;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct CursorHolder : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerInput = SystemAPI.GetSingleton<PlayerInputData>();
        
        if (Application.isFocused && playerInput.CursorLock && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
