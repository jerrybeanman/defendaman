using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using SimpleJSON;

/*---------------------------------------------------------------------------------------------------------------------
-- SOURCE FILE: MenuScript.cs
--
-- FUNCTIONS:
--      void Awake()
--      void Update()
--      IEnumerator start_level()
--      IEnumerator DoSend()
--      private void UpdateLobbyList()
--      private void AddToLobby(String name, int team, ClassType class_type, int index, bool ready_status)
--      public void SetReady()
--      public void SetAman()
--      public void ChooseTeam()
--      public void ChooseMap()
--      public void ChooseClass()
--      public void SelectTeam(int team)
--      public void SelectMap(int theme)
--      public void SelectClass(int value)
--      public void StartGame()
--      public void JoinLobby()
--      public void LeaveLobby()
--      public void Connect()
--      public void Settings()
--      public void Controls()
--      public void Exit()
--      public void Back()
--      private void _SwitchMenu(MenuStates menu)
--      private string _GetInputText(string input)
--
-- DATE: April 04, 2016
--
-- DESIGNER: Spenser Lee
--
-- PROGRAMMER: Spenser Lee, Jerry Jia
--
-- NOTES:
-- This file contains the logic behind the main menu and lobby for the game. 
---------------------------------------------------------------------------------------------------------------------*/
public class MenuScript : MonoBehaviour {

    // possible menu states
    public enum MenuStates
    {
        Previous, Settings, Connect, Lobby
    };

    public Canvas menu_canvas;

    public GameObject main_menu;
    public GameObject settings_menu;
    public GameObject connect_menu;
    public GameObject lobby_menu;
	public Toggle	  ready_toggle;
    public Toggle     aman_toggle;
    public Button     change_team;
    public GameObject ready_count;
    public GameObject host_ip;
    public GameObject map_theme;

    public GameObject team1_list;
    public GameObject team2_list;
    public GameObject soldier_panel;
    public GameObject mage_panel;
    public GameObject ninja_panel;
    public GameObject aman_panel;
    public GameObject team_select_panel;
    public GameObject class_select_panel;
    public GameObject map_select_panel;

    // list to keep track of which menu to go back to
    private List<GameObject> _menu_order = new List<GameObject>();

    private string _host_ip;
    private bool _team_set = false;
    private bool _class_set = false;

    private Sprite _default_avatar;
    private Sprite _gunner_avatar;
    private Sprite _ninja_avatar;
    private Sprite _mage_avatar;

    //----OnGUI test strings-------
    private List<Transform> _team1_slots;
    private List<Transform> _team2_slots;

    private string SendingPacket;

	/*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Awake
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee & Jerry Jia
    --
    -- PROGRAMMER: Spenser Lee & Jerry Jia
    --
    -- INTERFACE: void Awake()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Initialization function. Loads necessary sprites and sets default values upon the Menu Scene loading.
    -----------------------------------------------------------------------------------------------------------------*/
	void Awake()
	{
		_default_avatar = Resources.Load("Sprites/UI/default_avatar", typeof(Sprite)) as Sprite;
		_gunner_avatar = Resources.Load("Sprites/UI/gun_avatar", typeof(Sprite)) as Sprite;
		_ninja_avatar = Resources.Load("Sprites/UI/ninja_avatar", typeof(Sprite)) as Sprite;
		_mage_avatar = Resources.Load("Sprites/UI/mage_avatar", typeof(Sprite)) as Sprite;
		
		
		menu_canvas = menu_canvas.GetComponent<Canvas>();
		
		// set Main menu by default and disable other states & panels
		settings_menu.SetActive(false);
		connect_menu.SetActive(false);
		lobby_menu.SetActive(false);
		main_menu.SetActive(true);
		_menu_order.Add(main_menu);
		
		_team1_slots = new List<Transform>();
		_team2_slots = new List<Transform>();
		
		// initialize all 12 slots per team and disable them by default
		foreach (Transform slot in team1_list.transform)
		{
			_team1_slots.Add(slot);
			slot.gameObject.SetActive(false);
		}
		foreach (Transform slot in team2_list.transform)
		{
			_team2_slots.Add(slot);
			slot.gameObject.SetActive(false);
		}
		
		team_select_panel.SetActive(false);
		class_select_panel.SetActive(false);
		map_select_panel.SetActive(false);
	}
	
