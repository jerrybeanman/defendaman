using UnityEngine;
using System.Collections;

public class HUDScript : MonoBehaviour {

    public GameObject HealthBar;

    public float max_health = 100f;
    public float health = 0f;

    void Start()
    {
        health = max_health;
    }

    public void DecreaseHealth()
    {
        health -= 2f;
        float new_health = health / max_health;
        if (health >= 0)
        {
            _UpdateHealthBar(new_health);
        }
    }

    private void _UpdateHealthBar(float health)
    {
        HealthBar.transform.localScale = new Vector3(health, HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
    }
}
