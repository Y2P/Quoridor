using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;

public class ruleManager : MonoBehaviour
{

    public int[,] pathGraph = new int[81,81]; 
    public int colLength,rowLength;
    public bool pawnUpdate;
    public bool fenceUpdate;
    public GameObject p1; 
    public GameObject p2; 
    public Canvas gameOverCanvas; 

    private int x,z;
    private int[] gidx = new int[4];
    private int[] xz = new int[2];

    // Start is called before the first frame update
    void Start()
    {
        pawnUpdate = false;
        fenceUpdate = false; 
        colLength = gameObject.GetComponent<fieldInitializer>().colLength;
        rowLength = gameObject.GetComponent<fieldInitializer>().rowLength;

        initializeGrid();
        gameOverCanvas.enabled = false; 
        gameOverCanvas.GetComponent<BoxCollider>().enabled=false; 

    }

    // Update is called once per frame
    void Update()
    {
            if(fenceUpdate || pawnUpdate){


                // After a move, update the allowed paths
                initializeGrid();
                fenceUpdateGraph();
                pawnUpdateGraph();

                fenceUpdate = false;
                pawnUpdate = false;
                checkWinner();


                GetComponent<fieldInitializer>().IsMyTurn = !GetComponent<fieldInitializer>().IsMyTurn;
                drawGraph(pathGraph);
            }

        
    }




    public void initializeGrid(){
        pathGraph = new int[81,81];
        for(int i = 0 ; i < pathGraph.GetLength(0)-1 ; i++){
            if(i%colLength != 8 )
                pathGraph[i,i+1] = 1;
        }
        for(int i = 1 ; i < pathGraph.GetLength(0) ; i++){
            if(i%colLength != 0 )
                pathGraph[i,i-1] = 1;
        }
        for(int i = 0 ; i < pathGraph.GetLength(0)-colLength ; i++){
            if(i/colLength != 8 )
                pathGraph[i,i+colLength] = 1;
        }
        for(int i = colLength ; i < pathGraph.GetLength(0) ; i++){
            if(i/colLength != 0 )
                pathGraph[i,i-colLength] = 1;
        }
    }
    public void checkWinner() {
        Vector3 p1_pos = p1.transform.position;
        Vector3 p2_pos = p2.transform.position;
        if(p1_pos.x == 0 ) {
            gameOverCanvas.enabled = true;
            gameOverCanvas.GetComponent<BoxCollider>().enabled=true; 
            gameOverCanvas.transform.Find("winlose").GetComponent<TMPro.TextMeshProUGUI>().text = "You Win!!";  
            StartCoroutine(waiter());
        }else if(p2_pos.x == colLength-1 ){ 
            gameOverCanvas.enabled = true; 
            gameOverCanvas.GetComponent<BoxCollider>().enabled=true; 
            gameOverCanvas.transform.Find("winlose").GetComponent<TMPro.TextMeshProUGUI>().text = "You Lost!!";  
            StartCoroutine(waiter());
        }
        return;
    }
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(15);
        Application.Quit();

    }
    public void pawnUpdateGraph() {
        
        Vector3 p1_pos = p1.transform.position;
        Vector3 p2_pos = p2.transform.position;
        int p1_idx = tile2Graphidx(p1_pos.x,p1_pos.z);
        int p2_idx = tile2Graphidx(p2_pos.x,p2_pos.z);

        // If two pawns are neighbors.... 
        if(pathGraph[p1_idx,p2_idx] == 1) {
            // P1's neighbors are available for P2
            int[] neighborsp1 =  new int[4];
            neighborsp1[0] = p1_idx - 1;
            neighborsp1[1] = p1_idx - colLength;
            neighborsp1[2] = p1_idx + 1;
            neighborsp1[3] = p1_idx + colLength;

            for(int n = 0 ; n < 4 ; n++ ) {
                if(neighborsp1[n] < rowLength*colLength && neighborsp1[n] > 0 && neighborsp1[n] != p2_idx){
                    if(pathGraph[p1_idx,neighborsp1[n]] == 1){
                        pathGraph[p2_idx,neighborsp1[n]] = 1;
                        pathGraph[neighborsp1[n],p2_idx] = 1;
                    }
                }   
            }
            // P2's neighbors are available for P1

            int[] neighborsp2 =  new int[4];
            neighborsp2[0] = p2_idx - 1;
            neighborsp2[1] = p2_idx - colLength;
            neighborsp2[2] = p2_idx + 1;
            neighborsp2[3] = p2_idx + colLength;

            for(int n = 0 ; n < 4 ; n++ ) {
                if(neighborsp2[n] < rowLength*colLength && neighborsp2[n] > 0 && neighborsp2[n] != p1_idx){
                    if(pathGraph[p2_idx,neighborsp2[n]] == 1){
                        pathGraph[p1_idx,neighborsp2[n]] = 1;
                        pathGraph[neighborsp2[n],p1_idx] = 1;
                    }
                }   
            }
            pathGraph[p1_idx,p2_idx] = 0;
            pathGraph[p2_idx,p1_idx] = 0;
        }
    }
    public void fenceUpdateGraph() {
        GameObject[] fencesInScene = GameObject.FindGameObjectsWithTag("Respawn");
        for (int i=0; i < fencesInScene.Length;i++) {
            // Compute 2D coodinates of fence 
            x = (int)(fencesInScene[i].transform.position.x);
            z = (int)(fencesInScene[i].transform.position.z);

            // Get corresponding tile indices 
            gidx =  blockedNeighbors(x,z);
            float yangle = fencesInScene[i].transform.eulerAngles.y; 
            if(yangle > 180.0f)
                yangle -= 360.0f; 

            // According to the fence rotation, block corresponding paths between assoicated tiles
            if( yangle <= 45.0f) {
                pathGraph[gidx[0],gidx[2]] = 0;
                pathGraph[gidx[2],gidx[0]] = 0;
                pathGraph[gidx[1],gidx[3]] = 0;
                pathGraph[gidx[3],gidx[1]] = 0;
            } else   {
                
                pathGraph[gidx[0],gidx[1]] = 0;
                pathGraph[gidx[1],gidx[0]] = 0;
                pathGraph[gidx[2],gidx[3]] = 0;
                pathGraph[gidx[3],gidx[2]] = 0;
            }                
        }
    }

    public int[] blockedNeighbors(int x,int z) {
        int bottomright,bottomleft,topright,topleft;
        bottomright = x+z*rowLength;
        bottomleft = bottomright+rowLength;
        topright = bottomright+1;
        topleft = bottomleft+1;

        int[] retarr = new int[4];
        retarr[0] = bottomright;
        retarr[1] = bottomleft;
        retarr[2] = topright;
        retarr[3] = topleft;

        return retarr;
    }
    public int tile2Graphidx(float x, float z) {
        return (int)(z*rowLength+x);
    }
	public void drawGraph(int [,] graph) {
		for (int i=0; i < graph.GetLength(0);i++){ 
			for (int j=0; j < graph.GetLength(1);j++){ 
				if(graph[i,j] == 1){
					Vector3 pos1 = Graphidx2tile(i);
                    pos1.y = 1.0f;
					Vector3 pos2 = Graphidx2tile(j);
                    pos2.y = 1.0f;
					Debug.DrawLine(pos1,pos2, Color.red,25.0f);
				}
			}
        }
	}
	public Vector3 Graphidx2tile(int tileid) {
        Vector3 tilepos = new Vector3();
        tilepos.x = tileid%rowLength;
        tilepos.z = tileid/colLength;
        tilepos.y = 0.25f;
        return tilepos;
    }
}
