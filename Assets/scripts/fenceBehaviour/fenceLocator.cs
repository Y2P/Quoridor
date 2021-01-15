using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class fenceLocator : MonoBehaviour
{
   
	public bool fenceOverlap;
	public bool targetReach; 
	public GameObject fence;    
	public Vector3 fenceOrgPos;
	public Quaternion fenceOrgRot;
	public bool canBePlaced;  
	public bool IsMyTurn; 

	public Collider[] colliders;
	public int colLength,rowLength;
	public float xoffset,zoffset;
	public int[,] tempGraph;
	private GameObject cam;
	private bool fenceGrabbed;
    // Start is called before the first frame update
    void Start()
    {
		fence =null;
		cam = GameObject.Find("camera_1");
        colLength = cam.GetComponent<fieldInitializer>().colLength;
        rowLength = cam.GetComponent<fieldInitializer>().rowLength;
		tempGraph = (int [,])(cam.GetComponent<ruleManager>().pathGraph).Clone();
		canBePlaced = false; 
    }

    // Update is called once per frame
    void Update(){
		IsMyTurn = cam.GetComponent<fieldInitializer>().IsMyTurn; 
	    if (IsMyTurn){
			// Grab a fence
			fenceGrabbed = cam.GetComponent<fieldInitializer>().fenceGrabbed; 
			if(!fence && Input.GetMouseButtonDown(0) && !fenceGrabbed) {

					fence = getFence(); 

					if(!GameObject.ReferenceEquals( fence, gameObject)){
						fence = null;
						canBePlaced = false;
					}else{

						if (  fence.transform.position.x > 0 ){
							cam.GetComponent<fieldInitializer>().fenceGrabbed = true; 
							fenceOrgPos = fence.transform.position;
							fenceOrgRot = fence.transform.rotation; 
						}else {
							fence = null;
							canBePlaced = false;
						}
					}
				
			}  
			// If grabbed, preview
			else if (fence){
				
				Vector3 pos = getWorldPoint(); 
				if( (0.0f < pos.x && pos.x < rowLength-1.5f) &&(0.0f < pos.z && pos.z < colLength-1.5f))
				{
					transform.position = snapPosition(pos);
					fenceOverlap = FenceOverlap();
					targetReach = TargetReachable();
					if(!fenceOverlap && targetReach)  {
						GetComponent<Renderer>().enabled = true; 
						canBePlaced = true; 
					} else {
						GetComponent<Renderer>().enabled = false; 
						canBePlaced = false;
					}	
				}
				else {
						GetComponent<Renderer>().enabled = false; 
						canBePlaced = false;
				}
				// Rotation 
				if(Input.GetMouseButtonDown(1)) 
				{	
					float yangle = transform.eulerAngles.y; 
					if(yangle > 180.0f)
						yangle -= 360.0f; 

					if(yangle <= 45.0f)
						transform.eulerAngles = new Vector3(0.0f,90.0f, 0.0f); 
					else 
						transform.eulerAngles = new Vector3(0.0f,0.0f, 0.0f); 
				}
				// Putting a fence
				if(Input.GetMouseButtonDown(0)){
					if(canBePlaced  ){
						fence = null;
						gameObject.tag = "Respawn";
						GetComponent<fenceLocator>().enabled = false;
						GetComponent<BoxCollider>().enabled = false;
						cam.GetComponent<ruleManager>().fenceUpdate = true;
 
					// If not feasible, put it back 
					} else {
						GetComponent<Renderer>().enabled = true;

						fence = null;
						gameObject.transform.position = fenceOrgPos;
						gameObject.transform.rotation = fenceOrgRot;
					}								
					cam.GetComponent<fieldInitializer>().fenceGrabbed = false; 

				}
			}

		}
	}
	// Function to get mouse point
	public Vector3 getWorldPoint() {
		Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray,out hit)){

			return hit.point; 		
		}
		return Vector3.zero;
	}
	public GameObject getFence() {
		Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray,out hit)){

			return hit.transform.gameObject; 		
		}
		return null;
	}
	// Function to discritize the given position  
	public Vector3 snapPosition(Vector3 original) {
		Vector3 snapped;
		snapped.x = Mathf.Round(original.x)+0.5f;
		snapped.y = (0.25f);
		snapped.z = Mathf.Round(original.z)+0.5f;	
		return snapped; 
	}
	// Function for checking fence overlap while locating
	public bool FenceOverlap(){
		GameObject[] fencesInScene = GameObject.FindGameObjectsWithTag("Respawn");
		Renderer c1,c2; 
		c2 = GetComponent<Renderer>();

		for (int i=0; i < fencesInScene.Length;i++)
        { 	
			c1 = fencesInScene[i].GetComponent<Renderer>();
			if(c1.bounds.Intersects(c2.bounds))
				return true;
        }
        return false; 
    }



	public int[,] ShortestDist(int [,] graph){
		int numvertex = colLength*rowLength;
		// int vertexset = new int[colLength*colLength]
		int[,] dist = new int[numvertex,numvertex];
		// Floyd-Warshall algorithm to shortest paths
		for(int i = 0; i < numvertex ; i++){
			dist[i,i] = 0;
			for(int j = 0; j < numvertex ; j++){
				if(graph[i,j] == 1)
					dist[i,j] = 1;
				else if(i != j)
					dist[i,j] = 100000;	
			}
		}
		for(int k = 0; k < numvertex ; k++){
			for(int i = 0; i < numvertex ; i++){
				for(int j = 0; j < numvertex ; j++){
					if( dist[i,j] > dist[i,k] + dist[k,j] ){
						dist[i,j] = dist[i,k] + dist[k,j];	
					} 
				}
			}
		}
		return dist;	
	}
	public bool TargetReachable(){
		tempGraph = (int [,])(cam.GetComponent<ruleManager>().pathGraph).Clone();
		tempGraph = UpdatePathGraph(tempGraph);

		bool p1reachable = false; 
		bool p2reachable = false; 
		int[,] dist = ShortestDist(tempGraph);
		// Get the pawn tiles
        GameObject[] playerlist = GameObject.FindGameObjectsWithTag("pawn");

		Vector3 p1pos =  playerlist[0].transform.position;
		Vector3 p2pos =  playerlist[1].transform.position;
		int p1tile = tile2Graphidx(p1pos.x, p1pos.z);
		int p2tile = tile2Graphidx(p2pos.x, p2pos.z);

		for (int i = 0; i < rowLength; i++){
			if(dist[p1tile,i*colLength] < 100000)
				p1reachable =true;	
			if( dist[p2tile,(i+1)*colLength-1] < 100000)
				p2reachable = true;
		}
		return p1reachable & p2reachable;
	}
    private int[,] UpdatePathGraph(int[,] tempGraph)
    {

		// Compute 2D coodinates of fence 
		int x = (int)(transform.position.x);
		int z = (int)(transform.position.z);

		// Get corresponding tile indices 
		int[] gidx = cam.GetComponent<ruleManager>().blockedNeighbors(x,z);
		float yangle = transform.eulerAngles.y; 
		if(yangle > 180.0f)
			yangle -= 360.0f; 

		// According to the fence rotation, block corresponding paths between assoicated tiles
		if(yangle<= 45.0f)            
		{
			tempGraph[gidx[0],gidx[2]] = 0;
			tempGraph[gidx[2],gidx[0]] = 0;
			tempGraph[gidx[1],gidx[3]] = 0;
			tempGraph[gidx[3],gidx[1]] = 0;
		} else  {
			tempGraph[gidx[0],gidx[1]] = 0;
			tempGraph[gidx[1],gidx[0]] = 0;
			tempGraph[gidx[2],gidx[3]] = 0;
			tempGraph[gidx[3],gidx[2]] = 0;
		}                
        return tempGraph;
    }
	public int tile2Graphidx(float x, float z)
    {
        return (int)((Mathf.Abs(z+zoffset))*rowLength+(Mathf.Abs(x+xoffset)));
    }

}
