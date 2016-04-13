/*------------------------------------------------------------------------------

  SOURCE FILE:              client_library.cpp

  PROGRAM:                  Defendaman

  FUNCTIONS:                int getSO_ERROR(int fd)
                            void closeSocket(int fd)
                            extern "C" LobbyClient * TCP_CreateClient()
                            extern "C" void TCP_DisposeClient(LobbyClient* client)
                            extern "C" void TCP_DisposeClient(LobbyClient* client)
                            extern "C" int TCP_ConnectToServer(LobbyClient * client, const char * name, short port)
                            void * TCP_Recv(void * arg)
                            extern "C" int TCP_StartReadThread(LobbyClient * client)
                            extern "C" char * TCP_GetData(LobbyClient * client)
                            extern "C" int TCP_Send(LobbyClient * client, char * message, int size)
                            extern "C" GameClient * UDP_CreateClient()
                            extern "C" void UDP_DisposeClient(GameClient* client)
                            extern "C" int UDP_ConnectToServer(GameClient * client, const char * name, short port)
                            void * UDP_Recv(void * arg)
                            extern "C" int UDP_StartReadThread(GameClient * client)
                            extern "C" char * UDP_GetData(GameClient * client)
                            extern "C" int UDP_Send(GameClient * client, char * message, int size)


  DESIGNER/PROGRAMMER:      Gabriel Lee, Tyler Trepanier-Bracken, Vivek Kalia, Jerry

  NOTES:                    C++ library used for networking

-------------------------------------------------------------------------------*/
#include "Client.h"
#include "GameClient.h"
#include "LobbyClient.h"
using namespace Networking;

/*------------------------------------------------------------------------------

  FUNCTION:                 getSO_ERROR

  DESIGNER/PROGRAMMER:      Jerry Jia

  INTERFACE:                int getSO_ERROR(int fd)

  RETURNS:                  error

  NOTES:
-------------------------------------------------------------------------------*/
int getSO_ERROR(int fd) {
   int err = 1;
   socklen_t len = sizeof err;
   if (-1 == getsockopt(fd, SOL_SOCKET, SO_ERROR, (char *)&err, &len))
      printf("getSO_ERROR\n");
   if (err)
      errno = err;              // set errno to the socket SO_ERROR
   return err;
}
/*------------------------------------------------------------------------------

  FUNCTION:               closeSocket

  DESIGNER/PROGRAMMER:    Jerry Jia

  INTERFACE:              void closeSocket(int fd)

  RETURNS:                void

  NOTES:
-------------------------------------------------------------------------------*/
void closeSocket(int fd)
{
	// *not* the Windows closesocket()
   if (fd >= 0) {
      getSO_ERROR(fd); // first clear any errors, which can cause close to fail
      if (shutdown(fd, SHUT_RDWR) < 0) // secondly, terminate the 'reliable' delivery
         if (errno != ENOTCONN && errno != EINVAL) // SGI causes EINVAL
            printf("shutdown\n");
      if (close(fd) < 0) // finally call close()
         printf("close\n");
   }
}
/*------------------------------------------------------------------------------

  FUNCTION:               TCP_CreateTCPClient

  DESIGNER/PROGRAMMER:    Jerry Jia

  INTERFACE:              Client * TCP_CreateTCPClient()

  RETURNS:                new Client object

  NOTES:                  	Creates a client object, which will return an IntPtr type in C#

-------------------------------------------------------------------------------*/
extern "C" LobbyClient * TCP_CreateClient()
{
	return new LobbyClient();
}
/*------------------------------------------------------------------------------

  FUNCTION:                   TCP_DisposeTCPClient

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  Interface: 	void TCP_DisposeTCPClient(Client* client)
				                      [client] Pointer to the client obeject (In this case, the IntPtr value in C#)

  RETURNS:                    void

  NOTES:                      Free a client object from heap

-------------------------------------------------------------------------------*/
extern "C" void TCP_DisposeClient(LobbyClient* client)
{
	if(client != NULL)
	{
		printf("Closing Client.............\n");
		closeSocket(client->serverSocket);
		//free(client)
	}
}
/*------------------------------------------------------------------------------

  FUNCTION:                 TCP_ConnectToServer

  DESIGNER/PROGRAMMER:      Jerry Jia

  INTERFACE:               Interface:	int TCP_ConnectToServer(Client * client, const char * name, short port)
  				                        [client] Pointer to the client obeject (In this case, the IntPtr value in C#)
  				                        [name] 	 IP address of peer host

  RETURNS:

  NOTES:  Connects to the server, calls Init_TCP_Client_Socket() in the Client object
-------------------------------------------------------------------------------*/
extern "C" int TCP_ConnectToServer(LobbyClient * client, const char * name, short port)
{
	if(client != NULL)
  {
    printf("before TCP_ConnectToServer\n");
		return client->Init_Client_Socket(name, port);
  }
    return -1;
}
/*------------------------------------------------------------------------------

  FUNCTION:               TCP_Recv

  DESIGNER/PROGRAMMER:    Jerry Jia

  INTERFACE:              Interface:	void * TCP_Recv(void * arg)
				                      [arg] Has to take a client object

  RETURNS:               unction pointer to Client->Recv()

  NOTES:                  Helper function for thread creation. Calls recv() on the Client object.

-------------------------------------------------------------------------------*/
void * TCP_Recv(void * arg)
{
	return ((Client *)arg)->Recv();
}
/*------------------------------------------------------------------------------

  FUNCTION:               TCP_StartReadThread

  DESIGNER/PROGRAMMER:    Jerry Jia

  INTERFACE:              Interface:	void TCP_StartReadThread(Client * client)
				                      [client] Pointer to the client obeject (In this case, the IntPtr value in C#)

  RETURNS:                void

  NOTES:                  Creates a thread for recieving packet data

-------------------------------------------------------------------------------*/
extern "C" int TCP_StartReadThread(LobbyClient * client)
{
	return pthread_create(&client->ReadThread, NULL, &TCP_Recv, (void *)client);
}

