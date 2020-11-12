using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pawnMove : MonoBehaviour
{
    private GameObject selectedPawn;
    private bool fenceButtonClicked;
    public int colLength,rowLength;
    public float xoffset;
    public float zoffset;

    // Start is called before the first frame update
    void Start()
    {
        colLength = gameObject.GetComponent<fieldInitializer>().colLength;
        rowLength = gameObject.GetComponent<fieldInitializer>().rowLength;

    }

    // Update is called once per frame
    void Update()
    {

        // Get the selected pawn 
        selectedPawn = gameObject.GetComponent<selectionManager>().selectedPawn;
        // Ray stuff is for getting mouse input
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // We move pawns while not locating fences
        fenceButtonClicked = gameObject.GetComponent<fieldInitializer>().fenceButtonClicked;
        
        if(!fenceButtonClicked && Physics.Raycast (ray, out hit))
        {
            // If there is a selected pawn and targetted tile 
            if(selectedPawn != null && hit.transform.name == "tile(Clone)" && Input.GetMouseButtonDown (0))
            {
                // Get tile position
                Vector3 tilepos; 
                tilepos.x = hit.transform.position.x;
                tilepos.y = selectedPawn.transform.position.y;
                tilepos.z = hit.transform.position.z;
                // Get current allowed path graph 
                int [,] pathGraph = gameObject.GetComponent<ruleManager>().pathGraph;
                // Compute indices correspond to tile positions
                int tileIdx = tile2Graphidx(tilepos.x, tilepos.z);
                int pawnIdx = tile2Graphidx(selectedPawn.transform.position.x, selectedPawn.transform.position.z);

                // Check path is allowed
                if(pathGraph[pawnIdx,tileIdx] == 1)
                {
                    // If so, go there 
                    selectedPawn.transform.position = tilepos; 
                    gameObject.GetComponent<ruleManager>().pawnUpdate = true;
                }
                // Release the selected pawn
                selectedPawn = null;
            }
        }
    }
    public int tile2Graphidx(float x, float z)
    {
        return (int)((Mathf.Abs(z+zoffset))*rowLength+(Mathf.Abs(x+xoffset)));
    }
}
