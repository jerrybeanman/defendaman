
# Makefile
# Jerry Jia

CC= g++
NAME= Server
FLAGS= -std=c++11 -Wall -ggdb
LIBRARIES = -pthread
S_FILES= Server.cpp ServerTCP.cpp ServerUDP.cpp main.cpp ../Unity_Plugin/json11.cpp
OBJECT_FILES=$(S_FILES:.cpp=.o)
META_FILES=$(S_FILES:.cpp=.cpp.meta)


all:
		$(CC) $(FLAGS) $(LIBRARIES) -o $(NAME) $(S_FILES)

debug:
		$(CC) $(FLAGS) -g -o $(NAME) $(OBJECT_FILES)
clean:
		rm $(OBJECT_FILES) $(NAME) $(META_FILES) Makefile.meta

remake:
		make clean
		clear
		make
