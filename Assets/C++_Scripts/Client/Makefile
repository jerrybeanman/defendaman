CC = g++ -Wall -pedantic -W

Client:	Client.o main.o
	$(CC) Client.o main.o -o Client

clean:
	rm -f *.o core.* Client

Client.o: Client.cpp Client.h
	$(CC) -c Client.cpp

main.o:	main.cpp
	$(CC) -c main.cpp
