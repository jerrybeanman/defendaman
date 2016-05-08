#ifndef CLIENT_H_
#define CLIENT_H_
#include <iostream>
#include <cstring>
#include <cstdlib>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <utility>
#include <stdio.h>
#include <errno.h>
#include <unistd.h>
#include <pthread.h>
#include "CircularBuffer.h"

#define PACKETLEN    4096
#define PACKETLEN_UDP 512
#define MAXPACKETS   100
/*------------------------------------------------------------------------------

  SOURCE FILE:              Client.h

  PROGRAM:                  Defendaman

  FUNCTIONS:                virtual ~Client() {};
                            virtual int Init_Client_Socket(const char* name, short port) = 0
                            virtual int Send(char * message, int size) = 0;
                            virtual void * Recv() = 0;
                            void fatal(const char* error);
                            int Init_SockAddr(const char* hostname, short hostport);
                            char* GetData();

  DESIGNER/PROGRAMMER:      Jerry Jia, Martin Minkov, Tyler Trepanier, Scott Plummer

  NOTES:                    The client base class for both TCP and UDP network
-------------------------------------------------------------------------------*/
namespace Networking
{
    class Client
    {
        public:
          int             serverSocket;
          pthread_t       ReadThread;

          Client();
          virtual ~Client() {};

          virtual int Init_Client_Socket(const char* name, short port) = 0;

          virtual void * Recv() = 0;

          virtual int Send(char * message, int size) = 0;

          void fatal(const char* error);

          int Init_SockAddr(const char* hostname, short hostport);

          char* GetData();

        protected:
            CircularBuffer  CBPackets; // buffer for data coming in from network
            struct          sockaddr_in serverAddr;
            char*           currentData; // the single instance of data exposed to Unity for reading
    };
}
#endif