	/*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Update
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void Update()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Called once per frame. Waits for new incoming network messages and will update the lobby list if changes are
    -- made. (e.g. new player, team changes etc.)
    -----------------------------------------------------------------------------------------------------------------*/
	void Update()
	{
		if (LobbyNetwork.connected)
		{
			string tmp = Marshal.PtrToStringAnsi(NetworkingManager.TCP_GetData());
			if (!String.Equals(tmp, "[]"))
			{
				LobbyNetwork.ParseLobbyData(tmp);
				UpdateLobbyList();
			}
		}
	}


    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: UpdateLobbyList
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void UpdateLobbyList()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function to update the team lobby information. Scans the current lobby player information to check for
    -- team, class, and ready status changes, then updates the UI accordingly.
    -----------------------------------------------------------------------------------------------------------------*/
    private void UpdateLobbyList()
    {
        int t1_idx = 0;
        int t2_idx = 0;
        int ready = 0;
		bool readyStatus;
        bool isAman;

        // start off by disabling all player slots in each team
        foreach (Transform slot in team1_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (Transform slot in team2_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        // NOTE: Unlock/Lock aman panel depending on status recieved over network
        // TODO: lock aman selection toggle if GameData.AllyKingID != GameData.MyPlayer.PlayerID 
        //		 unlock aman selection toggle if GamaData.AllyKingID == -1, which indicates aman hasnt been selected or has been "deselected" 
        //		 put aman "indicator" to the corresponding player on both teams

		if (GameData.AllyKingID != GameData.MyPlayer.PlayerID)
        {
            aman_toggle.interactable = false;
            change_team.interactable = true;
        }
		if (GameData.AllyKingID == -1)
		{
			aman_toggle.interactable = true;
		}
        if (GameData.AllyKingID == GameData.MyPlayer.PlayerID)
        {
            change_team.interactable = false;
        }

        // check the current lobby data values and update the player slots
        foreach (PlayerData p in GameData.LobbyData.Values)
        {
			readyStatus = false;
            isAman = false;
            if (p.Ready)
            {
                ready++;
                readyStatus = true;
            }

            if (p.PlayerID == GameData.AllyKingID || p.PlayerID == GameData.EnemyKingID)
            {
                isAman = true;
            }

            AddToLobby(p.Username, p.TeamID, p.ClassType, (p.TeamID == 1 ? t1_idx++ : t2_idx++), readyStatus, isAman);
        }
        ready_count.transform.GetComponent<Text>().text = ready.ToString() + "/" + GameData.LobbyData.Count;
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: AddToLobby
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void AddToLobby(String name, int team, ClassType class_type, int index, bool readyStatus)
    --                  name:           player name to be added
    --                  team:           which team to add the player to
    --                  class_type:     which class type the player is
    --                  index:          which player slot to add them to
    --                  ready_status:   whether or not the player is ready
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function to actually add a player to a player slot. 
    -----------------------------------------------------------------------------------------------------------------*/
    private void AddToLobby(String name, int team, ClassType class_type, int index, bool ready_status, bool is_aman)
    {
        List<Transform> team_to_set = (team == 1 ? _team1_slots : _team2_slots);
        name = name.ToUpper();
        // Sprite avatar = _default_avatar;
		Sprite avatar = _gunner_avatar;
        if (index <= 12)
        {
            team_to_set[index].transform.Find("Name").transform.GetComponent<Text>().text = name;

            switch (class_type)
            {
                case ClassType.Gunner: avatar = _gunner_avatar; break;
                case ClassType.Ninja: avatar = _ninja_avatar; break;
                case ClassType.Wizard: avatar = _mage_avatar; break;
            }

            team_to_set[index].transform.Find("Profile").transform.GetComponent<Image>().sprite = avatar;

            if (ready_status)
            {
                team_to_set[index].transform.Find("ReadyStatus").gameObject.SetActive(true);
            }
            else
            {
                team_to_set[index].transform.Find("ReadyStatus").gameObject.SetActive(false);
            }
            if (is_aman)
            {
                team_to_set[index].transform.Find("AmanIndicator").gameObject.SetActive(true);
            }
            else
            {
                team_to_set[index].transform.Find("AmanIndicator").gameObject.SetActive(false);
            }

            team_to_set[index].gameObject.SetActive(true);
        }
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: SetReady
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee & Jerry Jia
    --
    -- INTERFACE: void SetReady()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to change ready status in the lobby. Sends a netorking packet to update ready status.
    -----------------------------------------------------------------------------------------------------------------*/
    public void SetReady()
    {
        GameData.MyPlayer.Ready = (ready_toggle.isOn ? true : false);
        LobbyNetwork.SendLobbyData(NetworkCode.ReadyRequest);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: SetAman
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee & Jerry Jia
    --
    -- INTERFACE: void SetAman()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to inidicate whether or not the current player has aman status.
    -----------------------------------------------------------------------------------------------------------------*/
    public void SetAman()
    {

		// NOTE:: This can only be called when either the player has already selected to be the aman, which he can deselect it
		//		  or no one on the team has selected aman. Functionality handled over network
		GameData.AllyKingID = aman_toggle.isOn ? GameData.MyPlayer.PlayerID : -1;
		LobbyNetwork.SendLobbyData(NetworkCode.AmanSelection);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ChooseTeam
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void ChooseTeam()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Activates the team selection panel. 
    -----------------------------------------------------------------------------------------------------------------*/
    public void ChooseTeam()
    {
        team_select_panel.SetActive(true);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ChooseMap
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void ChooseMap()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to bring up the map theme select panel.
    -----------------------------------------------------------------------------------------------------------------*/
    public void ChooseMap()
    {
        map_select_panel.SetActive(true);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ChooseClass
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void ChooseClass()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to bring up the class select panel.
    -----------------------------------------------------------------------------------------------------------------*/
    public void ChooseClass()
    {
        class_select_panel.SetActive(true);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: SelectTeam
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void SelectTeam(int team)
    --              team:   the team number
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function for selecting a team. Sends a network packet indicating which team was selected. 
    -----------------------------------------------------------------------------------------------------------------*/
    public void SelectTeam(int team)
    {
        team_select_panel.SetActive(false);

        if (GameData.MyPlayer.TeamID != team && GameData.MyPlayer.TeamID > 0)
        {
            int temp = GameData.AllyKingID;
            GameData.AllyKingID = GameData.EnemyKingID;
            GameData.EnemyKingID = temp;
        }

        GameData.MyPlayer.TeamID = team;
        LobbyNetwork.SendLobbyData(NetworkCode.TeamChangeRequest);
        _team_set = true;

        if (_class_set == false)
        {
            class_select_panel.SetActive(true);
        }

    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: SelectMap
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void SelectMap(int theme)
    --              theme:  the theme value
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function for selecting a map theme.
    -----------------------------------------------------------------------------------------------------------------*/
    public void SelectMap(int theme)
    {
        map_select_panel.SetActive(false);
        GameData.CurrentTheme = (Themes)theme;
        if ((Themes)theme == Themes.Grass)
        {
            map_theme.transform.GetComponent<Text>().text = "MAP THEME: GRASSLAND";
        } 
		else
        {
            map_theme.transform.GetComponent<Text>().text = "MAP THEME: TRON";
        }
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: SelectClass
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void SelectClass(int value)
    --              value:   class value
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function for selecting a class. Enables/disables the necessary class information panels and sends
    -- a netork packet indicating which class was selected. 
    -----------------------------------------------------------------------------------------------------------------*/
    public void SelectClass(int value)
    {
        class_select_panel.SetActive(false);

        switch (value)
        {
            case 1:
                soldier_panel.SetActive(true);
                mage_panel.SetActive(false);
                ninja_panel.SetActive(false);
                aman_panel.SetActive(false);
                break;
            case 2:
                soldier_panel.SetActive(false);
                mage_panel.SetActive(false);
                ninja_panel.SetActive(true);
                aman_panel.SetActive(false);
                break;
            case 3:
                soldier_panel.SetActive(false);
                mage_panel.SetActive(true);
                ninja_panel.SetActive(false);
                aman_panel.SetActive(false);
                break;
            case 4:
                soldier_panel.SetActive(false);
                mage_panel.SetActive(false);
                ninja_panel.SetActive(false);
                aman_panel.SetActive(true);
                break;
        }
        GameData.MyPlayer.ClassType = (ClassType)value;
        LobbyNetwork.SendLobbyData(NetworkCode.ClassChangeRequest);

        _class_set = true;
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: StartGame
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void StartGame()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function for starting the game.
    -----------------------------------------------------------------------------------------------------------------*/
    public void StartGame()
    {
        if (GameData.AllyKingID != -1 && GameData.EnemyKingID != -1)
        {
            LobbyNetwork.SendLobbyData(NetworkCode.GameStart);
        }
        team_select_panel.SetActive(false);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: JoinLobby
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void JoinLobby()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function called when a player attemps to join the lobby. User inputs a name and IP address, then attempts to
    -- connect to the server.
    -----------------------------------------------------------------------------------------------------------------*/
    public void JoinLobby()
    {			
        string name = _GetInputText("NameInput");
        _host_ip = _GetInputText("IPInput");
		GameData.IP = _host_ip;
		if(Application.platform != RuntimePlatform.LinuxPlayer)
		{
            _SwitchMenu(MenuStates.Lobby);
			return;
		}
        
        if (!(name.Length == 0) && name.Length <= 12  && !(_host_ip.Length == 0))
        {
            GameData.MyPlayer.Username = name;
            if (!LobbyNetwork.Connect(_host_ip))
            {
                return;
            }
            LobbyNetwork.SendLobbyData(NetworkCode.PlayerJoinedLobby);
            _SwitchMenu(MenuStates.Lobby);
        }
	}

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: LeaveLobby
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee & Jerry Jia
    --
    -- PROGRAMMER: Spenser Lee & Jerry Jia
    --
    -- INTERFACE: void LeaveLobby()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to leave the lobby. Sends necessary netorking messages to disconnect from the server
    -- and clears the lobby menu UI as necessary.
    -----------------------------------------------------------------------------------------------------------------*/
    public void LeaveLobby()
    {
        GameData.LobbyData.Remove(GameData.MyPlayer.PlayerID);
        LobbyNetwork.SendLobbyData(NetworkCode.PlayerLeftLobby);
        if (LobbyNetwork.connected)
        {
            LobbyNetwork.connected = false;
            NetworkingManager.TCP_DisposeClient();

        }
        foreach (Transform slot in team1_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (Transform slot in team2_list.transform)
        {
            slot.gameObject.SetActive(false);
        }
        Back();
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Connect
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void Connect()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to switch to connect to lobby menu.
    -----------------------------------------------------------------------------------------------------------------*/
    public void Connect()
    {
        _SwitchMenu(MenuStates.Connect);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Settings
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void Settings()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to switch to settings menu.
    -----------------------------------------------------------------------------------------------------------------*/
    public void Settings()
    {
        _SwitchMenu(MenuStates.Settings);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Controls
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void Controls()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to switch to controls menu.
    -----------------------------------------------------------------------------------------------------------------*/
    public void Controls()
    {
        // TODO: control menu?
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Exit
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void Exit()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to switch to exit the game.
    -----------------------------------------------------------------------------------------------------------------*/
    public void Exit()
    {
        Application.Quit();
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Back
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void Back()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- On click function to go back to the previous menu.
    -----------------------------------------------------------------------------------------------------------------*/
    public void Back()
    {
        if (_menu_order[_menu_order.Count - 1] == lobby_menu)
        {
            _class_set = false;
            _team_set = false;
        }
        _SwitchMenu(MenuStates.Previous);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: _SwitchMenu
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: void _SwitchMenu(MenuStates menu)
    --                  menu:   the menu to switch to
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Helper function to switch to a menu. Keeps track of the previous menu to enable the Back() method.
    -----------------------------------------------------------------------------------------------------------------*/
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
                host_ip.transform.GetComponent<Text>().text = _host_ip;
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
        }
        else
        {
            _menu_order.Add(new_menu);
        }

        _menu_order[_menu_order.Count - 1].SetActive(true);
    }

    /*-----------------------------------------------------------------------------------------------------------------
    -- FUNCTION: _GetInputText
    --
    -- DATE: April 04, 2016
    --
    -- DESIGNER: Spenser Lee
    --
    -- PROGRAMMER: Spenser Lee
    --
    -- INTERFACE: string _GetInputText(string input)
    --              input:  the name of the input field
    --
    -- RETURNS: string - the input field text
    --
    -- NOTES:
    -- Helper function to get input field text.
    -----------------------------------------------------------------------------------------------------------------*/
    private string _GetInputText(string input)
	{
		return GameObject.Find(input).GetComponent<InputField>().text;
	}

	// testing...
	void OnGUI()
	{
		if (Application.platform == RuntimePlatform.LinuxPlayer)
		{
			//GUI.Label(new Rect(10, 20, Screen.width, Screen.height), "Reading()");
			//GUI.Label(new Rect(10, 40, Screen.width, Screen.height), LobbyNetwork.RecievedData);
		}
	}
}
