using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;
public class ruleManager : MonoBehaviour
{
    public int[,] pathGraph = new int[81,81]; 
    private GameObject[] fencesInScene;
    public int colLength,rowLength;

    public bool turn; // false for the first, true for the second
    public bool pawnUpdate;
    public bool fenceUpdate;
    private int x,z;
    private int[] gidx = new int[4];
    private int[] xz = new int[2];
    private Vector3 point;
    private float speedMod;

    // Start is called before the first frame update
    void Start()
    {
        pawnUpdate = false;
        colLength = gameObject.GetComponent<fieldInitializer>().colLength;
        rowLength = gameObject.GetComponent<fieldInitializer>().rowLength;
        point = new Vector3(4f,0.0f,4f);
        speedMod = 10.0f;
        initializeGrid();
        

    }

    // Update is called once per frame
    void Update()
    {
        if(fenceUpdate || pawnUpdate){
            // After placing a fence, update the allowed paths
            turn = !turn;
            fencesInScene = GameObject.FindGameObjectsWithTag("Respawn");
            initializeGrid();
            fenceUpdateGraph(fencesInScene);
            pawnUpdateGraph();

            fenceUpdate = false;
            pawnUpdate = false;

            drawGraph(pathGraph);
            checkWinner();
        }

        if(turn == true){
            Vector3 targetPosition = new Vector3(-3f, 8.0f, 4f);
            float distToTarget = Vector3.Distance(transform.position,targetPosition);
            if( distToTarget>= 0.01f )
                transform.RotateAround (point,new Vector3(0.0f,1.0f,0.0f),distToTarget * Time.deltaTime * speedMod);                

        }else {
            Vector3 targetPosition = new Vector3(11f, 8.0f, 4f);
            float distToTarget = Vector3.Distance(transform.position,targetPosition);
            if( distToTarget>= 0.01f )
                transform.RotateAround (point,new Vector3(0.0f,1.0f,0.0f),distToTarget * Time.deltaTime * speedMod);                
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
    public void checkWinner()
    {
        GameObject p1 = GameObject.FindGameObjectsWithTag("pawn1")[0];
        GameObject p2 = GameObject.FindGameObjectsWithTag("pawn2")[0];
        if(p1.transform.position.x == 0 ){
            if(EditorUtility.DisplayDialog ("P1 Wins", "P1: Winner winner... chicken in the dinner", "Ok")){
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
            } 
        }else if(p2.transform.position.x == colLength-1 ){
            if(EditorUtility.DisplayDialog ("P2 Wins", "P2: Winner winner... chicken in the dinner", "Restart")){
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
            } 
        }
        return;
    }
    public void pawnUpdateGraph(){
        Vector3 p1 = gameObject.GetComponent<selectionManager>().pawn1.transform.position; 
        Vector3 p2 = gameObject.GetComponent<selectionManager>().pawn2.transform.position; 

        int p1_idx = tile2Graphidx(p1.x,p1.z);
        int p2_idx = tile2Graphidx(p2.x,p2.z);

        // If two pawns are neighbors.... 
        if(pathGraph[p1_idx,p2_idx] == 1){
            // P1's neighbors are available for P2
            int[] neighborsp1 =  new int[4];
            neighborsp1[0] = p1_idx - 1;
            neighborsp1[1] = p1_idx - colLength;
            neighborsp1[2] = p1_idx + 1;
            neighborsp1[3] = p1_idx + colLength;

            for(int n = 0 ; n < 4 ; n++ ){
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

            for(int n = 0 ; n < 4 ; n++ ){
                if(neighborsp2[n] < rowLength*colLength && neighborsp2[n] > 0 && neighborsp2[n] != p1_idx){
                    if(pathGraph[p2_idx,neighborsp2[n]] == 1){
                        pathGraph[p1_idx,neighborsp2[n]] = 1;
                        pathGraph[neighborsp2[n],p1_idx] = 1;
                    }
                }   
            }
        }
        // int jump_idx = 2*otherp_idx - turnp_idx;
        // pathGraph[turnp_idx,otherp_idx] = 0;
        // pathGraph[otherp_idx,turnp_idx] = 0;
        // if(pathGraph[otherp_idx,jump_idx] == 1){
        //     pathGraph[turnp_idx,jump_idx] = 1;
        //     pathGraph[jump_idx,turnp_idx] = 1;
        // }
    }
    public void fenceUpdateGraph(GameObject[] fencesInScene)
    {

        for (int i=0; i < fencesInScene.Length;i++)
        {
            // Compute 2D coodinates of fence 
            x = (int)(fencesInScene[i].transform.position.x);
            z = (int)(fencesInScene[i].transform.position.z);

            // Get corresponding tile indices 
            gidx =  blockedNeighbors(x,z);

            // According to the fence rotation, block corresponding paths between assoicated tiles
            if(fencesInScene[i].transform.eulerAngles.y == 0.0f)            
            {
                pathGraph[gidx[0],gidx[2]] = 0;
                pathGraph[gidx[2],gidx[0]] = 0;
                pathGraph[gidx[1],gidx[3]] = 0;
                pathGraph[gidx[3],gidx[1]] = 0;
            } else if(fencesInScene[i].transform.eulerAngles.y == 90.0f)  {
                pathGraph[gidx[0],gidx[1]] = 0;
                pathGraph[gidx[1],gidx[0]] = 0;
                pathGraph[gidx[2],gidx[3]] = 0;
                pathGraph[gidx[3],gidx[2]] = 0;
            }                
        }
        
    }
    // int[,] modifyPathGraph(int[,] originalPathGraph, bool turn){
    //     Vector3 otherplayerpos;
    //     int otherplayeridx;
    //     int[,] modifiedPathGraph = originalPathGraph;
    //     if(turn){
    //         otherplayerpos = gameObject.GetComponent<selectionManager>().pawn2.transform.position;
    //         otherplayeridx = tile2Graphidx(otherplayerpos.x,otherplayerpos.z);
    //     }else{
    //         otherplayerpos = gameObject.GetComponent<selectionManager>().pawn2.transform.position;
    //         otherplayeridx = tile2Graphidx(otherplayerpos.x,otherplayerpos.z);
    //     }
    //     modifiedPathGraph[otherplayeridx,otherplayeridx+1] = 0;
    //     modifiedPathGraph[otherplayeridx,otherplayeridx+colLength] = 0;
    //     modifiedPathGraph[otherplayeridx,otherplayeridx-1] = 0;
    //     modifiedPathGraph[otherplayeridx,otherplayeridx-colLength] = 0;


    //     modifiedPathGraph[otherplayeridx-1,otherplayeridx+1] = 1;
    //     modifiedPathGraph[otherplayeridx+1,otherplayeridx-1] = 1;
    //     modifiedPathGraph[otherplayeridx-1,otherplayeridx+1] = 1;
    //     modifiedPathGraph[otherplayeridx+1,otherplayeridx-1] = 1;

    //     modifiedPathGraph[otherplayeridx,otherplayeridx-1] = 1;
    //     modifiedPathGraph[otherplayeridx,otherplayeridx-1] = 1;

    //     return modifiedPathGraph;
    // }
    public int[] blockedNeighbors(int x,int z)
    {
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
    public int tile2Graphidx(float x, float z)
    {
        return (int)(z*rowLength+x);
    }
	public void drawGraph(int [,] graph){
		GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");

		for (int i=0; i < graph.GetLength(0);i++){ 
			for (int j=0; j < graph.GetLength(1);j++){ 
				if(graph[i,j] == 1){
					Vector3 pos1 = Graphidx2tile(i);
					Vector3 pos2 = Graphidx2tile(j);
					Debug.DrawLine(pos1,pos2, Color.blue,10.0f);
				}
			}
        }
	}
	public Vector3 Graphidx2tile(int tileid)
    {
        Vector3 tilepos = new Vector3();
        tilepos.x = tileid%rowLength;
        tilepos.z = tileid/colLength;
        tilepos.y = 0.25f;
        return tilepos;
    }
}
