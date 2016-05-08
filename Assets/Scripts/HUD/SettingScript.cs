using UnityEngine;
using System.Collections;


/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    SettingScript.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      void Update()
--      void SettingClose()
--
--
--  DATE:           March 13 , 2016
--
--  REVISIONS:      
--
--  DESIGNERS:      Carson, Eunwon Moon
--
--  PROGRAMMER:     Carson, Eunwon Moon
--
--  NOTES:
--  This class is to connect paused menu and game when user press 'ESC'
---------------------------------------------------------------------------------------*/

public class SettingScript : MonoBehaviour {

    public GameObject settings;
    /*---------------------------------------------------------------------------------------------------------------------
   -- FUNCTION: Start
   --
   -- DATE: March 13, 2016
   --
   -- REVISIONS: 
   --
   -- DESIGNER: Carson, Eunwon Moon
   --
   -- PROGRAMMER: Carson, Eunwon Moon
   --
   -- INTERFACE: void Start(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Function that's called when the script is first executed 
   --   make invisible paused menu.
   ---------------------------------------------------------------------------------------------------------------------*/
    void Start()
    {
        settings.SetActive(false);
    }
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Update
    --
    -- DATE: March 13, 2016
    --
    -- REVISIONS: March 17, 2016
    --
    -- DESIGNER: Carson, Eunwon Moon
    --
    -- PROGRAMMER: Carson, Eunwon Moon
    --
    -- INTERFACE: void Update(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called every frame.
    --      Detect if the user press 'ESC' or npt and display paused menu.
    ---------------------------------------------------------------------------------------------------------------------*/
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

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: setttingClose
    --
    -- DATE: March 13, 2016
    --
    -- REVISIONS:
    --
    -- DESIGNER: Carson, Eunwon Moon
    --
    -- PROGRAMMER: Carson, Eunwon Moon
    --
    -- INTERFACE: void setttingClose(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called just before return to game
    -- invisible paused menu and turn on the mouse input in the game.
    ---------------------------------------------------------------------------------------------------------------------*/
    public void setttingClose()
    {
        settings.SetActive(false);
        GameData.MouseBlocked = false;
    }
}
