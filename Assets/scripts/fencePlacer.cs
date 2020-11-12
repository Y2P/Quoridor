using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fencePlacer : MonoBehaviour
{
    private GameObject f;
    public int colLength; 
    public int rowLength; 
    public GameObject fenceinst;
    private GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
		cam = GameObject.Find("camera_1");
        colLength = cam.GetComponent<fieldInitializer>().colLength;
        rowLength = cam.GetComponent<fieldInitializer>().rowLength;
    }

    // Update is called once per frame
    void Update()
    {
    // In fence mode... 
	if(cam.GetComponent<fieldInitializer>().fenceButtonClicked){
        int x = (int)Mathf.Abs(transform.position.x);
        int z = (int)Mathf.Abs(transform.position.z);

        // If left click occurs in boundaries... 
		if(Input.GetMouseButtonDown(0))
            if (GetComponent<Renderer>().enabled == true)
            {
                // Place a fence
                f = (GameObject)Instantiate(fenceinst,transform.position,transform.localRotation);
                f.tag = "Respawn";
                cam.GetComponent<fieldInitializer>().fenceButtonClicked = false;
                GetComponent<fenceLocator>().fenceCounts[cam.GetComponent<ruleManager>().turn?1:0]--;
                cam.GetComponent<ruleManager>().fenceUpdate = true;
            }
                            
        }
    }
 
}
