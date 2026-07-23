using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Awake()
    { 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
}