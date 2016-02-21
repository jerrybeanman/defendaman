#include "Server.h"

using namespace Networking;

/****************************************************************************
Initialize server socket, fill in the server address, and binds the address
to the socket

@return: void
*****************************************************************************/
int Server::Init_UDP_Server_Socket(short port)
{
    int err;
    /* Create a file descriptor for the socket */

    if((err = _UDPReceivingSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) < 0)
    {
        fatal("Init_UDP_Server_Socket: socket() failed\n");
        return err;
    }

    /* Fill in server socket address structure */
    memset((char *)&_ServerAddress, 0, sizeof(_ServerAddress));
    _ServerAddress.sin_family = AF_INET;
    _ServerAddress.sin_port = htons(port);
    _ServerAddress.sin_addr.s_addr = htonl(INADDR_ANY);

    /* Bind server address to the socket */
    if((err = bind(_UDPReceivingSocket, (sockaddr *)&_ServerAddress, sizeof(_ServerAddress))) < 0)
    {
        fatal("Init_UDP_Server_Socket: bind() failed\n");
        return err;
    }

    return 0;

}

/****************************************************************************
Listen for incoming UDP traffics

@return: a packet
*****************************************************************************/
std::string Server::UDP_Server_Recv()
{
    int err;
    std::string packet;                     /* packet recieved  */
    struct sockaddr_in Client;              /* Incoming client's socket address information */
    unsigned ClientLen = sizeof(Client);
    char * buf = (char *)malloc(BUFSIZE);

    if((err = recvfrom(_UDPReceivingSocket, buf, BUFSIZE, 0, (sockaddr *)&Client, &ClientLen)) <= 0)
    {
        fatal("UDP_Server_Recv: recvfrom() failed\n");
        return NULL;
    }
	_ClientAddress.push_back(Client);
    packet = buf;
    free(buf);
    return packet;
}

/* Echoes the message back to all clients*/
void Server::UDP_Server_Send(const char* message)
{
	for (int i = 0; 0 <_ClientAddresses.size();i++){
		if (sendto(_UDPReceivingSocket, message, sizeof(message), 0 , (sockaddr *)&_ClientAddress[i], sizeof(_ClientAddress[i])) == -1)
		{
			fatal("Send to UDP Client failed \n");
		}
	}
}

void Server::fatal(const char* error)
{
    std::cerr << error << std::endl;
}

