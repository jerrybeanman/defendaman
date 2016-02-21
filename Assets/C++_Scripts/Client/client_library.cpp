#include "Client.h"

using namespace Networking;

extern "C" int GetTwo()
{
	return 2;
}

extern "C" Client * CreateClient()
{
	return new Client();
}

extern "C" void DisposeClient(Client* client)
{
	if(client != NULL)
	{
		delete client;
		client = NULL;
	}
}

extern "C" int Call_Init_TCP_Client_Socket(Client * client, char * name, short port)
{
	if(client != NULL)
		client->Init_TCP_Client_Socket(name, port);
    return -1;
}

extern "C" int Call_Send(Client * client, char * message, int size)
{
	return client->Send(message, size);
}

extern "C" int Call_Recv(Client * client, char * message, int size, int * recvbytes)
{
	return client->Recv(message, size, recvbytes);
}