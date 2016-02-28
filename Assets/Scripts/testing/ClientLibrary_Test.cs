using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;

public class ClientLibrary_Test : MonoBehaviour {
	
	[DllImport("ClientLibrary.so")]
	private static extern IntPtr CreateClient();
	
	[DllImport("ClientLibrary.so")]
	private static extern void DisposeClient(IntPtr client);
	
	[DllImport("ClientLibrary.so")]
	private static extern int Call_Init_TCP_Client_Socket(IntPtr client, 
	                                                      string  name, 
	                                                      short port);
	
	[DllImport("ClientLibrary.so")]
	private static extern int Call_Send(IntPtr client, 
	                                    string message, 
	                                    int size);
	
	[DllImport("ClientLibrary.so")]
	private static extern IntPtr GetData(IntPtr client);
	
	[DllImport("ClientLibrary.so")]
	private static extern int GetTwo();
	
	
	private	IntPtr 	TCPClient;
	public 	string  IP; 
	public  short 	port = 7000;
	public 	Sprite 	alive;
	public 	Sprite	dead;

	private string RecievedData = "";



	void Update()
	{
		RecievedData = Marshal.PtrToStringAnsi(GetData(TCPClient));
	}


	void OnGUI()
	{
		GUI.Label(new Rect(10, 20, Screen.width, Screen.height), "Reading()");
		GUI.Label(new Rect(10, 40, Screen.width, Screen.height), RecievedData);
		
	}

	private string CreateString(String s)
	{
		string newstring = "";
		newstring += s;
		return newstring;
	}
	public void B_CreateClient()
	{
		TCPClient = CreateClient();
		GetComponent<SpriteRenderer>().sprite = alive;
	}
	
	public void B_CreateSocket()
	{
		int 	errno;
		if((errno = Call_Init_TCP_Client_Socket(TCPClient, IP, port)) < 0)
		{
			return;
		}else
		{
			GetComponent<SpriteRenderer>().sprite = dead;
		}
	}


	public void B_GetTwo()
	{
		if(GetTwo() == 2)
		{
			GetComponent<SpriteRenderer>().sprite = dead;
		}
	}
	
	public void B_Send()
	{
		string message = "jerrybeanman team2";
		Call_Send(TCPClient, message, message.Length);
	}
	
	void OnDestroy()
	{
		DisposeClient(TCPClient);
		TCPClient = IntPtr.Zero;
	}
}