/*------------------------------------------------------------------------------

  FUNCTION:                 TCP_GetData

  DESIGNER/PROGRAMMER:      jerry Jia

  INTERFACE:                Interface:	char * TCP_GetData(Client * client)
				                      [client] Pointer to the client object (In this case, the IntPtr value in C#)

  RETURNS:

  NOTES:                  "[]" if the circular buffer is empty, otherwise the packet pointed by rear in client->CircularBuf

-------------------------------------------------------------------------------*/
extern "C" char * TCP_GetData(LobbyClient * client)
{
		return client->GetData();
}
/*------------------------------------------------------------------------------

  FUNCTION:               TCP_Send

  DESIGNER/PROGRAMMER:    Jerry Jia

  INTERFACE:            Interface:	int TCP_Send(Client * client, char * message, int size)
				                    [client] 	Pointer to the client object (In this case, the IntPtr value in C#)
				                    [message]	The packet to send
				                    [size]		Size of the packet

  RETURNS:              1 for failure, 0 on success

  NOTES:                	Sends a message to the server socket

-------------------------------------------------------------------------------*/
extern "C" int TCP_Send(LobbyClient * client, char * message, int size)
{
	// ITS ONLY PACKET LEN, IGNORE SIZE FOR NOW
	return client->Send(message, PACKETLEN);
}

/*===========================================
				GAME CLIENT
===========================================*/

