using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// Networking Code for lobby messages
public enum NetworkCode
{
	Seed				= 0,
	TeamChangeRequest 	= 1,
	ClassChangeRequest 	= 2,
	ReadyRequest 		= 3,
	PlayerJoinedLobby 	= 4,
	GameStart 			= 5,
	UpdatePlayerList	= 6,
	PlayerLeftLobby		= 7
}



public class LobbyNetwork : MonoBehaviour {

	// Message that will be updated to send
	public static string 		SendingPacket;
	// Indicates weather or not if we're connected to the network
	public static bool 			connected = false;
	// Debug stuff
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
			packetData.Add(new Pair<string, string>(NetworkKeyString.ClassID, ((int)GameData.MyPlayer.ClassType).ToString()));
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

	public static bool Start = false;

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
				GameData.LobbyData[PlayerPacketID].TeamID = packet[NetworkKeyString.TeamID].AsInt;

                break;
            }
            case NetworkCode.ClassChangeRequest:
            {
				GameData.LobbyData[PlayerPacketID].ClassType = (ClassType)packet[NetworkKeyString.ClassID].AsInt;
				break;
            }
            case NetworkCode.ReadyRequest:
            {
				GameData.LobbyData[PlayerPacketID].Ready = Convert.ToBoolean(packet[NetworkKeyString.Ready].AsInt);

				PrintData();
                break;
            }
            case NetworkCode.PlayerJoinedLobby:
            {
                PlayerData tmpPlayer = new PlayerData();
                tmpPlayer.Ready = false;
                tmpPlayer.PlayerID = PlayerPacketID;
                tmpPlayer.Username = packet[NetworkKeyString.UserName];
                if (GameData.MyPlayer.PlayerID == -1)
                {
                    GameData.MyPlayer.PlayerID = PlayerPacketID;
                }
                else
                    GameData.LobbyData.Add(PlayerPacketID, tmpPlayer);
				PrintData();
                break;
            }
            case NetworkCode.UpdatePlayerList:
            {
                // fills in existing player data
                foreach (JSONNode playerData in packet["LobbyData"].AsArray)
                {
					int id = playerData[NetworkKeyString.PlayerID].AsInt;
					PlayerData tempPlayer 	= new PlayerData();
					tempPlayer.Ready 		= false;
					tempPlayer.PlayerID  	= id;
					tempPlayer.ClassType 	= (ClassType)playerData[NetworkKeyString.ClassID].AsInt;
					tempPlayer.TeamID 		= playerData[NetworkKeyString.TeamID].AsInt;
					tempPlayer.Ready 		= Convert.ToBoolean(playerData[NetworkKeyString.Ready].AsInt);
					tempPlayer.Username		= playerData[NetworkKeyString.UserName];
                    GameData.LobbyData.Add(id, tempPlayer);

                    print("[Debug] UpdatePlayerList()" + playerData[NetworkKeyString.TeamID].AsBool);
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
                break;
            }
			case NetworkCode.Seed:
			{
				GameData.Seed = packet["Seed"].AsInt;
				Application.LoadLevel("hud_test");
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
