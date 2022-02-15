using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationIndexController : MonoBehaviour
{
    public GameObject[]panelList;
    public int shownGameObjectIndex;
    // Start is called before the first frame update
    void Start()
    {
        for( int i = 0 ; i < panelList.Length ; ++i )
            panelList[i].SetActive( false ) ;
        SelectNextGameObject();
        Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     // Add this function as callback for your NEXT onClick event
    public void SelectNextGameObject()
    {
        Debug.Log("Next"); 
     int index = shownGameObjectIndex >= panelList.Length - 1 ? -1 : shownGameObjectIndex ;
     SelectGameObject( index + 1 );
     
    }
    // Add this function as callback for your PREVIOUS onClick event
     public void SelectPreviousGameObject()
 {
     int index = shownGameObjectIndex <= 0 ? panelList.Length : shownGameObjectIndex ;
     SelectGameObject( index - 1 );
     Debug.Log("Previous"); 
 }
    // Add this function as callback for any INDEX event
      public void SelecCustomGameObject(string index)
 {
     //int index = shownGameObjectIndex <= 0 ? panelList.Length : shownGameObjectIndex ;
     SelectGameObject( int.Parse(index) );
     Debug.Log("Custom Index" + index); 
 }
     public void SelectGameObject( int index )
 {
     if ( shownGameObjectIndex >= 0 )
         panelList[shownGameObjectIndex].SetActive( false );
     shownGameObjectIndex = index;
     panelList[shownGameObjectIndex].SetActive( true );
 }
}
