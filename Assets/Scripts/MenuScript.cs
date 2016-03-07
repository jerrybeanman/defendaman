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
	UpdatePlayerList	= 6
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

	//----OnGUI test strings-------
	private string RecievedData = "Waiting for Inputs...";
	private string FormatedData;

	private string SendingPacket;
	private int PlayerID;
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
		if(connected)
		{
			string tmp = Marshal.PtrToStringAnsi(NetworkingManager.TCP_GetData());
			if(!String.Equals(tmp,"[]"))
			{
				// NOTE:: This indicates the player that sent the packet, not the current player ID
				int PlayerPacketID;

				RecievedData = tmp;

				var packet = (JSON.Parse(RecievedData).AsArray)[0];
				PlayerPacketID = packet[NetworkKeyString.PlayerID].AsInt;
				switch((NetworkCode)packet["ID"].AsInt)
				{
				case NetworkCode.TeamChangeRequest:
				{
					int TeamID = packet[NetworkKeyString.TeamID].AsInt;
					GameData.LobbyData[PlayerPacketID].TeamID = TeamID;

					FormatedData = "Recieved from player ID: " + PlayerPacketID.ToString() + ", for team request on team: " + TeamID.ToString();
					// TODO::Spenser - Do GUI update here using TeamID and PlayerPacketID when player has changed team
					break;
				}
				case NetworkCode.ClassChangeRequest:
				{
					int ClassID = packet[NetworkKeyString.ClassID].AsInt;
					GameData.LobbyData[PlayerPacketID].ClassType = (ClassType)ClassID;

					FormatedData = "Recieved from player ID: " + PlayerPacketID.ToString() + ", for class request on class: " + ClassID.ToString();
					// TODO::Spener - DO GUI update here using ClassID and PlayerPacketID when player has changed class
					break;
				}
				case NetworkCode.ReadyRequest:
				{
					bool IsReady = packet[NetworkKeyString.Ready].AsBool;
					GameData.LobbyData[PlayerPacketID].Ready = IsReady;

					FormatedData = "Recieved from player ID: " + PlayerPacketID.ToString() + ", which is " + (IsReady ? "Ready" : "Not Ready");
					// TODO::Spenser - Do GUI update here using IsReady and PlayerPacketID when player changed ready status
					break;
				}
				case NetworkCode.PlayerJoinedLobby:
				{
					if(GameData.MyPlayerID == 0)
						GameData.MyPlayerID = PlayerPacketID;
					else
					{
						GameData.LobbyData[PlayerPacketID].PlayerID = PlayerPacketID;
						GameData.LobbyData[PlayerPacketID].Username = packet[NetworkKeyString.UserName];
					}
					break;
				}
				case NetworkCode.UpdatePlayerList:
				{
					// fills in existing player data
					foreach(JSONNode playerData in packet["LobbyData"].AsArray) 
					{
						int id = playerData[NetworkKeyString.PlayerID].AsInt;
						GameData.LobbyData[id].PlayerID 	= id;
						GameData.LobbyData[id].ClassType 	= (ClassType)playerData[NetworkKeyString.ClassID].AsInt;
						GameData.LobbyData[id].TeamID 		= playerData[NetworkKeyString.TeamID].AsInt;
						GameData.LobbyData[id].Ready 		= playerData[NetworkKeyString.Ready].AsBool;
						GameData.LobbyData[id].Username		= playerData[NetworkKeyString.UserName];
					}
					break;
				}
				case NetworkCode.GameStart:
				{
					// start the game
					break;
				}
				}

			}
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
        string ip 	= _GetInputText("IPInput");
		_SwitchMenu(MenuStates.Lobby);
        // TODO: validate player name and IP addr
        if (!(name.Length == 0) && !(ip.Length == 0))
        {
            _player_name = name;

            // Connect to the server
			if(NetworkingManager.TCP_ConnectToServer(ip, 7000) < 0)
			{
				RecievedData = "Cant connect to server";
				return;
			}
			// Thread creation
			if(NetworkingManager.TCP_StartReadThread() < 0)
			{
				RecievedData =  "Error creating read thread";
				return;
			}

			RecievedData = "Connected";
			connected = true;

			List<Pair<string, string>> packetData = new List<Pair<string, string>>();
			packetData.Add(new Pair<string, string>(NetworkKeyString.UserName, _player_name));
			SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.PlayerJoinedLobby, packetData, Protocol.NA);

		}else
			RecievedData = "IP length is zero";

    }

    // === game lobby menu ===
    public void ChooseTeam()
    {
        team_select_panel.SetActive(true);
        _RemovePlayerFromLobbyList(0);
    }
    public void SelectTeam(int team)
    {
		// TODO:: Fix this
        // add player text with appropriate colour to canvas
		// This call is causing an error
        //_AddPlayerToLobbyList(_player_name, team);

		// Construct json packet 
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.TeamID, team.ToString()));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.TeamChangeRequest, packetData, Protocol.NA);
		Debug.Log(SendingPacket);
		// Send dummy packet
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
		{
			// handle error here 
			Debug.Log("SelectTeam(): Packet sending failed\n");
		}

        team_select_panel.SetActive(false);
    }
	public void SelectClass(int value)
	{
		// TODO: associate class value with player here
		class_select_panel.SetActive(false);
		
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, PlayerID.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.ClassID, value.ToString()));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.ClassChangeRequest, packetData, Protocol.NA);
		Debug.Log(SendingPacket);
		
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
		packetData.Add(new Pair<string, string>(NetworkKeyString.StartGame, "1"));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.GameStart, packetData, Protocol.NA);
		Debug.Log(SendingPacket);
		
		// Send dummy packet
		if(NetworkingManager.TCP_Send(SendingPacket, 256) < 0)
		{
			// handle error here 
			Debug.Log("SelectTeam(): Packet sending failed\n");
		}
		
		team_select_panel.SetActive(false);
	}
	
	public void SetReady(bool isReady)
	{
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add (new Pair<string, string>("PlayerID", PlayerID.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.Ready, isReady ? "1" : "0"));
		SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.ReadyRequest, packetData, Protocol.NA);
		Debug.Log(SendingPacket);
		
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
		//disconnect from server

        foreach (GameObject go in _lobby_list)
        {
            Destroy(go);
            _y_offset--;
        }
        _SwitchMenu(MenuStates.Previous);
    }

	public void LeaveLobby()
	{
		if(connected)
		{
			foreach (GameObject go in _lobby_list)
			{
				Destroy(go);
				_y_offset--;
			}
			_SwitchMenu(MenuStates.Previous);
			connected = false;
			NetworkingManager.TCP_DisposeClient();

		}
		Back();
	}

	/* 
	 * Private Functions 
	 */
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
}
