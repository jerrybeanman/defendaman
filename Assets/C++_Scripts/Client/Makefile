CC = g++  -std=c++11 -Wall -pedantic -W -ggdb

make:
all:
	library


library:
	$(CC) client_library.cpp Client.cpp CircularBuffer.cpp LobbyClient.cpp -O3 -fPIC -shared -o ClientLibrary.so
	mv ClientLibrary.so ../../../

clean:
	rm ClientLibrary.so
	rm ../../../ClientLibrary.so
