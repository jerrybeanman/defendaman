using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public int maxHealth = 100;
    public int curHealth = 100;
    BaseClass baseClass;

    public float healthBarLength;

    // Use this for initialization
    void Start()
    {
        healthBarLength = Screen.width / 6;
        baseClass = GetComponent<BaseClass>();
    }


	void LateUpdate()
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + 1, -10);
	}
    void OnGUI()
    {

        // Vector2 targetPos = transform.position;
        //targetPos = Camera.main.WorldToScreenPoint(transform.position);

        //GUI.Box(new Rect(targetPos.x -30, Screen.height - targetPos.y - 46, 60, 20), curHealth + "/" + maxHealth);

    }

    public void AddjustCurrentHealth(int adj)
    {
        curHealth += adj;

        if (curHealth < 0)
            curHealth = 0;

        if (curHealth > maxHealth)
            curHealth = maxHealth;

        if (maxHealth < 1)
            maxHealth = 1;

        healthBarLength = (Screen.width / 6) * (curHealth / (float)maxHealth);
    }
}