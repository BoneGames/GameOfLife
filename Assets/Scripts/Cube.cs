using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    #region Variables
    // Variables
    public bool On
    {
        get {return on; }
        set {on = value;
             ChangeColor();
            if(Time.time - Mathf.Floor(Time.time) > 0.5f)
            {
                GetAllNeighbourValues();
            }
        }
    }
    private bool on;
    int closeOns;

    // References
    Manager managerScript;
    public Renderer rend;
    
    // Lists
    List<Cube> closeCubes = new List<Cube>();
    #endregion

    void Start()
    {
        rend = GetComponent<Renderer>();
        managerScript = FindObjectOfType<Manager>();

        // Get surrounding cubes
        Collider[] closeCubesStart = Physics.OverlapSphere(transform.position, 1);
        foreach (Collider cube in closeCubesStart)
        {
            if (cube.transform != transform)
            {
                closeCubes.Add(cube.GetComponent<Cube>());
            }
        }

        // Run These functions to collect data, and update visuals acordingly
        InvokeRepeating("GetNeighbourValues", 0.5f, 1);
        InvokeRepeating("ChangeState", 1, 1);
    }

    private void OnMouseOver()
    {
        managerScript.DisplayPattern(this);
    }

    public void GetAllNeighbourValues()
    {
        closeOns = 0;
        foreach (Cube cube in managerScript.allCubes)
        {
            cube.GetNeighbourValues();
        }
        
    }


    public void GetNeighbourValues()
    {
        closeOns = 0;
        foreach (Cube cube in closeCubes)
        {
            if(cube.On)
            {
                closeOns++;
            }
        }
    }

    void ChangeState()
    {
        if (on)
        {
            if (closeOns < 2 || closeOns > 3)
            {
                On = false;
            }
        }
        else
        {
            if (closeOns == 3)
            {
                On = true;
            }
        }
    }

    public void ChangeColor()
    {
        if (on)
        {
            rend.material.color = Color.white;
        }
        else
        {
            rend.material.color = Color.black;
        }
    }

    private void OnMouseDown()
    {
        // If mouse is over a UI button - don't edit grid
        if (managerScript.mouseOnButton)
        {
            return;
        }

        // if in place pattern Mode - don't place individual cubes
        if (!managerScript.patternActive)
        On = !On;
    }
}
