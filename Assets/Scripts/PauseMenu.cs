using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    PauseMenu.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void SetMovementOption(int i)
--      void turnoffMenu()
--      void MainMenu()
--      void SettingOptionMenu()
--      void SetMovement(int i)
--
--
--  DATE:           Feburary , 2016
--
--  REVISIONS:      March 30, 2016
--                      -- connection using C# instead of unitiy function 
--
--  DESIGNERS:      Eunwon Moon
--
--  PROGRAMMER:     Eunwon Moon
--
--  NOTES:
--  This class is displaying menu while playing game.
--  possible to change movement control type or sound volume.
--
---------------------------------------------------------------------------------------*/

public class PauseMenu : MonoBehaviour {
 
    public GameObject MainMenuGrp;
    public GameObject SettingMenuGrp;
    
    public Slider volumeSlide;
    public Toggle absolutToggle;
    public Toggle relativeToggle;

    int movementType = 1;
    float volval;

    // Use this for initialization

    void Start()
    {
        MainMenuGrp = GameObject.Find("Settings/MainMenu");
        SettingMenuGrp = GameObject.Find("Settings/SettingMenuGrp");
        volumeSlide = GameObject.Find("Settings/SettingMenuGrp/MusicSlider").GetComponent<Slider>();
        MainMenu(); 

    }

    // Update is called once per frame

    void Update()
    {


    }


    public void SetMovementOption(int i)
    {
        var player = GameManager.instance.player;
        if (i == 1)
            player.GetComponent<Movement>().setAbs();
        else
            player.GetComponent<Movement>().setRel();
        movementType = i;
    }


    public void turnoffMenu()
    {
        MainMenuGrp.SetActive(false);
        SettingMenuGrp.SetActive(false);
    }

    public void MainMenu()
    {
        MainMenuGrp.SetActive(true);
        SettingMenuGrp.SetActive(false);
    }

    public void SettingOptionMenu()
    {
        MainMenuGrp.SetActive(false);
        SettingMenuGrp.SetActive(true);

        var player = GameManager.instance.player;
        movementType = player.GetComponent<Movement>().getInputType();

        print("move type " + movementType);

        if (movementType == 1)
        {
            absolutToggle.isOn = true;
            relativeToggle.isOn = false;
        }
        else
        {
            absolutToggle.isOn = false;
            relativeToggle.isOn = true;
        }
/*
        GameObject gM = GameObject.Find("GameManager");
        volval = gM.GetComponent<AudioSource>().volume;
        print("volume val: " + volval);
        volumeSlide.value = volval;
       */
    }

    public void SetMovement(int i)
    {
        movementType = i;

    }



}
