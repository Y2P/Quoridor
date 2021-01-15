using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pawnMove : MonoBehaviour
{
    private bool fenceGrabbed;
    public bool selected; 
    public int colLength,rowLength;
    public float xoffset;
    public float zoffset;
    private GameObject cam;
    private bool IsMyTurn;

    // Start is called before the first frame update

    void Start()
    {
        selected = false; 

        cam = GameObject.Find("camera_1");

        colLength = cam.GetComponent<fieldInitializer>().colLength;
        rowLength = cam.GetComponent<fieldInitializer>().rowLength;
    }
    // Update is called once per frame
    void Update()
    {

        // Ray stuff is for getting mouse input
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // We move pawns while not locating fences
        fenceGrabbed = cam.GetComponent<fieldInitializer>().fenceGrabbed;
        IsMyTurn = cam.GetComponent<fieldInitializer>().IsMyTurn; 
        if( IsMyTurn && selected && !fenceGrabbed && Physics.Raycast (ray, out hit))
        {
            // If there is a selected pawn and targetted tile 
            if(hit.transform.name != "pawn" && Input.GetMouseButtonDown(0)){
                selected = false; 
                if(hit.transform.name == "tile(Clone)") {
                    // Get tile position
                    Vector3 tilepos; 
                    tilepos.x = hit.transform.position.x;
                    tilepos.y = gameObject.transform.position.y;
                    tilepos.z = hit.transform.position.z;
                    // Get current allowed path graph 
                    int [,] pathGraph = cam.GetComponent<ruleManager>().pathGraph;
                    // Compute indices correspond to tile positions
                    int tileIdx = tile2Graphidx(tilepos.x, tilepos.z);
                    int pawnIdx = tile2Graphidx(gameObject.transform.position.x, gameObject.transform.position.z);

                    // Check path is allowed
                    if(pathGraph[pawnIdx,tileIdx] == 1)
                    {
                        // If so, go there 
                        gameObject.transform.position = tilepos; 
                        cam.GetComponent<ruleManager>().pawnUpdate = true;
                    }
                }

            }

        }
    }
    public int tile2Graphidx(float x, float z)
    {
        return (int)((Mathf.Abs(z+zoffset))*rowLength+(Mathf.Abs(x+xoffset)));
    }
}
