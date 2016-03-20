/**********************************************************
Project: Defendaman

Progarmmer: Jerry Jia

Source File: Client.cpp

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
    Description: The UDP client class for the in-game data transfers.
**********************************************************/
#include "Client.h"
using namespace Networking;


int main()
{

}

Knight GetKnightOfLife()
{
    // Asks small knight who the fat knight is
    // which he responds with knight of life
    if(AskSmallKnight(FatKnight) == KnightOfLife)
    {
        // Then we ask the fat knight who he is
        // He responds that he is the knight of dungeon
        if(AskFatKnight(FatKnight) == KnightOfDungeon || AskFatKnight(FatKnight) == KnightOfDeath)
        {
            //  We know for sure that the fat knight is definitely not 
            //  the Knight of Life, because if he was the knight of life
            //  Indicated by the small knight he would've said he is the
            //  knight of life. (Always told the truth)

            //  We can also determine that small knight is definitely not
            //  the Knight of life because he would've told the truth when 
            //  asked who the fat knight is.

            //  Therefore, by the process of elimination, the tall knight is 
            //  the knight of life
            return TallKnight;

        }
    }
}

Knight GetKnightOfDeath()
{
    // First we determine who the Knight of Life is
    Knight KnightOfLife = GetKnightOfLife();

    // Since the knight of life always tell the truth
    // He will tell us who one of the remaining two knight 
    // is.
    if(KnightOfLife.Ask(FatKnight) == KnightOfDeath)
        return FatKnight;
    if(KnightOfLife.Ask(SmallKnight) == KnightOfDeath)
        return SmallKnight;
}

/**********************************************************
Description: Class constructor

Progarmmer: Jerry Jia

Parameters: none

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
Client::Client()
{
    // Initialize the circular buffer
    CBInitialize(&CBPackets, MAXPACKETS, PACKETLEN);

    // allocate space for the one packet of data
    // that is being exposed to unity
    currentData = (char *)malloc(PACKETLEN);
}

/**********************************************************
Description: Initialize the socket address structure by recieving the port number and
either the hostname or an IPV4 address.

Progarmmer: Jerry Jia

Parameters:
    hostname - The host name of the server.
    hostport - The port number of the server.

Returns: Socket file descriptor and the server address for future use.
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
int Client::Init_SockAddr(const char* hostname, short hostport)
{
    //address that client socket should connect to
    struct sockaddr_in addr;
    struct hostent* host;

    //set up port and protocol of address structure
    memset(&addr, 0, sizeof(addr));
    addr.sin_family      = AF_INET;
    addr.sin_port        = htons(hostport);

    if((host = gethostbyname(hostname)) == 0)
    {
        std::cout << "Error in gethostbyname" << std::endl;
        return 1;
    }

    memcpy(&addr.sin_addr, host->h_addr, host->h_length);
    //Assign sockaddr_in variable to Client class variable
    serverAddr = addr;
    return 0;
}

/**********************************************************
Description: Wrapper function to report errors.

Programmer: Jerry jia

Parameters:
    error - The error information to report.

Returns: void

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
void Client::fatal(const char* error)
{
    perror(error);
}

/**********************************************************
Description: Get data from the client's circular buffer.

Programmer: Jerry jia, Dylan Blake

Parameters: none

Returns: Character pointer of the data removed from the circular buffer.

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
char* Client::GetData()
{
    if (CBPackets.Count != 0)
    {
        memset(currentData, 0, PACKETLEN);
        CBPop(&CBPackets, currentData);
    }
    else
    {
        strcpy(currentData, "[]");
    }
    return currentData;
}
