using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
        // MainMenuGrp = GameObject.Find("Canvas").g;
        //  SettingMenuGrp = GameObject.Find("SettingMenuGrp");
       // volumeSlide = GameObject.Find("SettingMenuGrp").GetComponent<Slider>();
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
        
       volumeSlide.value = GameObject.Find("GameManager").GetComponent<AudioSource>().volume;
        
    }

    public void SetMovement(int i)
    {
        movementType = i;

    }



}
