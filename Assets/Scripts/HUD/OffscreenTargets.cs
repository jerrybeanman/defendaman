using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OffscreenTargets : MonoBehaviour {

    public Canvas canvas;
    public Image OffScreenSprite;
    public bool displayOffscreen;
    Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) * .5f;

    private Image offSprite;
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
            if (displayOffscreen)
            {
                PlaceOffscreen(screenpos);
            }

        }
    }

    void PlaceOffscreen(Vector3 screenpos)
    {
        float x = screenpos.x;
        float y = screenpos.y;
        float offset = 100;
        float angle = 0;

        if (screenpos.x > Screen.width) //right
        {
            angle = 90;
            x = Screen.width - offset;
        }
        if (screenpos.x < 0)
        {
            angle = -90;
            x = offset;
        }

        if (screenpos.y > Screen.height)
        {
            angle = 180;
            y = Screen.height - offset;
        }
        if (screenpos.y < 0)
        {
            y = offset;
        }

        Vector3 offscreenPos = new Vector3(x, y, 0);
        offSprite.rectTransform.position = offscreenPos;

        offSprite.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

}
