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

    // Update is called once per frame
    void Update()
    {
        AddjustCurrentHealth(0);
        maxHealth = (int)baseClass.ClassStat.MaxHp;
        curHealth = (int)baseClass.ClassStat.CurrentHp;
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