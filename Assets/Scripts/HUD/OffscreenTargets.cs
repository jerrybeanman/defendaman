using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OffscreenTargets : MonoBehaviour {

    public Canvas canvas;
    public Image OffScreenSprite;
    [Range(0.1f, 1.0f)]
    public float offset = 0.7f;

    private Image offSprite;
    private Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) * .5f;
    private Vector3 offScreen = new Vector3(-100, -100, 0);

    private Rect centerRect;
    // Use this for initialization
    void Start()
    {
        offSprite = Instantiate(OffScreenSprite, offScreen, Quaternion.Euler(new Vector3(0, 0, 0))) as Image;
        offSprite.rectTransform.parent = canvas.transform;
        centerRect.width = Screen.width / 2;
        centerRect.height = Screen.height / 2;
        centerRect.position = new Vector2(screenCenter.x - centerRect.width / 2, screenCenter.y - centerRect.height / 2);
    }

    void Update()
    {
        PlaceIndicators();
    }

    void PlaceIndicators()
    {

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");

        if (objects.Length == 0)
        {
            return;
        }
        List<GameObject> players = new List<GameObject>();
        
        foreach(GameObject g in objects)
        {
            // forced to do this check due to bad tag naming...
            if (!g.ToString().Contains("Holder"))
            {
                players.Add(g);
            }
        }

        Vector3 screenpos = Camera.main.WorldToScreenPoint(players[1].transform.position);

        //if onscreen
        if (screenpos.z > 0 && screenpos.x < Screen.width && screenpos.x > 0 && screenpos.y < Screen.height && screenpos.y > 0)
        {
            offSprite.rectTransform.position = offScreen;//get rid of the arrow indicator
        }
        else {
            PlaceOffscreen(screenpos);
        }
    }

    void PlaceOffscreen(Vector3 screenpos)
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
        } else
        {
            indicationPos.x = screenBounds.y / slope;
            indicationPos.y = screenBounds.y;
        }

        if (indicationPos.x < -screenBounds.x)
        {
            indicationPos.x = -screenBounds.x;
            indicationPos.y = slope * -screenBounds.x;
        } else if (indicationPos.x > screenBounds.x)
        {
            indicationPos.x = screenBounds.x;
            indicationPos.y = slope * screenBounds.x;
        }

        indicationPos += screenCenter;
        screenpos += screenCenter;

        offSprite.rectTransform.position = indicationPos;
        offSprite.rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

}
