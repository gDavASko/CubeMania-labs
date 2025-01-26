using Unity.VisualScripting;
using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    public static MouseWorldPosition Instance { get; private set; }

    private Camera _cam = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    public Vector3 GetPosition()
    {
        Ray mouseRay = _cam.ScreenPointToRay(Input.mousePosition);

       /* if(Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            return hit.point;
        }*/

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if(plane.Raycast(mouseRay, out float distance))            
            return mouseRay.GetPoint(distance);               
        else
            return Vector3.zero;
    }
}
