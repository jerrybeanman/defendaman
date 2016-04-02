using UnityEngine;
using System.Collections;

public abstract class Projectile : Trigger
{
    private Vector2 startPos;
    public int maxDistance;
    public int pierce = 0;
    
    protected void Start()
    {
        startPos = transform.position;
        if(GameData.MyPlayer == null)
        {
            Debug.Log("caught null");
            return;
        }
        if (teamID != GameData.MyPlayer.TeamID)
        {
            Material hiddenMat = (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
            gameObject.layer = LayerMask.NameToLayer("HiddenThings");
            gameObject.GetComponent<SpriteRenderer>().material = hiddenMat;
        }
    }

    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //If its a player or an AI, ignore it, otherwise destroy itself
        var player = other.gameObject.GetComponent<BaseClass>();
        if (player != null && teamID == player.team)
        {
            //If it collided with a player
            return;
        }

        var trigger = other.gameObject.GetComponent<Trigger>();
        if (trigger != null && (teamID == trigger.teamID || trigger is Area))
        {
            //If it collided with another projectile or a sword
            return;
        }

        var ai = other.gameObject.GetComponent<AI>();
        if (ai != null && teamID == ai.team)
        {
            
            //If it collided with AI
            return;
        }

        if (other.gameObject.GetComponent<WorldItemData>() != null)
        {
            //If it collided with items
            return;
        }
        //Otherwise, its a wall or some solid

		if(other.tag == "HealthBar")
		{
			return;
		}
        if (--pierce < 0) 
		{
            Destroy(gameObject);
        }
    }
}