/*------------------------------------------------------------------------------

  FUNCTION:           UDP_CreateClient

  DESIGNER/PROGRAMMER:    Jerry Jia

  INTERFACE:          GameClient * UDP_CreateClient()

  RETURNS:            new GameClient object

  NOTES:              Creates a game client object, which will return an IntPtr type in C#

-------------------------------------------------------------------------------*/
extern "C" GameClient * UDP_CreateClient()
{
	return new GameClient();
}
/*------------------------------------------------------------------------------

  FUNCTION:                   UDP_DisposeClient

  DESIGNER/PROGRAMMER:        Gabriel Lee, Tyler Trepanier

  INTERFACE:                  Interface: 	void UDP_DisposeClient(GameClient* client)
				                          [client] Pointer to the client obeject (In this case, the IntPtr value in C#)

  RETURNS:                    void

  NOTES:                      Free a game client object from heap

-------------------------------------------------------------------------------*/
extern "C" void UDP_DisposeClient(GameClient* client)
{
	if(client != NULL)
	{
		closeSocket(client->serverSocket);
		client = NULL;
	}
}
/*------------------------------------------------------------------------------

  FUNCTION:                 Connects to the server, calls Init_TCP_Client_Socket() in the Client object

  DESIGNER/PROGRAMMER:        Gabriel Lee, Tyler Trepanier

  INTERFACE:            	Interface:	int UDP_ConnectToServer(Client * client, const char * name, short port)
  				                  [client] Pointer to the client obeject (In this case, the IntPtr value in C#)
  				                  [name] 	 IP address of peer host

  RETURNS:                  int

  NOTES:                    Connects to the server, calls Init_TCP_Client_Socket() in the Client object

-------------------------------------------------------------------------------*/
extern "C" int UDP_ConnectToServer(GameClient * client, const char * name, short port)
{
	if(client != NULL)
		return client->Init_Client_Socket(name, port);
  return -1;
}
/*------------------------------------------------------------------------------

  FUNCTION:                 	Helper function for thread creation. Calls recv() on the Client object.

  DESIGNER/PROGRAMMER:      Gabriel Lee, Tyler Trepanier

  INTERFACE:                	Interface:	void * UDP_Recv(void * arg)
  				                          [arg] Has to take a client object

  RETURNS:                 function pointer to Client->Recv()

  NOTES:                    Helper function for thread creation. Calls recv() on the Client object.

-------------------------------------------------------------------------------*/
void * UDP_Recv(void * arg)
{
	return ((Client *)arg)->Recv();
}
/*------------------------------------------------------------------------------

  FUNCTION:                 UDP_StartReadThread

  DESIGNER/PROGRAMMER:      Gabriel Lee, Tyler Trepanier

  INTERFACE:                Interface:	void UDP_StartReadThread(GameClient * client)
				                        [client] Pointer to the client obeject (In this case, the IntPtr value in C#)

  RETURNS:                  int

  NOTES:                    Creates a thread for recieving packet data

-------------------------------------------------------------------------------*/
extern "C" int UDP_StartReadThread(GameClient * client)
{
	return pthread_create(&client->ReadThread, NULL, &UDP_Recv, (void *)client);
}
/*------------------------------------------------------------------------------

  FUNCTION:                 UDP_GetData

  DESIGNER/PROGRAMMER:      Gabriel Lee, Tyler Trepanier

  INTERFACE:                Interface:	char * UDP_GetData(Client * client)
				                      [client] Pointer to the client object (In this case, the IntPtr value in C#)

  RETURNS:                  "[]" if the circular buffer is empty, otherwise the packet pointed by rear in client->CircularBuf

  NOTES:                  Grabs data packets stored in the circular buffer in the Client object

-------------------------------------------------------------------------------*/
extern "C" char * UDP_GetData(GameClient * client)
{
		return client->GetData();
}
/*------------------------------------------------------------------------------

  FUNCTION:             UDP_Send

  DESIGNER/PROGRAMMER:  Gabriel Lee, Tyler Trepanier

  INTERFACE:	           Interface:	int UDP_Send(Client * client, char * message, int size)
  				                [client] 	Pointer to the client object (In this case, the IntPtr value in C#)
  				                [message]	The packet to send
  				                [size]		Size of the packet

  RETURNS:              -1 for failure, 0 on success

  NOTES:                  Sends a message to the server socket

-------------------------------------------------------------------------------*/
extern "C" int UDP_Send(GameClient * client, char * message, int size)
{
	return client->Send(message, PACKETLEN_UDP);
}
