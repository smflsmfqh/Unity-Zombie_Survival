using UnityEditor.Build.Content;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static readonly string  moveAxisName = "Vertical";
    public static readonly string  rotateAxisName = "Horizontal";
    public static readonly string  fireButtonName = "Fire1";
    public static readonly string  reloadButtonName = "Reload";

    public float move { get; private set; }
    public float rotate { get; private set; }   
    public bool fire { get; private set; }
    public bool reload { get; private set; }

    void Awake()
    {
        move = 0f;
        rotate = 0f;
        fire = false;
        reload = false;
        
    }
    void Update()
    {

        move = Input.GetAxis(moveAxisName);
        rotate = Input.GetAxis(rotateAxisName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButton(reloadButtonName);
    }
}
