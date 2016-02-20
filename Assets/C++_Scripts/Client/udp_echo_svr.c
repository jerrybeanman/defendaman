// A simple echo server using UCP 

#include <stdio.h>
#include <netdb.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <errno.h>
#include <strings.h>
#include <arpa/inet.h>
#include <stdlib.h>
#include <unistd.h>

#define SERVER_UDP_PORT 	8000	// Default port
#define MAXLEN			65000	//Buffer length
#define TRUE			1
 
int main (int argc, char **argv)
{
	int	sd, port, n;
	socklen_t client_len;
	char	buf[MAXLEN];
	struct	sockaddr_in server, client;

	switch(argc)
	{
		case 1:
			port = SERVER_UDP_PORT;	// Default port
		break;
		case 2:
			port = atoi(argv[1]);	//User specified port
		break;
		default:
			fprintf(stderr, "Usage: %s [port]\n", argv[0]);
			exit(1);
   	}

	// Create a non-blocking datagram socket
	if ((sd = socket (AF_INET, SOCK_DGRAM | SOCK_NONBLOCK, 0)) == -1)
	{
		perror ("Can't create a socket"); 
		exit(1);
	}

	// Bind an address to the socket
	bzero((char *)&server, sizeof(server)); 
	server.sin_family = AF_INET; 
	server.sin_port = htons(port); 
	server.sin_addr.s_addr = htonl(INADDR_ANY);
	if (bind (sd, (struct sockaddr *)&server, sizeof(server)) == -1)
	{
		perror ("Can't bind name to socket");
		exit(1);
	}

	while (TRUE)
	{
		client_len = sizeof(client);
		if ((n = recvfrom (sd, buf, MAXLEN, 0, (struct sockaddr *)&client, &client_len)) < 0)
		{
			//perror ("recvfrom error");
			//exit(1);
		}
		else {
			printf ("Received %d bytes\t", n);
			printf ("Msg:%s\n", buf);
			printf ("From host: %s\n", inet_ntoa (client.sin_addr));
		}
		if (sendto (sd, buf, n, 0,(struct sockaddr *)&client, client_len) != n)
		{
			//perror ("sendto error");
			//exit(1);
		}

	}
	close(sd);
	return(0);
}
