#include "Client.h"
#include "GameClient.h"
#include "LobbyClient.h"
using namespace Networking;

/*
	Creates a client object, which will return an IntPtr type in C#

	Interface:	Client * TCP_CreateTCPClient()

	Programmer: Jerry Jia

	@return: new Client object
*/
extern "C" LobbyClient * TCP_CreateClient()
{
	return new LobbyClient();
}

/*
	Free a client object from heap

	Interface: 	void TCP_DisposeTCPClient(Client* client)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)

	Programmer: Jerry Jia

	@return: void
*/
extern "C" void TCP_DisposeClient(LobbyClient* client)
{
	if(client != NULL)
	{
		delete client;
		client = NULL;
	}
}

/*
	Connects to the server, calls Init_TCP_Client_Socket() in the Client object

	Interface:	int TCP_ConnectToServer(Client * client, const char * name, short port)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)
				[name] 	 IP address of peer host

	Programmer: Jerry Jia

	@return: void
*/
extern "C" int TCP_ConnectToServer(LobbyClient * client, const char * name, short port)
{
	if(client != NULL)
		return client->Init_Client_Socket(name, port);
    return -1;
}

/*
	Helper function for thread creation. Calls recv() on the Client object.

	Interface:	void * TCP_Recv(void * arg)
				[arg] Has to take a client object

	Programmer: Jerry Jia

	@return: function pointer to Client->Recv()
*/
void * TCP_Recv(void * arg)
{
	return ((Client *)arg)->Recv();
}

/*
	Creates a thread for recieving packet data

	Interface:	void TCP_StartReadThread(Client * client)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)

	Programmer: Jerry Jia

	@return: void
*/
extern "C" int TCP_StartReadThread(LobbyClient * client)
{
	return pthread_create(&client->ReadThread, NULL, &TCP_Recv, (void *)client);
}


/*
	Grabs data packets stored in the circular buffer in the Client object

	Interface:	char * TCP_GetData(Client * client)
				[client] Pointer to the client object (In this case, the IntPtr value in C#)


	Programmer: Jerry Jia

	@return: "[]" if the circular buffer is empty, otherwise the packet pointed by rear in client->CircularBuf
*/
extern "C" char * TCP_GetData(LobbyClient * client)
{
		return client->GetData();
}

/*
	Sends a message to the server socket

	Interface:	int TCP_Send(Client * client, char * message, int size)
				[client] 	Pointer to the client object (In this case, the IntPtr value in C#)
				[message]	The packet to send
				[size]		Size of the packet

	Programmer: Jerry Jia

	@return: -1 for failure, 0 on success
*/
extern "C" int TCP_Send(LobbyClient * client, char * message, int size)
{
	return client->Send(message, size);
}

/*===========================================
				GAME CLIENT
===========================================*/


/*
	Creates a game client object, which will return an IntPtr type in C#

	Interface:	GameClient * Game_CreateClient()

	Programmer: Gabriel Lee, Tyler Trepanier

	@return: new GameClient object
*/
extern "C" GameClient * Game_CreateClient()
{
	return new GameClient();
}

/*
	Free a game client object from heap

	Interface: 	void Game_DisposeClient(GameClient* client)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)

	Programmer: Gabriel Lee, Tyler Trepanier

	@return: void
*/
extern "C" void Game_DisposeClient(GameClient* client)
{
	if(client != NULL)
	{
		delete client;
		client = NULL;
	}
}

/*
	Connects to the server, calls Init_TCP_Client_Socket() in the Client object

	Interface:	int Game_ConnectToServer(Client * client, const char * name, short port)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)
				[name] 	 IP address of peer host

	Programmer: Gabriel Lee, Tyler Trepanier

	@return: int
*/
extern "C" int Game_ConnectToServer(GameClient * client, const char * name, short port)
{
	if(client != NULL)
		return client->Init_Client_Socket(name, port);
    return -1;
}

/*
	Helper function for thread creation. Calls recv() on the Client object.

	Interface:	void * Game_Recv(void * arg)
				[arg] Has to take a client object

	Programmer: Gabriel Lee, Tyler Trepanier

	@return: function pointer to Client->Recv()
*/
void * Game_Recv(void * arg)
{
	return ((Client *)arg)->Recv();
}

/*
	Creates a thread for recieving packet data

	Interface:	void Game_StartReadThread(GameClient * client)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)

	Programmer: Gabriel Lee, Tyler Trepanier

	@return: int
*/
extern "C" int Game_StartReadThread(GameClient * client)
{
	return pthread_create(&client->ReadThread, NULL, &Game_Recv, (void *)client);
}


/*
	Grabs data packets stored in the circular buffer in the Client object

	Interface:	char * Game_GetData(Client * client)
				[client] Pointer to the client object (In this case, the IntPtr value in C#)


	Programmer: Gabriel Lee, Tyler Trepanier

	@return: "[]" if the circular buffer is empty, otherwise the packet pointed by rear in client->CircularBuf
*/
extern "C" char * Game_GetData(GameClient * client)
{
		return client->GetData();
}

/*
	Sends a message to the server socket

	Interface:	int Game_Send(Client * client, char * message, int size)
				[client] 	Pointer to the client object (In this case, the IntPtr value in C#)
				[message]	The packet to send
				[size]		Size of the packet

	Programmer: Gabriel Lee, Tyler Trepanier

	@return: -1 for failure, 0 on success
*/
extern "C" int Game_Send(GameClient * client, char * message, int size)
{
	return client->Send(message, size);
}
