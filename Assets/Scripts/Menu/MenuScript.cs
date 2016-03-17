using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using SimpleJSON;

public class MenuScript : MonoBehaviour {
    public enum MenuStates
    {
        Previous, Settings, Connect, Lobby
    };

    // list to keep track of which menu to go back to
    private List<GameObject> _menu_order = new List<GameObject>();
    private Sprite _default_avatar;
    private Sprite _gunner_avatar;
    private Sprite _ninja_avatar;
    private Sprite _mage_avatar;

    public Canvas menu_canvas;

    public GameObject main_menu;
    public GameObject settings_menu;
    public GameObject connect_menu;
    public GameObject lobby_menu;
	public Toggle	  ready_toggle;
    public GameObject ready_count;

    public GameObject team1_list;
    public GameObject team2_list;
    public GameObject soldier_panel;
    public GameObject mage_panel;
    public GameObject ninja_panel;
    public GameObject aman_panel;
    public GameObject team_select_panel;
    public GameObject class_select_panel;

    //----OnGUI test strings-------

    private List<Transform> _team1_slots;
    private List<Transform> _team2_slots;

    private string SendingPacket;

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
    }
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

    void OnGUI()
    {
        GUI.Label(new Rect(10, 20, Screen.width, Screen.height), "Reading()");
        GUI.Label(new Rect(10, 40, Screen.width, Screen.height), LobbyNetwork.RecievedData);
    }

    // === connection menu ===
    public void JoinLobby()
    {			
        string name = _GetInputText("NameInput");
        string ip = _GetInputText("IPInput");
		GameData.IP = ip;
        // TODO: validate player name and IP addr
        if (!(name.Length == 0) && !(ip.Length == 0))
        {
            GameData.MyPlayer.Username = name;
            Debug.Log("[Debug] IP: " + ip + "User Name:" + name);
            if (!LobbyNetwork.Connect(ip))
            {
                return;
            }
            StartCoroutine(DoSend());
            LobbyNetwork.SendLobbyData(NetworkCode.PlayerJoinedLobby);
            _SwitchMenu(MenuStates.Lobby);
        }
	}

	IEnumerator DoSend()
	{
		yield return new WaitForSeconds(1);
	}
    // === game lobby menu ===

    public void UpdateLobbyList()
    {
        int t1_idx = 0;
        int t2_idx = 0;
        int ready = 0;

        foreach (Transform slot in team1_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (Transform slot in team2_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (PlayerData p in GameData.LobbyData.Values)
        {
			Debug.Log ("PlayerUsername: " + p.Username);

            if (p.Ready)
            {
                ready++;
            }
            AddToLobby(p.Username, p.TeamID, p.ClassType, (p.TeamID == 1 ? t1_idx++ : t2_idx++));
        }
        ready_count.transform.GetComponent<Text>().text = ready.ToString() + "/" + GameData.LobbyData.Count;
    }

    private void AddToLobby(String name, int team, ClassType class_type, int index)
    { 
        List<Transform> team_to_set = (team == 1 ? _team1_slots : _team2_slots);
        name = name.ToUpper();
        Sprite avatar = _default_avatar;
        
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
            team_to_set[index].gameObject.SetActive(true);
        }
    }
    int t1id = 0;
    int t2id = 0;
    int idx = 0;
    public void TestAddToLobby(int team)
    {
        if (idx <= 23)
        {
            PlayerData p = new PlayerData();
            p.PlayerID = (team == 1 ? t1id++ : t2id++);
            p.Username = (team == 1 ? t1id.ToString() : t2id.ToString());
            p.TeamID = team;
            p.ClassType = ClassType.Gunner;
            p.Ready = false;

            GameData.LobbyData.Add(idx++, p);
        }
    }

    public void ChooseTeam()
    {
        team_select_panel.SetActive(true);
    }
    public void SelectTeam(int team)
    {
        team_select_panel.SetActive(false); // should do this after networking call
		GameData.MyPlayer.TeamID = team;
        LobbyNetwork.SendLobbyData(NetworkCode.TeamChangeRequest);

    }
	public void SelectClass(int value)
	{
		// TODO: associate class value with player here
		class_select_panel.SetActive(false);

        switch (value)
        {
            case 0:
                soldier_panel.SetActive(true);
                mage_panel.SetActive(false);
                ninja_panel.SetActive(false);
                aman_panel.SetActive(false);
                break;
            case 1:
                soldier_panel.SetActive(false);
                mage_panel.SetActive(true);
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
                mage_panel.SetActive(false);
                ninja_panel.SetActive(false);
                aman_panel.SetActive(true);
                break;
        }
		GameData.MyPlayer.ClassType = (ClassType)value;
		LobbyNetwork.SendLobbyData(NetworkCode.ClassChangeRequest);
	}

	public void StartGame()
	{
		// Don't need this atm
		// Application.LoadLevel("HUDScene");
		
		LobbyNetwork.SendLobbyData(NetworkCode.GameStart);
		
		team_select_panel.SetActive(false);
	}
	
	public void SetReady()
	{
		GameData.MyPlayer.Ready = (ready_toggle.isOn ? true : false);
		LobbyNetwork.SendLobbyData(NetworkCode.ReadyRequest);
	}


    public void ChooseClass()
    {
        class_select_panel.SetActive(true);
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
        _SwitchMenu(MenuStates.Previous);
    }

	public void LeaveLobby()
	{
		LobbyNetwork.SendLobbyData(NetworkCode.PlayerLeftLobby);
		if(LobbyNetwork.connected)
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
}
