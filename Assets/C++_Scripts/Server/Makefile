
# Makefile
# Jerry Jia

CC=g++
NAME=Server
FLAGS=-Wall
LIBRARIES = -pthread
S_FILES= Server.cpp ServerTCP.cpp main.cpp
OBJECT_FILES=$(S_FILES:.cpp=.o)
META_FILES=$(S_FILES:.cpp=.cpp.meta)


$(NAME): 	$(OBJECT_FILES)
		$(CC) $(LIBRARIES) $(FLAGS) -o $(NAME) $(OBJECT_FILES)

debug:
		$(CC) $(FLAGS) -g -o $(NAME) $(OBJECT_FILES)
clean:
		rm $(OBJECT_FILES) $(NAME) $(META_FILES) Makefile.meta

remake:
		make cleanclera
		clear
		make
