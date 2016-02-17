/**
Simple game using UDP messages that moves a player depending on the
received message from the Server. The Server used in this game is a 
simple UDP echo server.

Note: (Tyler Trepanier) Need to test if non-blocking sockets take a
portion of the data or the entire datagram.

@author Tom Tang
*/

#include <string>
#include <sys/types.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <iostream>
#include <unistd.h>
#include "Client.h"
#include <stdio.h>
void render(char move);

//change to real port num for the server
#define PORT_NUM 8000
#define MAXLEN   100

//Game Data example structure that can be used.
typedef struct {
    uint32_t x;
    uint32_t y;
    char data[100];
} game_data;

int main(int argc, char *argv[])
{
    int i = 0;
    int socketDes;
    struct sockaddr server;
    game_data data = {htonl(1), htonl(2), "Hello Server"};
    char rbuf[8000];
    socklen_t server_len;

    if(argc != 2)
    {
        std::cerr << "Usage: [ip]" << std::endl;
        return 1;
    }
    Networking::Client c;
    std::pair<int, struct sockaddr> socketInfo = c.Init_UDP_Client_Socket(argv[1], PORT_NUM);
    socketDes = socketInfo.first;
    server = socketInfo.second;

    server_len = sizeof(server);

    //game part of the code
    char get = 0;

    system("stty raw igncr -echo");
    system("clear");

    char play;
    system("setterm -cursor off");
    
    /*
    Parent process deals with receiving data from the server and rendering
    the playing field.

    Child process deals with sending data to the server.
    */
    if(fork() == 0){
      while(1){
        get = getchar();

        char send[2] = {get,'\0'};
        c.sendUDP(socketInfo.first, send ,socketInfo.second);
        if(get == 'l'){
          break;
        }
      }
    }else{
      while(1){
        printf("\033[1;1H");
        render(play);
        printf("\033[1;1H");

        char receive[2] = {'z','\0'};

        /*
        This line below receives the UDP message from the server.
        This is a non-blocking socket so it can immediate jump out
        of recvfrom which is normally a blocking call.
        */
        c.receiveUDP(socketInfo.first,&socketInfo.second, receive);
        play = receive[0];
        
        if(play == 'l'){
          break;
        }
      }
    }

    /* One process example of sending/receiving. Result is choppy.
    while(1){

      render(play);

      get = getchar();
      char send[2] = {get,'\0'};
      c.sendUDP(socketInfo.first, send ,socketInfo.second);
      char receive[2] = {'z','\0'};
      c.receiveUDP(socketInfo.first,&socketInfo.second, receive);

      play = receive[0];
      if(play == 'l'){
        break;
      }
    } */

    system("stty cooked -igncr echo");
    system("setterm -cursor on");
    system("clear");

}

/**************************************************************************
Takes in a players keystroke and moves it along the grid depending if it's
WASD.

W = up
A = left
S = down
D = right

@author Tom Tang
***************************************************************************/
void render(char move){
  int GRIDSIZE = 9;
  static int x = 0;
  static int y = 0;
  static char guy = 'x';
  
  // Parse the movement command
  if(move == 'w'){
    y--;
  }else if(move == 'a'){
    x--;
  }else if(move == 's'){
    y++;
  }else if(move == 'd'){
    x++;
  }
  

  y=y % GRIDSIZE;
  x=x % GRIDSIZE;
  if(x<0){
    x = x + GRIDSIZE;
  }
  if(y<0){
    y = y + GRIDSIZE;
  }
  
  size_t spoty;
  size_t spotx;

  printf("\033[10;1H");

  // This section populates the terminal with the dots
  for(spoty = 0 ; spoty<y ;spoty++){
    printf("          .........\n\r");
  }
  printf("          ");
  for(spotx = 0; spotx<x; spotx++ ){
    printf(".");
  }

  printf("%c", guy);
  spotx = spotx +1;

  for(; spotx<GRIDSIZE; spotx++ ){
    printf(".");
  }
  printf("\n\r");
  spoty = spoty + 1;
  for(;spoty < GRIDSIZE;spoty++){
    printf("          .........\n\r");
  }

}
