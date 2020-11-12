using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fenceLocator : MonoBehaviour
{
   
	private GameObject fencepreview; 
    public bool fenceButtonClicked;	
	public bool fenceOverlap;
	public Collider[] colliders;
	public int colLength,rowLength;
	public float xoffset,zoffset;
	public int[] fenceCounts ; 
	public int[,] tempGraph;
	private GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
		cam = GameObject.Find("camera_1");
        colLength = cam.GetComponent<fieldInitializer>().colLength;
        rowLength = cam.GetComponent<fieldInitializer>().rowLength;
		fenceCounts = new int[2];
		fenceCounts[0] = 10;
		fenceCounts[1] = 10;
		tempGraph = (int [,])(cam.GetComponent<ruleManager>().pathGraph).Clone();
    }

    // Update is called once per frame
    void Update(){
		if(fenceCounts[cam.GetComponent<ruleManager>().turn?1:0]==0){
			cam.GetComponent<fieldInitializer>().fenceButtonClicked = false;
			fenceButtonClicked = false;
		}
		else{
			// Check fence mode	
			fenceButtonClicked = cam.GetComponent<fieldInitializer>().fenceButtonClicked;
		
			if(fenceButtonClicked == true) // In fence mode ...
			{
				// Get mouse position 
				Vector3 pos = getWorldPoint(); 
				
				// Within boundaries
				if( (0.0f < pos.x && pos.x < rowLength-1.5f) &&(0.0f < pos.z && pos.z < colLength-1.5f))
				// Preview the fence
				{
					transform.position = snapPosition(pos);
					// Make the fence invisible if overlapping. I use this variable while placing 
					GetComponent<Renderer>().enabled = !FenceOverlap() && TargetReachable();
				}

				else {
					// Otherwise, put it out of scene. 
					GetComponent<Renderer>().enabled = false;
				}
				// Rotate the fence by right click
				if(Input.GetMouseButtonDown(1)) 
				{	
					if(transform.eulerAngles.y == 0.0f)
						transform.eulerAngles = new Vector3(0.0f,90.0f, 0.0f); 
					else if(transform.eulerAngles.y == 90.0f)
						transform.eulerAngles = new Vector3(0.0f,0.0f, 0.0f); 

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
		Collider c1,c2; 
		c2 = GetComponent<Collider>();

		for (int i=0; i < fencesInScene.Length;i++)
        { 

			c1 = fencesInScene[i].GetComponent<Collider>();
			if(c1.bounds.Intersects(c2.bounds))
				return true;
        }
        return false; 
    }
	// public void tileDistance(int [,] dist,int [,] graph){
	// 	GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");

	// 	for (int i=0; i < tiles.Length;i++){ 
	// 		TextMesh t = tiles[i].gameObject.GetComponent<TextMesh>();
	// 		Vector3 tilepos = tiles[i].transform.position;
	// 		int tileidx = cam.GetComponent<pawnMove>().tile2Graphidx(tilepos.x, tilepos.z);
	// 		Vector3 p1pos =  GameObject.Find("pawn1").transform.position;
	// 		int p1tile = cam.GetComponent<pawnMove>().tile2Graphidx(p1pos.x, p1pos.z);

	// 		t.text = tileidx.ToString();// (dist[tileidx,p1tile].ToString());
    //     }
	// }



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
	    // tileDistance(dist,tempGraph);
		// Get the pawn tiles

		Vector3 p1pos =  GameObject.Find("pawn1").transform.position;
		Vector3 p2pos =  GameObject.Find("pawn2").transform.position;
		int p1tile = cam.GetComponent<pawnMove>().tile2Graphidx(p1pos.x, p1pos.z);
		int p2tile = cam.GetComponent<pawnMove>().tile2Graphidx(p2pos.x, p2pos.z);

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

		// According to the fence rotation, block corresponding paths between assoicated tiles
		if(transform.eulerAngles.y == 0.0f)            
		{
			tempGraph[gidx[0],gidx[2]] = 0;
			tempGraph[gidx[2],gidx[0]] = 0;
			tempGraph[gidx[1],gidx[3]] = 0;
			tempGraph[gidx[3],gidx[1]] = 0;
		} else if(transform.eulerAngles.y == 90.0f)  {
			tempGraph[gidx[0],gidx[1]] = 0;
			tempGraph[gidx[1],gidx[0]] = 0;
			tempGraph[gidx[2],gidx[3]] = 0;
			tempGraph[gidx[3],gidx[2]] = 0;
		}                
        return tempGraph;
    }
}
