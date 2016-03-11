using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using SimpleJSON;

public enum NetworkCode
{
	TeamChangeRequest 	= 1,
	ClassChangeRequest 	= 2,
	ReadyRequest 		= 3,
	PlayerJoinedLobby 	= 4,
	GameStart 			= 5,
	UpdatePlayerList	= 6,
	PlayerLeftLobby		= 7
}
public class NetworkKeyString
{
	public static string PlayerID 	= "PlayerID";
	public static string TeamID		= "TeamID";
	public static string ClassID	= "ClassID";
	public static string Ready		= "Ready";
	public static string StartGame 	= "StartGame";
	public static string UserName   = "UserName";
}
public class MenuScript : MonoBehaviour {
    public enum MenuStates
    {
        Previous, Settings, Connect, Lobby
    };

    // list to keep track of the player text items in the lobby
    //private List<GameObject> _lobby_list = new List<GameObject>();
    //private int _y_offset = 0;

    // list to keep track of which menu to go back to
    private List<GameObject> _menu_order = new List<GameObject>();
    private string _player_name;

    public Canvas menu_canvas;

    public GameObject main_menu;
    public GameObject settings_menu;
    public GameObject connect_menu;
    public GameObject lobby_menu;
	public Toggle	  ready_toogle;

    public GameObject team1_list;
    public GameObject team2_list;
    public GameObject soldier_panel;
    public GameObject mage_panel;
    public GameObject ninja_panel;
    public GameObject team_select_panel;
    public GameObject class_select_panel;

    //----OnGUI test strings-------
    private string RecievedData = "Waiting for Inputs...";
    private string FormatedData = "Foramted...";

    private List<Transform> _team1_slots;
    private List<Transform> _team2_slots;

    private string SendingPacket;
    private int PlayerID = -1;
    private bool connected = false;

