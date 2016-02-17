CC = g++ -Wall -pedantic -W -pthread

Server:	Server.o main.o
	$(CC) Server.o main.o -o Server

clean:
	rm -f *.o core.* Server

Server.o: Server.cpp Server.h
	$(CC) -c Server.cpp

main.o:	main.cpp
	$(CC) -c main.cpp
