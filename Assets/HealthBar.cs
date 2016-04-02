using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{

	private SpriteRenderer 	spriteRenderer;
	private BaseClass 		baseClass;
	private float 			TotalHealth;
	private GameObject 		holder;

	public 	Sprite allyHealth;
	public 	Sprite enemyHealth;
    // Use this for initialization
    void Start()
    {
		holder = transform.GetChild(0).gameObject;
		// this is bad, dont do it lol. ill fix it later
		spriteRenderer 	= holder.transform.GetChild(0).GetComponent<SpriteRenderer>();
		baseClass 		= transform.parent.gameObject.GetComponent<BaseClass>();
		TotalHealth		= baseClass.ClassStat.MaxHp;

		// fix this later too
		if(baseClass.team == GameData.MyPlayer.TeamID)
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

    public void UpdateHealth(float MaxHp, float CurrentHp)
    {
		Debug.Log("Current:" + CurrentHp + " Max: " + MaxHp);
		holder.transform.localScale = new Vector3(CurrentHp / MaxHp, holder.transform.localScale.y, holder.transform.localScale.z);
    }
}