    void Awake()
    {
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
        if (connected)
        {
            string tmp = Marshal.PtrToStringAnsi(NetworkingManager.TCP_GetData());
            if (!String.Equals(tmp, "[]"))
            {
                // NOTE:: This indicates the player that sent the packet, not the current player ID
                int PlayerPacketID;
				Debug.Log("[Debug]: " + tmp);
                RecievedData = tmp;
                var packet = (JSON.Parse(RecievedData).AsArray)[0];
                PlayerPacketID = packet[NetworkKeyString.PlayerID].AsInt;
                switch ((NetworkCode)packet["ID"].AsInt)
                {
                    case NetworkCode.TeamChangeRequest:
                    {
                        Debug.Log("[Debug]: Team chagne request");
                        Debug.Log("[Debug]: Content--" + tmp);
                        int TeamID = packet[NetworkKeyString.TeamID].AsInt;
                        GameData.LobbyData[PlayerPacketID].TeamID = TeamID;

                        FormatedData = "Recieved from player ID: " + PlayerPacketID.ToString() + ", for team request on team: " + TeamID.ToString();
						PrintData();
				
						UpdateLobbyList();
				
                        break;
                    }
                    case NetworkCode.ClassChangeRequest:
                    {
                        Debug.Log("[Debug]: Class change request");
                        Debug.Log("[Debug]: Content--" + tmp);
                        int ClassID = packet[NetworkKeyString.ClassID].AsInt;
                        GameData.LobbyData[PlayerPacketID].ClassType = (ClassType)ClassID;

                        FormatedData = "Recieved from player ID: " + PlayerPacketID.ToString() + ", for class request on class: " + ClassID.ToString();
						PrintData();
				
						UpdateLobbyList();
                        break;
                    }
                    case NetworkCode.ReadyRequest:
                    {
                        Debug.Log("[Debug]: Ready request");
                        Debug.Log("[Debug]: Content--" + tmp);
                        bool IsReady = packet[NetworkKeyString.Ready].AsBool;
                        GameData.LobbyData[PlayerPacketID].Ready = IsReady;

                        FormatedData = "Recieved from player ID: " + PlayerPacketID.ToString() + ", which is " + (IsReady ? "Ready" : "Not Ready");
				
						PrintData();
						UpdateLobbyList();
                        break;
                    }
                    case NetworkCode.PlayerJoinedLobby:
                    {
                        Debug.Log("[Debug]: Player has joined lobby");
                        Debug.Log("[Debug]: Content--" + tmp);

                        PlayerData tmpPlayer = new PlayerData();
                        tmpPlayer.PlayerID = PlayerPacketID;
                        tmpPlayer.Username = packet[NetworkKeyString.UserName];
                        if (PlayerID == -1)
                        {
                            PlayerID = PlayerPacketID;
                            GameData.MyPlayerID = PlayerPacketID;
                            Debug.Log("[UltraDebug] Updated Player ID: " + PlayerID);

                        }
                        else
                            GameData.LobbyData.Add(PlayerPacketID, tmpPlayer);
						PrintData();
				
						UpdateLobbyList();
                        break;
                    }
                    case NetworkCode.UpdatePlayerList:
                    {
                        Debug.Log("[Debug]: Got update table message");
                        Debug.Log("[Debug]: Content--" + tmp);

                        // fills in existing player data
                        foreach (JSONNode playerData in packet["LobbyData"].AsArray)
                        {
							int id = playerData[NetworkKeyString.PlayerID].AsInt;
							Debug.Log("[Debug]: IN UPDATEPLAYERLIST");
							PlayerData tempPlayer 	= new PlayerData();
							tempPlayer.PlayerID  	= id;
							tempPlayer.ClassType 	= (ClassType)playerData[NetworkKeyString.ClassID].AsInt;
							tempPlayer.TeamID 		= playerData[NetworkKeyString.TeamID].AsInt;
							tempPlayer.Ready 		= playerData[NetworkKeyString.Ready].AsBool;
							tempPlayer.Username		= playerData[NetworkKeyString.UserName];
                            GameData.LobbyData.Add(id, tempPlayer);

                            Debug.Log("[Debug]: Player ID: " + GameData.LobbyData[id].PlayerID.ToString() + "ClassID: " +
                                      GameData.LobbyData[id].ClassType.ToString() + "TeamID: " + GameData.LobbyData[id].TeamID.ToString() + "Username: " + GameData.LobbyData[id].Username);
                        }
                        FormatedData = "UpdatePlayerList";
						UpdateLobbyList();	
                        break;
                    }
					case NetworkCode.PlayerLeftLobby:
					{
						GameData.LobbyData.Remove(PlayerPacketID);
						UpdateLobbyList();
						break;
					}
                    case NetworkCode.GameStart:
                    {
						FormatedData = "Player: " + PlayerPacketID + " has started the game!";
						// start game stuff here
                        break;
                    }
                }

            }
        }
    }

	void PrintData()
	{
		Debug.Log("[Debug]: IN PrintData()");
		foreach(PlayerData p in GameData.LobbyData.Values)
		{
			Debug.Log("[Debug]: Player ID: " + p.PlayerID.ToString() + "ClassID: " +
			          p.ClassType.ToString() + "TeamID: " + p.TeamID.ToString() + "Username: " + p.Username);
		}
	}

