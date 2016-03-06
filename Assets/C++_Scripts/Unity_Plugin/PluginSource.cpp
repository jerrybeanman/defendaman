#include "PluginSource.h"

using namespace json11;

extern std::queue<std::string> jsonPackets;

extern "C" char * receiveData() {
	if (!jsonPackets.empty()) {
		std::string nextPacket = jsonPackets.front();
		jsonPackets.pop();
		return nextPacket.c_str();
	} else {
		return "[]";
	}
}

extern "C" void sendData(const char * message) {
	
	//Send 
}
