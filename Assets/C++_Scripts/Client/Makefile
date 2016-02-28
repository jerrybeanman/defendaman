CC = g++ -Wall -pedantic -W -ggdb

make:
all:
	library


library:
	g++ client_library.cpp Client.cpp CircularBuffer.cpp  -O3 -fPIC -shared -o ClientLibrary.so
	mv ClientLibrary.so ../../../

clean:
	rm ClientLibrary.so
	rm ../../../ClientLibrary.so
