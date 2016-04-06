using UnityEngine;
using System.Collections;

public class SettingScript : MonoBehaviour {

    public GameObject settings;

    void Start()
    {
        settings.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settings.activeSelf) {
                settings.SetActive(false);
                GameData.MouseBlocked = false;
            } else {
                settings.SetActive(true);
                GameData.MouseBlocked = true;
            }
        }
    }
    public void setttingClose()
    {
        settings.SetActive(false);
        GameData.MouseBlocked = false;
    }
}
