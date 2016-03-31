using UnityEngine;
using System.Collections;

public abstract class Area : Trigger
{
    void Start()
    {
        if (teamID != GameData.MyPlayer.TeamID)
        {
            Material hiddenMat = (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
            gameObject.layer = LayerMask.NameToLayer("HiddenThings");
            gameObject.GetComponent<SpriteRenderer>().material = hiddenMat;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {

    }
}