    void OnGUI()
    {
        GUI.Label(new Rect(10, 20, Screen.width, Screen.height), "Reading()");
        GUI.Label(new Rect(10, 40, Screen.width, Screen.height), RecievedData);
        GUI.Label(new Rect(10, 60, Screen.width, Screen.height), FormatedData);
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
			Debug.Log("[Debug] IP: " + ip + "User Name:" + name);
            // Connect to the server
            if (NetworkingManager.TCP_ConnectToServer(ip, 7000) < 0)
            {
                RecievedData = "Cant connect to server";
                return;	
            }
            // Thread creation
            if (NetworkingManager.TCP_StartReadThread() < 0)
            {
                RecievedData = "Error creating read thread";
                return;
            }

            RecievedData = "Connected";
            connected = true;

            List<Pair<string, string>> packetData = new List<Pair<string, string>>();
            packetData.Add(new Pair<string, string>(NetworkKeyString.UserName, "\"" + _player_name + "\""));
            SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.PlayerJoinedLobby, packetData, Protocol.NA);
            // Send dummy packet
            if (NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
            {
                // handle error here 
                Debug.Log("SelectTeam(): Packet sending failed\n");
            }
        } else
            RecievedData = "IP length is zero";
		_SwitchMenu(MenuStates.Lobby);
		
		
	}
	
    // === game lobby menu ===

    public void UpdateLobbyList()
    {
        // scan lobby list
        // add each player to respective team list
        // on list change, remove and re add players from lists

        foreach (Transform slot in team1_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (Transform slot in team2_list.transform)
        {
            slot.gameObject.SetActive(false);
        }

        int t1_idx = 0;
        int t2_idx = 0;
        Debug.Log("lobby size = " + GameData.LobbyData.Count);
        foreach (PlayerData p in GameData.LobbyData.Values)
        {
			Debug.Log ("PlayerUsername: " + p.Username);
            AddToLobby(p.Username, p.TeamID, p.ClassType, (p.TeamID == 1 ? t1_idx++ : t2_idx++));
        }
    }

    private void AddToLobby(String name, int team, ClassType class_type, int index)
    { 
        List<Transform> team_to_set = (team == 1 ? _team1_slots : _team2_slots);
        name = name.ToUpper();

        team_to_set[index].transform.Find("Name").transform.GetComponent<Text>().text = name;
        
        //team_to_set[index].GetComponent<Image>().sprite = "path/to/class/avatar";    // TODO: 

        team_to_set[0].gameObject.SetActive(true);
    }

    private void RemoveFromLobby(int team)
    {
        GameObject team_to_set = (team == 1 ? team1_list : team2_list);
    }

    public void ChooseTeam()
    {
        team_select_panel.SetActive(true);
    }
    public void SelectTeam(int team)
    {
        team_select_panel.SetActive(false); // should do this after networking call

		// Construct json packet 
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.TeamID, team.ToString()));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.TeamChangeRequest, packetData, Protocol.NA);
		// Send dummy packet
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
		{
			// handle error here 
			Debug.Log("SelectTeam(): Packet sending failed\n");
		}

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
                break;
            case 1:
                soldier_panel.SetActive(false);
                mage_panel.SetActive(true);
                ninja_panel.SetActive(false);
                break;
            case 2:
                soldier_panel.SetActive(false);
                mage_panel.SetActive(false);
                ninja_panel.SetActive(true);
                break;
        }
		
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.ClassID, value.ToString()));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.ClassChangeRequest, packetData, Protocol.NA);
		
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
		{
			// handle error here
			Debug.Log("SelectClass(): Pakcet sending failed");
		}
	}

	public void StartGame()
	{
		// Don't need this atm
		// Application.LoadLevel("HUDScene");
		
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.GameStart, packetData, Protocol.NA);
		// Send dummy packet
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
		{
			// handle error here 
			Debug.Log("SelectTeam(): Packet sending failed\n");
		}
		
		team_select_panel.SetActive(false);
	}
	
	public void SetReady()
	{
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.Ready, ready_toogle.isOn ? "1" : "0"));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.ReadyRequest, packetData, Protocol.NA);

		// Send dummy packet
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
		{
			// handle error here
			Debug.Log("SetReady(): Packet sending failed");
		}
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
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.PlayerLeftLobby, packetData, Protocol.NA);
		// Send dummy packet
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
			Debug.Log("[Debug] LeaveLobby(): Packet sending failed");

		if(connected)
		{
			connected = false;
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
