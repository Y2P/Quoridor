using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class fieldInitializer : MonoBehaviour
{
    public int colLength = 9; 
    public int rowLength = 9; 

    public GameObject tile;
    public GameObject fence;
    public bool fenceButtonClicked;
    private int[] fenceCounts;
    // Start is called before the first frame update
    void Start()
    {
	    fenceButtonClicked = false;
        // Initialize a tile grid
        for (int i = 0 ; i < colLength; i++)
    	{ 	    
            for (int j = 0 ; j < rowLength; j++)
            {
                GameObject t = Instantiate(tile,new Vector3(i,0.25f,j),Quaternion.identity);

            }
	    }
   
    }

    // Update is called once per frame
    void Update()
    { 
    }
    // Fence button gui 
	void OnGUI() 
	{   
        GUIStyle customButton = new GUIStyle("button");
        customButton.fontSize = 35;
	    if(GUI.Button(new Rect(100, 70, 180, 100), "Put Fence",customButton)){
		    fenceButtonClicked = !fenceButtonClicked;
	    }
        fence = GameObject.Find("fence");
        fenceCounts = fence.GetComponent<fenceLocator>().fenceCounts;
        bool turn = gameObject.GetComponent<ruleManager>().turn; 
        string stringToEdit =  "P1: Remaining Fences: "+ (fenceCounts[0]).ToString()+ "\n" +"P2: Remaining Fences: "+ (fenceCounts[1]).ToString();  ;
        GUIStyle customtextfield = new GUIStyle("textField");
        customtextfield.fontSize = 35;
        stringToEdit = GUI.TextField(new Rect(500, 70, 500, 100), stringToEdit,customtextfield);
        if(GUI.Button(new Rect(300, 70, 150, 100), "Restart",customButton)){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
	    }
	}

}
