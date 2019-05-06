using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Manager : MonoBehaviour
{
    #region Variables
    // Lists
    public List<Cube> allCubes = new List<Cube>();
    List<List<int>> patterns = new List<List<int>>();
    List<Button> patternButtons = new List<Button>();

    // References
    public GameObject cubePrefab;
    public Button patternButtonPrefab;
    public Canvas canvas;
    Transform camPos;

    // Variables
    public int patternSelected;
    public bool patternActive;
    public int gridSize;
    public bool mouseOnButton;
    #endregion

    #region Initialisation
    void Start()
    {
        // Start on Pause for set up
        Time.timeScale = 0;

        // Get Cam Transform.pos
        camPos = Camera.main.transform;

        // Set camera far enough back to see all cubes
        camPos.position = new Vector3(0, (float)gridSize + 1f, 0);
       
        // Generate grid of cubes
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // create square grid
        for (int  x= -gridSize/2; x <= gridSize/2; x++)
        {
            for (int z = -gridSize/2; z <= gridSize/2; z++)
            {
                // Instantiate each cube at grid position
                GameObject _cube = Instantiate(cubePrefab, new Vector3(x, 0, z), Quaternion.identity);
                // add cube to allCubes List
                allCubes.Add(_cube.GetComponent<Cube>());
                // Parent cubes to Cubes Object
                _cube.transform.SetParent(this.transform);
            }
        }
    }
    #endregion

    #region Basic Buttons
    public void Faster()
    {
        Time.timeScale += 0.2f;
    }

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

    public void Slower()
    {
        Time.timeScale -= 0.2f;
    }

    public void Pause()
    {
        if(Time.timeScale == 0)
        {
            Time.timeScale = 1;
            if((Time.time - Mathf.Floor(Time.time)) > 0.5f)
            { 
                foreach(Cube c in allCubes)
                {
                    c.GetNeighbourValues();
                }
            }
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    public void ClearAll()
    {
        foreach(Cube c in allCubes)
        {
            c.On = false;
        }
    }

    #endregion

    #region Pattern Placement
    public void SavePattern()
    {
        //Temporary List for Pattern points
        List<int> patternGet = new List<int>();
        for (int i = 0; i < allCubes.Count; i++)
        {
            // if cube is ON, add index to List <int> patternGet
            if(allCubes[i].On)
            {
                patternGet.Add(i);
            }
        }

        // if no ONs to record, exit function
        if (patternGet.Count == 0)
            return;

        // Get lowest pattern point index
        int floor = patternGet[0];

        // Apply lowest pattern point to all values in pattern (makes pattern 0-based for applying dynamically later)
        for (int i = 0; i < patternGet.Count; i++)
        {
            patternGet[i] -= floor;
        }

        // copy pattern to List of Pattern Lists
        patterns.Add(patternGet);

        // Create Button on screen to access pattern
        AddPatternGUI();
    }

    void AddPatternGUI()
    {
        // Create Button, child to Canvas
        Button newButton = Instantiate(patternButtonPrefab, canvas.transform, true);

        // Set button text display to number of patterns for visual reference
        newButton.GetComponentInChildren<Text>().text = patterns.Count.ToString();

        // if not the first pattern button created
        if(patternButtons.Count >= 1)
        {
            // set button to last button position plus offset
            newButton.transform.position = patternButtons[patternButtons.Count - 1].transform.position + new Vector3(0, -25, 0);
        }
        else
        {
            // Set button position to top right, under save pattern button
            newButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(-3, -30, 0);
        }

        // Set Method to call when button is pressed
        newButton.onClick.AddListener(SelectPatternButton);

        // Add button to List<Button> patternButtons
        patternButtons.Add(newButton);
    }

    public void SelectPatternButton()
    {
        // if same pattern button pressed
        if(int.Parse(EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text) - 1 == patternSelected)
        {
            // change patternActive bool
            patternActive = !patternActive;
        }
        // new pattern button pressed
        else
        {
            // set pattern selected
            patternSelected = int.Parse(EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text) - 1;
            patternActive = true;
        }
    }

    public void DisplayPattern(Cube mouseOverCube)
    {
        // if no pattern is currently being applied, exit function
        if (!patternActive)
            return;

        // Get the index of the Cube that called DisplayPattern() in List<Cube> allCubes
        int thisIndex = 0;
        for (int i = 0; i < allCubes.Count; i++)
        {
            if (allCubes[i] == mouseOverCube)
            {
                thisIndex = i;
                break;
            }
        }

        // Reset all cubes to show white or black based on their On bool
        foreach (Cube c in allCubes)
        {
            c.ChangeColor();
        }

        // Set the cubes that correspond with the selected pattern to appear blue;
        for (int i = 0; i < patterns[patternSelected].Count; i++)
        {
            // Make sure that pattern indexes dont go beyond the total cubes (off the right of the screen)
            if(patterns[patternSelected][i] + thisIndex < allCubes.Count)
            {
                allCubes[patterns[patternSelected][i] + thisIndex].rend.material.color = Color.blue;
            }
        }
    }

    void PlacePattern()
    {
        // If mouse is over a UI button - don't edit grid
        if(mouseOnButton)
        {
            return;
        }

        // On click while pattern activated
        if (patternActive && Input.GetMouseButtonDown(0))
        {
            foreach (Cube c in allCubes)
            {
                // find all cubes with material set to blue color (pseudo bool)
                if (c.rend.material.color == Color.blue)
                {
                    // set these cubes to ON
                    c.On = true;
                }
            }
        }
    }

    #endregion

    private void Update()
    {
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
        ShiftView(Input.mousePosition);
        PlacePattern();
    }


    #region Adjust View
    void ShiftView(Vector3 mousepos)
    {
        // if mouse on left edge, && camera is not too far to the left
        if(mousepos.x < 0 && camPos.position.x > -gridSize)
        {
            camPos.position -= new Vector3(0.1f, 0, 0);
        }
        // if mouse on right edge, && camera is not too far to the right
        if (mousepos.x > Screen.width && camPos.position.x < gridSize)
        {
            camPos.position += new Vector3(0.1f, 0, 0);
        }
        // if mouse on bottom edge, && camera is not too far down
        if (mousepos.y < 0 && camPos.position.z > -gridSize)
        {
            camPos.position -= new Vector3(0, 0, 0.1f);
        }
        // if mouse on top edge, && camera is not too far up
        if (mousepos.y > Screen.height && camPos.position.z < gridSize)
        {
            camPos.position += new Vector3(0, 0, 0.1f);
        }
    }

    void Zoom(float scroll)
    {
        if (scroll != 0)
        {
            if(scroll > 0)
            {
                // zoom cam in
                camPos.position += new Vector3(0, 0.5f, 0);
            }
            else
            {
                // zoom cam out
                camPos.position -= new Vector3(0, 0.5f, 0);
            }
        }
    }
    #endregion
}
