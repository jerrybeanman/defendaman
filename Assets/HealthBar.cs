using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
    BaseClass baseClass;
    
    void Start() {
        baseClass = GetComponent<BaseClass>();
    }

    void OnGUI()
    {
        Vector2 targetPos = transform.position;
        targetPos = Camera.main.WorldToScreenPoint(transform.position);

        GUI.Box(new Rect(targetPos.x -30, Screen.height - targetPos.y - 46, 60, 20), baseClass.ClassStat.CurrentHp + "/" + baseClass.ClassStat.MaxHp);
    }
}