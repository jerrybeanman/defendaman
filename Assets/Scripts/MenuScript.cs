using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class MenuScript : MonoBehaviour {	
    public enum MenuStates
    {
        Previous, Settings, Connect, Lobby
    };

    // list to keep track of the player text items in the lobby
    private List<GameObject> _lobby_list = new List<GameObject>();
    private int _y_offset = 0;

    // list to keep track of which menu to go back to
    private List<GameObject> _menu_order = new List<GameObject>();
    private string _player_name;

    public Canvas menu_canvas;

    public GameObject main_menu;
    public GameObject settings_menu;
    public GameObject connect_menu;
    public GameObject lobby_menu;

    public GameObject team_select_panel;
    public GameObject class_select_panel;

	private string RecievedData = "Waiting for Inputs...";
    void Awake()
    {
        menu_canvas = menu_canvas.GetComponent<Canvas>();

        // set Main menu by default and disable other states & panels
        settings_menu.SetActive(false);
        connect_menu.SetActive(false);
        lobby_menu.SetActive(false);
        main_menu.SetActive(true);
        _menu_order.Add(main_menu);

        team_select_panel.SetActive(false);
        class_select_panel.SetActive(false);
    }
	bool connected = false;
	void Update()
	{
		string tmp = Marshal.PtrToStringAnsi(NetworkingManager.TCP_GetData());
		if(!String.Equals(tmp,"[]"))
		{
			RecievedData = "equals?";
			RecievedData = tmp;
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 20, Screen.width, Screen.height), "Reading()");
		GUI.Label(new Rect(10, 40, Screen.width, Screen.height), RecievedData);
	}

    // === connection menu ===
    public void JoinLobby()
    {
        string name = _GetInputText("NameInput");
        string ip = _GetInputText("IPInput");

        // TODO: validate player name and IP addr
        if (!(name.Length == 0) && !(ip.Length == 0))
        {
            _player_name = name;
            _SwitchMenu(MenuStates.Lobby);

            // Connect to the server
			if(NetworkingManager.TCP_ConnectToServer(ip, 7000) < 0)
			{
				// Error check here
				Debug.Log("Cant connect to server\n");
			}

			// Thread creation
			if(NetworkingManager.TCP_StartReadThread() < 0)
			{
				// Error check here
				Debug.Log("Error creating read thread\n");
			}
        }
    }

    // === game lobby menu ===
    public void LoadLevel()
    {
        Application.LoadLevel("HUDScene");
    }

    public void ChooseTeam()
    {
        team_select_panel.SetActive(true);
        _RemovePlayerFromLobbyList(0);
    }
    public void SelectTeam(int team)
    {
        // add player text with appropriate colour to canvas

		// This call is causing an error
        //_AddPlayerToLobbyList(_player_name, team);

		// Send dummy packet
		if(NetworkingManager.TCP_Send(_player_name + " selected " + team + "!", 30) < 0)
		{
			// handle error here 
			Debug.Log("SelectTeam(): Packet sending failed\n");
		}

        team_select_panel.SetActive(false);
    }

    public void ChooseClass()
    {
        class_select_panel.SetActive(true);
    }

    public void SelectClass(int value)
    {
        // TODO: associate class value with player here
        class_select_panel.SetActive(false);

		// Send dummy packet
		if(NetworkingManager.TCP_Send(_player_name + "selected class " + value + "!", 20) < 0)
		{
			// handle error here
			Debug.Log("SelectClass(): Pakcet sending failed");
		}
    }

    private void _AddPlayerToLobbyList(string player_name, int team)
    {
        _lobby_list.Add(_AddTextToCanvas(menu_canvas.transform, 290, (120 - (25 * _y_offset++)), 100, 35, player_name, 20, team));
    }

    // need to remove player names if switch teams and then redraw text
    // adjust y_offset value when removing player
    // need some kind of player_id to do this in case of identical name
    private void _RemovePlayerFromLobbyList(int player_id)
    {
        // right now just get rid of last one...
        Destroy(_lobby_list[_lobby_list.Count - 1]);
        _lobby_list.RemoveAt(_lobby_list.Count - 1);
        _y_offset--; // not necessarily when dealing with other connections...
    }

    // === private helper functions ===

    // adds a text object to canvas
    private GameObject _AddTextToCanvas(Transform parent, float x, float y, float w, float h, string message, int font_size, int color)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent);

        textObject.layer = 5; // UI layer

        RectTransform trans = textObject.AddComponent<RectTransform>();
        trans.sizeDelta.Set(w, h);
        trans.anchoredPosition3D = new Vector3(0, 0, 0);
        trans.anchoredPosition = new Vector2(x, y);
        trans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        trans.localPosition.Set(0, 0, 0);

        CanvasRenderer renderer = textObject.AddComponent<CanvasRenderer>();

        Text text = textObject.AddComponent<Text>();
        text.supportRichText = true;
        text.text = message.ToUpper();
        text.fontSize = font_size;
        text.font = Resources.Load("Fonts/Neutra/Neutra2Text-Book", typeof(Font)) as Font;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.color = ((color == 1) ? Color.red : Color.blue);

        return textObject;
    }

    private string _GetInputText(string input)
    {
        return GameObject.Find(input).GetComponent<InputField>().text;
    }

    private void _SwitchMenu(MenuStates menu)
    {
        GameObject new_menu;
        bool remove = false;

        switch (menu)
        {
            case MenuStates.Previous:
                new_menu = null; // to make compiler happy
                remove = true;
                break;
            case MenuStates.Settings:
                new_menu = settings_menu;
                break;
            case MenuStates.Connect:
                new_menu = connect_menu;
                break;
            case MenuStates.Lobby:
                new_menu = lobby_menu;
                team_select_panel.SetActive(true);
                break;
            default:
                new_menu = main_menu;
                break;
        }

        _menu_order[_menu_order.Count - 1].SetActive(false);
        if (remove)
        {
            _menu_order.RemoveAt(_menu_order.Count - 1);
        } else
        {
            _menu_order.Add(new_menu);
        }

        _menu_order[_menu_order.Count - 1].SetActive(true);
    }

    // === button functions ===

    // --- main menu buttons ---
    public void Connect()
    {
        _SwitchMenu(MenuStates.Connect);
    }

    public void Settings()
    {
        _SwitchMenu(MenuStates.Settings);
    }

    public void Exit()
    {
        Application.Quit();
    }

    // --- settings menu buttons ---
    public void Controls()
    {
        Debug.Log("controls button pressed");
    }

    public void Back()
    {
		//disconnect from server

        foreach (GameObject go in _lobby_list)
        {
            Destroy(go);
            _y_offset--;
        }
        _SwitchMenu(MenuStates.Previous);
    }

}
