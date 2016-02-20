#include "PluginSource.h"

int x = 0;
int y = 0;
char data[200] = "Failed To Update";

using namespace json11;

char * readFromTCP() {
	return "";
}

char * readFromUDP() {
	sprintf(data, "{DataType : 1, ID : 1, x : %d , y : %d}", x, y);
	return data;
}

extern "C" char * receiveData() {
	char tcp[64] = readFromTCP();
	char udp[100] = readFromUDP();
	if (strlen(tcp) > 1))
		sprintf(data, "[%s,%s]", readFromTCP, readFromUDP);
	else
		sprintf(data, "[%s]", reaedFromUDP);
	return data;
}

extern "C" void sendData(const char * message) {
	if (message[7] == '1') {
		x++;
		y++;
	}

	/*Json json(message);

	if (json[UP].string_value() == "1")
		++y;

	if (json[DOWN].int_value() == 1)
		--y;

	if (json[LEFT].bool_value() == true)
		--x;

	if (json[RIGHT].string_value() == "1")
		++x;*/
}
