using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    ButtonSound.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      void StartSoundPlay()
--      void backSoundPlay()
--      void readySoundPlay()
--      void clickSoundPlay()
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

public class ButtonSound : MonoBehaviour {


    public AudioClip startSound;
    public AudioClip backSound;
    public AudioClip readySound;
    public AudioClip clickSound;

    private Button button { get { return GetComponent<Button>(); } }
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

  /*---------------------------------------------------------------------------------------------------------------------
  -- FUNCTION: Start
  --
  -- DATE: March 13, 2016
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
  -- Function that's called when the script is first executed 
  --   add each audio clip sound.
  ---------------------------------------------------------------------------------------------------------------------*/
    void Start () {
        
	    gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;

        //add sound file on each audio clip
        backSound = (AudioClip)Resources.Load("Music/UI/ui_back");
        startSound = (AudioClip)Resources.Load("Music/UI/ui_start");
        readySound = (AudioClip)Resources.Load("Music/UI/ui_ready");
        clickSound = (AudioClip)Resources.Load("Music/UI/ui_click");

    }

  /*---------------------------------------------------------------------------------------------------------------------
  -- FUNCTION: StartSoundPlay
  --
  -- DATE: March 13, 2016
  --
  -- REVISIONS: 
  --
  -- DESIGNER: Eunwon Moon
  --
  -- PROGRAMMER: Eunwon Moon
  --
  -- INTERFACE: void starSoundPlay(void)
  --
  -- RETURNS: void
  --
  -- NOTES:
  -- Function that's called  to play start button sound using audio source component
  ---------------------------------------------------------------------------------------------------------------------*/
    public void startSoundPlay()
    {
        source.clip = startSound;
        source.PlayOneShot(startSound);
    }
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: backSoundPlay
    --
    -- DATE: March 13, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Eunwon Moon
    --
    -- PROGRAMMER: Eunwon Moon
    --
    -- INTERFACE: void backSoundPlay(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called  to play back button sound using audio source component
    ---------------------------------------------------------------------------------------------------------------------*/
    public void backSoundPlay()
    {
        source.clip = backSound;
        source.PlayOneShot(backSound);
    }
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ReadySoundPlay
    --
    -- DATE: March 13, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Eunwon Moon
    --
    -- PROGRAMMER: Eunwon Moon
    --
    -- INTERFACE: void readySoundPlay(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called  to play ready button sound using audio source component
    ---------------------------------------------------------------------------------------------------------------------*/
    public void readySoundPlay()
    {
        source.clip = readySound;
        source.PlayOneShot(readySound);
    }
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: clickSoundPlay
    --
    -- DATE: March 13, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Eunwon Moon
    --
    -- PROGRAMMER: Eunwon Moon
    --
    -- INTERFACE: void clickSoundPlay(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called  to play button click sound using audio source component
    ---------------------------------------------------------------------------------------------------------------------*/

    public void clickSoundPlay()
    {
        source.clip = clickSound;
        source.PlayOneShot(clickSound);
    }
}
