using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public bool mouseOnButton;
    public void MouseOverButton()
    {
        Debug.Log("true");
        mouseOnButton = true;
    }

    public void MouseOffButton()
    {
        Debug.Log("false");
        mouseOnButton = false;
    }
}
