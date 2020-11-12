using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selectionManager : MonoBehaviour
{
    public GameObject pawn1;
    public GameObject pawn2;
    public GameObject selectedPawn; 
    private Color orgColor1;
    private Color orgColor2;

    private bool fenceButtonClicked;
    private bool turn;
    // Start is called before the first frame update
    void Start()
    {
        // Save the original color of pawns for de-selection
        orgColor1 = pawn1.GetComponent<Renderer>().material.color;
        orgColor2 = pawn2.GetComponent<Renderer>().material.color;

    }

    // Update is called once per frame
    void Update()
    {
        // Ray stuff is for getting mouse input

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        fenceButtonClicked = gameObject.GetComponent<fieldInitializer>().fenceButtonClicked;
        turn = gameObject.GetComponent<ruleManager>().turn;

        // While not placing fences

        if(!fenceButtonClicked && Physics.Raycast (ray, out hit))
        {
            // Selection of the first pawn
            if(hit.transform.name == "pawn1" && Input.GetMouseButtonDown (0) && !turn)
            {
                selectedPawn = pawn1; 
                selectedPawn.GetComponent<Renderer>().material.color = Color.white;
                pawn2.GetComponent<Renderer>().material.color = orgColor2;
 
            }
            // Selection of the second pawn

            else if(hit.transform.name == "pawn2" && Input.GetMouseButtonDown (0) && turn)
            {
                selectedPawn = pawn2; 
                selectedPawn.GetComponent<Renderer>().material.color = Color.white;
                pawn1.GetComponent<Renderer>().material.color = orgColor1;

            }
            // In case of no selection 
            else if(Input.GetMouseButtonDown (0))
            {
                selectedPawn = null; 
                pawn1.GetComponent<Renderer>().material.color = orgColor1;
                pawn2.GetComponent<Renderer>().material.color = orgColor2;

            }
        } else{
            selectedPawn = null; 
            pawn1.GetComponent<Renderer>().material.color = orgColor1;
            pawn2.GetComponent<Renderer>().material.color = orgColor2;

        }
    }
}
