using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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

public class LobbyNetwork : MonoBehaviour {

	public static string 		SendingPacket;
	public static bool 			connected = false;
	public static string 		RecievedData = "Waiting for input...";


	public static bool Connect(string ip)
	{
		// Connect to the server
		if (NetworkingManager.TCP_ConnectToServer(ip, 7000) < 0)
		{
			RecievedData = "Cant connect to server";
			return false;
		}
		// Thread creation
		if (NetworkingManager.TCP_StartReadThread() < 0)
		{
			RecievedData = "Error creating read thread";
			return false;
		}

		RecievedData = "Connected";
		connected = true;
		return true;
	}

	public static void SendLobbyData(NetworkCode code)
	{
		// Construct json packet
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		if(code != NetworkCode.PlayerJoinedLobby)
			packetData.Add (new Pair<string, string>(NetworkKeyString.PlayerID, GameData.MyPlayer.PlayerID.ToString()));

		switch(code)
		{
		case NetworkCode.TeamChangeRequest:
			packetData.Add(new Pair<string, string>(NetworkKeyString.TeamID, GameData.MyPlayer.TeamID.ToString()));
			Send(NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.TeamChangeRequest, packetData, Protocol.NA));
			break;
		case NetworkCode.ClassChangeRequest:
			packetData.Add(new Pair<string, string>(NetworkKeyString.ClassID, (int)GameData.MyPlayer.ClassType);
			Send(NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.ClassChangeRequest, packetData, Protocol.NA));
			break;
		case NetworkCode.ReadyRequest:
			packetData.Add(new Pair<string, string>(NetworkKeyString.Ready, GameData.MyPlayer.Ready ? "1" : "0"));
			Send(NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.ReadyRequest, packetData, Protocol.NA));
			break;
		case NetworkCode.PlayerJoinedLobby:
			packetData.Add(new Pair<string, string>(NetworkKeyString.UserName, "\"" + GameData.MyPlayer.Username + "\""));
			Send(NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.PlayerJoinedLobby, packetData, Protocol.NA));
			break;
		case NetworkCode.PlayerLeftLobby:
			SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.PlayerLeftLobby, packetData, Protocol.NA);
			Send(NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.PlayerLeftLobby, packetData, Protocol.NA));
			break;
		case NetworkCode.GameStart:
			SendingPacket = NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.GameStart, packetData, Protocol.NA);
			Send(NetworkingManager.send_next_packet(DataType.Lobby, (int)NetworkCode.GameStart, packetData, Protocol.NA));
			break;
		}
	}

	private static void Send(string packet)
	{
		if(NetworkingManager.TCP_Send(packet, 256) < 0)
			Debug.Log("[Debug]: SelectTeam(): Packet sending failed\n");
	}
	public static void ParseLobbyData(string raw)
	{
        int PlayerPacketID;
		Debug.Log("[Debug]: " + raw);
        RecievedData = raw;
        var packet = (JSON.Parse(RecievedData).AsArray)[0];
        PlayerPacketID = packet[NetworkKeyString.PlayerID].AsInt;
        switch ((NetworkCode)packet["ID"].AsInt)
        {
            case NetworkCode.TeamChangeRequest:
            {
                Debug.Log("[Debug]: Team chagne request");
                Debug.Log("[Debug]: Content--" + raw);
				GameData.LobbyData[PlayerPacketID].TeamID = packet[NetworkKeyString.TeamID].AsInt;;

				PrintData();
                break;
            }
            case NetworkCode.ClassChangeRequest:
            {
                Debug.Log("[Debug]: Class change request");
                Debug.Log("[Debug]: Content--" + raw);
				GameData.LobbyData[PlayerPacketID].ClassType = (ClassType)packet[NetworkKeyString.ClassID].AsInt;;

				PrintData();
				break;
            }
            case NetworkCode.ReadyRequest:
            {
                Debug.Log("[Debug]: Ready request");
                Debug.Log("[Debug]: Content--" + raw);
				GameData.LobbyData[PlayerPacketID].Ready = packet[NetworkKeyString.Ready].AsBool;;

				PrintData();
                break;
            }
            case NetworkCode.PlayerJoinedLobby:
            {
                Debug.Log("[Debug]: Player has joined lobby");
                Debug.Log("[Debug]: Content--" + raw);

                PlayerData tmpPlayer = new PlayerData();
                tmpPlayer.PlayerID = PlayerPacketID;
                tmpPlayer.Username = packet[NetworkKeyString.UserName];
                if (GameData.MyPlayer.PlayerID == -1)
                {
                	Debug.Log("[Debug]: Got our own ID!");
                    GameData.MyPlayer.PlayerID = PlayerPacketID;
                }
                else
                    GameData.LobbyData.Add(PlayerPacketID, tmpPlayer);
				PrintData();
                break;
            }
            case NetworkCode.UpdatePlayerList:
            {
                Debug.Log("[Debug]: Got update table message");
                Debug.Log("[Debug]: Content--" + raw);

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
                break;
            }
			case NetworkCode.PlayerLeftLobby:
			{
				GameData.LobbyData.Remove(PlayerPacketID);
				break;
			}
            case NetworkCode.GameStart:
            {
				RecievedData = "Player: " + PlayerPacketID + " has started the game!";
				// start game stuff here
                break;
            }
        }
	}



	public static void PrintData()
	{
		Debug.Log("[Debug]: IN PrintData()");
		foreach(PlayerData p in GameData.LobbyData.Values)
		{
			Debug.Log("[Debug]: Player ID: " + p.PlayerID.ToString() + "ClassID: " +
			          p.ClassType.ToString() + "TeamID: " + p.TeamID.ToString() + "Username: " + p.Username);
		}
	}
}
