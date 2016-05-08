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
--      void quit()
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



    /*---------------------------------------------------------------------------------------------------------------------
   -- FUNCTION: Start
   --
   -- DATE: March 10, 2016
   --
   -- REVISIONS: 
   --
   -- DESIGNER: Eunwon Moon
   --
   -- PROGRAMMER: Eunwon Moon
   --
   -- INTERFACE: void Start(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Function that's called when the script is first executed - first menu group
   ---------------------------------------------------------------------------------------------------------------------*/
    void Start()
    {
        MainMenu(); 
    }


    /*---------------------------------------------------------------------------------------------------------------------
   -- FUNCTION: SetMovementOption
   --
   -- DATE: March 10, 2016
   --
   -- REVISIONS: 
   --
   -- DESIGNER: Eunwon Moon
   --
   -- PROGRAMMER: Eunwon Moon
   --
   -- INTERFACE: void SetMovementOption(int i)
   --               i: movement type checkbox index 
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Function that's change movement system either relative or absolute
   ---------------------------------------------------------------------------------------------------------------------*/
    public void SetMovementOption(int i)
    {
        var  player = GameManager.instance.player;
        if (i == 1)
            player.GetComponent<Movement>().setAbs();
        else
            player.GetComponent<Movement>().setRel();
        movementType = i;
    }

    /*---------------------------------------------------------------------------------------------------------------------
   -- FUNCTION: turnoffMenu()
   --
   -- DATE: March 10, 2016
   --
   -- REVISIONS: 
   --
   -- DESIGNER: Eunwon Moon
   --
   -- PROGRAMMER: Eunwon Moon
   --
   -- INTERFACE: void Start(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Function that's called when close the paused menu.
   ---------------------------------------------------------------------------------------------------------------------*/
    public void turnoffMenu()
    {
        MainMenuGrp.SetActive(false);
        SettingMenuGrp.SetActive(false);
    }
    /*---------------------------------------------------------------------------------------------------------------------
   -- FUNCTION: MainMenu
   --
   -- DATE: March 10, 2016
   --
   -- REVISIONS: 
   --
   -- DESIGNER: Eunwon Moon
   --
   -- PROGRAMMER: Eunwon Moon
   --
   -- INTERFACE: void MainMenu(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Function that's called to display main menu group.
   ---------------------------------------------------------------------------------------------------------------------*/
    public void MainMenu()
    {
        MainMenuGrp.SetActive(true);
        SettingMenuGrp.SetActive(false);
    }
    /*---------------------------------------------------------------------------------------------------------------------
   -- FUNCTION: SettingOptionMenu
   --
   -- DATE: March 10, 2016
   --
   -- REVISIONS: 
   --
   -- DESIGNER: Eunwon Moon
   --
   -- PROGRAMMER: Eunwon Moon
   --
   -- INTERFACE: void SettingOptionMenu(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Function that's called when the user select setting in the main menu.
   -- possible to change movement system and background music volume
   ---------------------------------------------------------------------------------------------------------------------*/
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
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: quitGame
    --
    -- DATE: March 10, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Eunwon Moon
    --
    -- PROGRAMMER: Eunwon Moon
    --
    -- INTERFACE: void quitGame(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when the user select quit in the main menu.
    --  close the game application
    ---------------------------------------------------------------------------------------------------------------------*/
    void quitGame()
    {
        Application.Quit();
    }

}
