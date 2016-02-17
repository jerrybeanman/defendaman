#include "plugin_engine.h"

int data = 0;

extern "C" char * receiveData() {
	char result[16]; 
	sprintf(result, "%d", data);
	return result;
}


extern "C" void sendData(const char *) {
	data++;
}