using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

    public enum MenuStates
    {
        Main, Settings, Connect
    };
    private GameObject currentState;

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject connectMenu;

    void Awake()
    {
        // set Main menu by default
        currentState = mainMenu;
        switchMenu(MenuStates.Main);
    }

    //---- main menu buttons ----
    public void connect()
    {
        Debug.Log("connect button pressed");
        switchMenu(MenuStates.Connect);
    }

    public void host()
    {
        Debug.Log("host button pressed");
    }

    public void settings()
    {
        Debug.Log("settings button pressed");
        switchMenu(MenuStates.Settings);
    }

    public void exit()
    {
        Debug.Log("exit button pressed");
    }

    //---- settings menu buttons ----
    public void video()
    {
        Debug.Log("video button pressed");
    }

    public void sound()
    {
        Debug.Log("sound button pressed");
    }

    public void controls()
    {
        Debug.Log("controls button pressed");
    }

    public void back()
    {
        Debug.Log("back button pressed");
        switchMenu(MenuStates.Main);
    }

    //---- connection menu ----
    public void joinLobby()
    {
        string name = getInputText("PlayerName");
        string ip = getInputText("ServerIP");

        if (!(name.Length == 0))
        {
            Debug.Log("Player name: " + name);
        }
        if (!(ip.Length == 0))
        {
            Debug.Log("Server IP: " + ip);
        }
        if (!(name.Length == 0) && !(ip.Length == 0))
        {
            Debug.Log("Go To Lobby");
            //Application.LoadLevel("TestScene");
        }
    }

    //---- helper functions ----

    private string getInputText(string input)
    {
        return GameObject.Find(input).GetComponent<InputField>().text;
    }

    private void switchMenu(MenuStates menu)
    {
        GameObject newState;

        switch (menu)
        {
            case MenuStates.Main:
                newState = mainMenu;
                break;
            case MenuStates.Settings:
                newState = settingsMenu;
                break;
            case MenuStates.Connect:
                newState = connectMenu;
                break;
            default:
                newState = mainMenu;
                break;
        }

        currentState.SetActive(false);
        currentState = newState;
        currentState.SetActive(true);
    }
}
