using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class OffscreenTargets : MonoBehaviour
{

    public Canvas canvas;
    public Image OffScreenSprite;
    [Range(0.1f, 1.0f)]
    public float offset = 0.7f;
    [Range(0.1f, 1.0f)]
    public float indicator_scale = 1.0f;

    private Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) * .5f;
    private Vector3 offScreen = new Vector3(-100, -100, 0);

    private Image offSprite;

    private Rect centerRect;
    // Use this for initialization
    void Start()
    {
        offSprite = Instantiate(OffScreenSprite, offScreen, Quaternion.Euler(new Vector3(0, 0, 0))) as Image;
        offSprite.rectTransform.parent = canvas.transform;
        offSprite.transform.localScale *= indicator_scale;

        centerRect.width = Screen.width / 2;
        centerRect.height = Screen.height / 2;
        centerRect.position = new Vector2(screenCenter.x - centerRect.width / 2, screenCenter.y - centerRect.height / 2);
    }

    // late update is called every frame after Update() finishes
    void LateUpdate()
    {
        PlaceIndicators();
    }

    void PlaceIndicators()
    {
        Vector3 targetPos;

        if (GameData.AllyKingID != -1)
        {
            targetPos = GameData.PlayerPosition[GameData.AllyKingID];
            targetPos = Camera.main.WorldToScreenPoint(targetPos);

            if (targetPos.z > 0 && targetPos.x < Screen.width &&
                targetPos.x > 0 && targetPos.y < Screen.height && targetPos.y > 0)
            {
                offSprite.rectTransform.position = offScreen;   //get rid of the arrow indicator
            }
            else {
                PlaceOffscreen(targetPos, offSprite);
            }
        }
    }

    void PlaceOffscreen(Vector3 screenpos, Image indicator)
    {
        Vector3 indicationPos = screenpos;
        screenpos -= screenCenter;

        float angle = Mathf.Atan2(screenpos.y, screenpos.x);
        angle = angle * Mathf.Rad2Deg;

        float slope = screenpos.y / screenpos.x;

        Vector3 screenBounds = screenCenter * offset; // offset from edge value

        if (screenpos.y < 0)
        {
            indicationPos.x = -screenBounds.y / slope;
            indicationPos.y = -screenBounds.y;
        }
        else
        {
            indicationPos.x = screenBounds.y / slope;
            indicationPos.y = screenBounds.y;
        }

        if (indicationPos.x < -screenBounds.x)
        {
            indicationPos.x = -screenBounds.x;
            indicationPos.y = slope * -screenBounds.x;
        }
        else if (indicationPos.x > screenBounds.x)
        {
            indicationPos.x = screenBounds.x;
            indicationPos.y = slope * screenBounds.x;
        }

        indicationPos += screenCenter;
        screenpos += screenCenter;

        indicator.rectTransform.position = indicationPos;
        indicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

}
