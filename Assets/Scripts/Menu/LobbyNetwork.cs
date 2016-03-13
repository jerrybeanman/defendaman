using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using System.Runtime.InteropServices;

/*public enum NetworkCode
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
}*/


public class LobbyNetwork : MonoBehaviour {

	private string SendingPacket;
	private int PlayerID = -1;
	private bool connected = false;

	private string RecievedData;
	void Update()
	{
		if(connected)
		{
			string tmp = Marshal.PtrToStringAnsi(NetworkingManager.TCP_GetData());
			if(!String.Equals(tmp,"[]"))
			{

			}
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 20, Screen.width, Screen.height), "Reading()");
		GUI.Label(new Rect(10, 40, Screen.width, Screen.height), RecievedData);
		
	}

	void ParseLobbyData(string raw)
	{
		// NOTE:: This indicates the player that sent the packet, not the current player ID
		int PlayerPacketID;
		
		RecievedData = raw;
		var packet = (JSON.Parse(RecievedData).AsArray)[0];
		PlayerPacketID = packet[NetworkKeyString.PlayerID].AsInt;
		switch((NetworkCode)packet["ID"].AsInt)
		{
		case NetworkCode.TeamChangeRequest:
		{
			Debug.Log("[Debug]: Team chagne request");
			Debug.Log("[Debug]: Content--" + raw);
			int TeamID = packet[NetworkKeyString.TeamID].AsInt;
			GameData.LobbyData[PlayerPacketID].TeamID = TeamID;
			
			// TODO::Spenser - Do GUI update here using TeamID and PlayerPacketID when player has changed team
			break;
		}
		case NetworkCode.ClassChangeRequest:
		{
			Debug.Log("[Debug]: Class change request");
			Debug.Log("[Debug]: Content--" + raw);
			int ClassID = packet[NetworkKeyString.ClassID].AsInt;
			GameData.LobbyData[PlayerPacketID].ClassType = (ClassType)ClassID;
			
			// TODO::Spener - DO GUI update here using ClassID and PlayerPacketID when player has changed class
			break;
		}
		case NetworkCode.ReadyRequest:
		{
			Debug.Log("[Debug]: Ready request");
			Debug.Log("[Debug]: Content--" + raw);
			bool IsReady = packet[NetworkKeyString.Ready].AsBool;
			GameData.LobbyData[PlayerPacketID].Ready = IsReady;
			
			// TODO::Spenser - Do GUI update here using IsReady and PlayerPacketID when player changed ready status
			break;
		}
		case NetworkCode.PlayerJoinedLobby:
		{
			Debug.Log("[Debug]: Player has joined lobby");
			Debug.Log("[Debug]: Content--" + raw);
			
			PlayerData tmpPlayer = new PlayerData();
			tmpPlayer.PlayerID = PlayerPacketID;
			tmpPlayer.Username = packet[NetworkKeyString.UserName];
			if(PlayerID == -1)
			{
				PlayerID = PlayerPacketID;
				GameData.MyPlayerID = PlayerPacketID;
				Debug.Log("[UltraDebug] Updated Player ID: " + PlayerID);
				
			}
			else	
				GameData.LobbyData.Add(PlayerPacketID, tmpPlayer);
			break;
		}
		case NetworkCode.UpdatePlayerList:
		{
			Debug.Log("[Debug]: Got update table message");
			Debug.Log("[Debug]: Content--" + raw);
			
			// fills in existing player data
			foreach(JSONNode playerData in packet["LobbyData"].AsArray) 
			{
				Debug.Log("[Debug]: IN UPDATEPLAYERLIST");
				int id = playerData[NetworkKeyString.PlayerID].AsInt;
				GameData.LobbyData[id].PlayerID 	= id;
				GameData.LobbyData[id].ClassType 	= (ClassType)playerData[NetworkKeyString.ClassID].AsInt;
				GameData.LobbyData[id].TeamID 		= playerData[NetworkKeyString.TeamID].AsInt;
				GameData.LobbyData[id].Ready 		= playerData[NetworkKeyString.Ready].AsBool;
				GameData.LobbyData[id].Username		= playerData[NetworkKeyString.UserName];
				
				Debug.Log("[Debug]: Player ID: " + GameData.LobbyData[id].PlayerID.ToString() + "ClassID: " + 
				          GameData.LobbyData[id].ClassType.ToString() + "TeamID: " + GameData.LobbyData[id].TeamID.ToString() + "Username: " + GameData.LobbyData[id].Username);
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
