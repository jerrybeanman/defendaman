CC = g++  -std=c++11 -Wall -pedantic -W -ggdb

make:
all: LobbyClient GameClient EchoServer


GameClient:	main.o Client.o CircularBuffer.o GameClient.o
	$(CC) main.o Client.o CircularBuffer.o GameClient.o -o GameClient

LobbyClient:	main.o Client.o CircularBuffer.o LobbyClient.o
	$(CC) main.o Client.o CircularBuffer.o LobbyClient.o -o LobbyClient

CircularBuffer.o:	CircularBuffer.cpp CircularBuffer.h
	$(CC) -c -g CircularBuffer.cpp

EchoServer:
	$(CC) -o EchoServer udp_echo_svr.c

clean:
	rm -f *.o core.* Game GameClient LobbyClient EchoServer

Client.o: Client.cpp Client.h
	$(CC) -c -g Client.cpp

GameClient.o: GameClient.cpp GameClient.h
	$(CC) -c -g GameClient.cpp

LobbyClient.o: LobbyClient.cpp LobbyClient.h
	$(CC) -c -g LobbyClient.cpp

game.o:	game.cpp
	$(CC) -c -g game.cpp

main.o: main.cpp
	$(CC) -c -g main.cpp
