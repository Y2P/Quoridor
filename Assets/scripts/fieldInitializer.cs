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
    private int fenceServer = 10; 
    private int fenceClient = 10; 
    public bool fenceGrabbed; 
    public bool IsMyTurn;
    // Start is called before the first frame update
    private Quaternion verticalPos; 

    void Start()
    {
        fenceGrabbed =false; 
        IsMyTurn = true; 
            // Initialize a tile grid
            for (int i = 0 ; i < colLength; i++)
            { 	    
                for (int j = 0 ; j < rowLength; j++)
                {
                    GameObject t = Instantiate(tile,new Vector3(i,0.34f,j),Quaternion.identity);

                }
            }
            // Initialize the fences

            verticalPos.eulerAngles = new Vector3(0,90.0f,0); 

            for (int i = 0 ; i < fenceServer; i++)
            { 	    
                GameObject f = Instantiate(fence,new Vector3((float)9.25,0.34f,9*(float)i/10),verticalPos);
            }
            for (int i = 0 ; i < fenceClient; i++)
            {
                GameObject f = Instantiate(fence,new Vector3((float)-1.25,0.34f,9*(float)i/10),verticalPos);

            }

        
    }

    // Update is called once per frame
    void Update()
    { 
    }
}
