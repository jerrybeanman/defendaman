Compile as a Linux .so library:
	g++ *.cpp -O3 -fPIC -shared -o MySharedLibrary.so

[Place the library .so file under Assets/Plugins folder]

To run the game:
	in terminal type:
		./run.sh 	[Note: change the name of the build in run.sh to the name of the game]
