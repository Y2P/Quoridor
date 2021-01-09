using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class pawnHighlight: NetworkBehaviour
{

    //When the mouse hovers over the GameObject, it turns to this color (red)
    Color m_MouseOverColor = Color.white;

    //This stores the GameObject’s original color
    Color m_OriginalColor;

    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;
    public void Start()
    {
        if(isLocalPlayer)
            GetComponent<MeshRenderer>().material.color = Color.green;

        m_Renderer = GetComponent<MeshRenderer>();
        m_OriginalColor = m_Renderer.material.color;
    }
    

    void OnMouseDown()
    {
        // Change the color of the GameObject to red when the mouse is over GameObject
        m_Renderer.material.color = m_MouseOverColor;
        gameObject.GetComponent<pawnMove>().selected = true;
    }

    void Update()
    {
        // Reset the color of the GameObject back to normal
        if( Input.GetMouseButtonDown(0) )
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;
            if( Physics.Raycast( ray, out hit, 100 ) ){
                if( !GameObject.ReferenceEquals(gameObject,hit.transform.gameObject) )
                {
                    m_Renderer.material.color = m_OriginalColor;
                }
            }
        }

    }
}