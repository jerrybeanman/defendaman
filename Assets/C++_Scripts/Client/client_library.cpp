#include "Client.h"

using namespace Networking;

/* 
	Creates a client object, which will return an IntPtr type in C#
	
	Interface:	Client * TCP_CreateTCPClient()

	Programmer: Jerry Jia

	@return: new Client object
*/
extern "C" Client * TCP_CreateTCPClient()
{
	return new Client();
}

/* 
	Free a client object from heap 
	
	Interface: 	void TCP_DisposeTCPClient(Client* client)
				[client] Pointer to the client obeject (In this case, the IntPtr value in C#)

	Programmer: Jerry Jia

	@return: void
*/
extern "C" void TCP_DisposeTCPClient(Client* client)
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
extern "C" int TCP_ConnectToServer(Client * client, const char * name, short port)
{
	if(client != NULL)
		return client->Init_TCP_Client_Socket(name, port);
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
extern "C" int TCP_StartReadThread(Client * client)
{
	return pthread_create(&client->ReadThread, NULL, &TCPRecv, (void *)client);
}


/* 
	Grabs data packets stored in the circular buffer in the Client object
	
	Interface:	char * TCP_GetData(Client * client)
				[client] Pointer to the client object (In this case, the IntPtr value in C#)
				

	Programmer: Jerry Jia
	
	@return: "[]" if the circular buffer is empty, otherwise the packet pointed by rear
*/
extern "C" char * TCP_GetData(Client * client)
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
	
	@return: Send a packet to the peer host
*/
extern "C" int TCP_Send(Client * client, char * message, int size)
{
	return client->Send(message, size);
}
