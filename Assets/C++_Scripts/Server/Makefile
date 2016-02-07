CC = g++ -Wall -pedantic -W

Client:	game.o Client.o
	$(CC) Client.o -g game.o -o Game

clean:
	rm -f *.o core.* Game

Client.o: Client.cpp Client.h
	$(CC) -c -g Client.cpp

game.o:	game.cpp
	$(CC) -c -g game.cpp
