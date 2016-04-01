using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{

	private SpriteRenderer spriteRenderer;
	public Sprite allyHealth;
	public Sprite enemyHealth;
    // Use this for initialization
    void Start()
    {
		// this is bad, dont do it lol. ill fix it later
		spriteRenderer = transform.GetChild(0).transform.GetChild(0).GetComponent<SpriteRenderer>();

		// fix this later too
		if(transform.parent.gameObject.GetComponent<BaseClass>().team == GameData.MyPlayer.TeamID)
		{
			spriteRenderer.sprite = allyHealth;
		}else
		{
			spriteRenderer.sprite = enemyHealth;
		}
	}


	void LateUpdate()
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + 1, -10);
	}


    public void TakeDmg(float dmg)
    {

    }
}