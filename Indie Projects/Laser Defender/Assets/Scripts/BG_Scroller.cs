using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BG_Scroller : MonoBehaviour
{
    /*
     here we are using a 3D object - Quad in dimention of requiewd bg size
    which help us in making a scolling animation in bg. we can delete the previous
    bg UI by spite renderer, in the properties of the bg sprite, change:
    Texture Type: Default       &       Wrap Mode: Repeat
    this way we add a material out this texture in 3D objects by simply 
    dragging and dropping over the Quad 3D-obj. Then in the material change:
    Shadder: Unlit/Texture to keep a bright bg overlayed with bg texture image
    that was added. Now the properties of this material itself acts as scrolling
    property by simply adjusting the offset vector (here scrolling in y axis)
     */
    [SerializeField] Vector2 OffsetScrollSpeed = new Vector2(0f,0.25f);
    Material bgMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // getting the material used by the mesh renderer
        bgMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        bgMaterial.mainTextureOffset += OffsetScrollSpeed * Time.deltaTime;
    }
}
