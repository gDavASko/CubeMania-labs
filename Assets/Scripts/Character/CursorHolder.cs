using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct CursorHolder : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Application.isFocused && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (!Application.isFocused && Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
