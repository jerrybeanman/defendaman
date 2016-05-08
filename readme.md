# DefendAman

DefendAman is a multiplayer, top-down, 2D, team-based MOBA action game developed for Linux using Unity. The premise of the game is simple. There are multiple teams of players and one player on each team is the "Aman". Players must work with their team to simultaneously defend their Aman while also trying to take out the opposing team's Aman. This classic game of "VIP" is modernised with class selection, character progression, resource gathering, base buildings, and chat system. DefendAman combines fast-paced action with teamwide tactics for an exciting experience.

##Authors:
* **Jerry Jia** - [jerrybeanman](https://github.com/jerrybeanman)
* **Dylan Blake** - [dmblake](https://github.com/dmblake)
* **Carson Roscoe** - [CarsonRoscoe](https://github.com/CarsonRoscoe)
* **Alvin Man** - [AlvinMan](https://github.com/alvinman)
* **Jaegar Sarauer** - [JaegarSarauer](https://github.com/JaegarSarauer)
* **Allen Tsang** - [AllenTsang](https://github.com/AllenTsang)
* **Joseph Tam** - [JosephTam](https://github.com/josephtam)
* **Krystle Bulalakaw** - [krstlb](https://github.com/krstlb)
* **Hank Lo** - [Aify](https://github.com/Aify)
* **Spenser Lee** - [SpenserL](https://github.com/SpenserL)
* **Micah Willems** - [MicahWillems](https://github.com/micahwillems)
* **Tyler Trepanier** - [Tmanthegamer](https://github.com/Tmanthegamer)
* **Moon Eunwon** - [Imoongom](https://github.com/imoongom)
* **Dhivya Manohar** - [Dhivmanohar](https://github.com/dhivmanohar)
* **Colin Bose** - [ColinBose](https://github.com/ColinBose)
* **Oscar Kwan** - [Okwan](https://github.com/okwan)
* **Gabriella Chueng** - [Gabriellabcit](https://github.com/gabriellabcit)
* **Thomas Yu** - [Thomasyu93](https://github.com/thomasyu93)
* **Gabriel Lee** - [ScrawmySquirrle](https://github.com/ScrawnySquirrel)
* **Tom Tang** - [Eigenket1](https://github.com/eigenket1)
* **Vivek Kalia** - [Vivekkalia](https://github.com/vivekkalia)
* **Scott Plummer** - [Frozenhawk](https://github.com/Frozenhawk)

##Introduction:

###Matchmaking:
A match of DefendAman is played with two teams. When a player connects to a server hosting a DefendAman game, they can choose which side they want to play on. Teams are made up of at least two players and a game must have minimum of four players before it will begin. At the start of the match, each team can have one only one player to be the Aman. Gameplay revolves around managing both defense and offense; players have to ensure their Aman is safe before striking at the opposing team.
	
	`[TODO:: Insert lobby system screenshot here]`

###Classes:
Each player can take on one of three roles: **Ninja**, **Gunner** or **Wizard**. Each class has a distinct play style that provides a unique benefit to a team. 
- **Ninja**
	- Ninjas are the close-quarter combat specialists with more health and the special ability to teleport to quickly zone in and out of combat.
	
		`[TODO:: Insert Ninja special ability screenshot here]`


- **Gunner**
	- Nimble ranged attackers.
		- Normal attack: Fire out a stream of regular bullets
		- Special Ability: Fire a long ranged explosive laser attack.
	
		`[TODO:: Insert Gunner special ability screenshot here]`
- **Wizard**
	- Wizards are the spell-based heavy hitters.
		- Normal attack: Cast fireballs that applies a DOT(damage over time) to the target.
		- Special Ability:  Cast a large ice AOE(area of effect) that limits enemy movement and also buff ally's statuses.
	
		`[TODO:: Insert Wizard special ability screenshot here]`

- **Aman**
	- The player chosen to be the Aman receives some special abilities in addition to their chosen role. These unique abilities are essential to ensuring that a team is successful. 
	- The "Aman" player will also have a special glow around its character to indicate that he/she is the "Aman"
	
		`*[TODO:: Insert Aman player screen shot here]*`

###Buildings:
DefendAman also features an aspect of base building and resource management. Scattered throughout the playing field are various objects that provide an essential resource. When a player destroys one of these objects, they can collect the resource that falls out. That resource is used to build walls and provide upgrades for their team. Creating defensive fortifications is an essential way for a team to defend their Aman.

		`[TODO:: Insert player destroying tree screenshot here]`

Currently, there are five different types of buildings player can purchase using the ingame currency. 

		`[TODO:: Insert shop panel buliding screenshot here here]`
		
		`[TODO:: Insert shop panel upgrade screenshot here]`

- **Wall**
	- A very generic wall that blocks enemy vision and also prevents ninja from teleporting through.
	
		`[TODO:: Insert wall screenshot here]`


- **Watch Tower**
	- A rotating watch tower that grants vision to ally team members, which strengthens map awareness and control
	
		`[TODO:: Insert watch tower screenshot here]`

- **Turret**
	- A simple turret that annihilates anyone who comes across its FOV (field of visoin)
	
		`[TODO::turret screenshot here]`

- **Upgrade buliding**
	- Once it is placed, players can gain access to upgrading their firearms which allows more damage output.
	
		`*[TODO:: Insert upgrade buliding screen shot here]*`
			
		`*[TODO:: Insert upgrade buliding shopt panel screen shot here]*`

- **Potion buliding**
	- Once it is placed, players can gain access to puchasing various types of potions which grants more survivability and manuverability.
	
		`*[TODO:: Insert potion buliding screen shot here]*`
			
		`*[TODO:: Insert potion buliding shopt panel screen shot here]*`

##How-To:

###Compile:
- **Notes:**
	- Pre-existing debug and release game build, server executable, client c++ library, 
	  and map generator library can be found available under:
		- **DefendAman/Builds/Debug** 
		- **DefendAman/Builds/Release**
	- **Since the latest version of Unity has compatibility issues with linux 
		builds, we would need to roll back to a slightly older version of
		Unity.**
		- Download Unity version 5.2.4 at: https://unity3d.com/get-unity/download/archive
			- Select version 5.2.4, either Unity installer or the actual editor will work

- **Open project:**
	- Once the Unity editor is downloaded on a Windows machine, open the executable.
	- On the projects selection menu, click "Open"
	- Browse to the DefendAman project, and select the folder to open it.

- **Build game executable:**
	- Make sure the project is correctly loaded in the Unity editor
	- Open build settings window through File->Build Settings on the top left corner
	- Make sure all scenes are properly loaded in the "Scenes in Build" window
		- If nothing is displayed in the window:
			- Open **grass_theme**, **tron_theme**, and MenuScene by double clicking them individually
			- On the build settings, add each scenes to the build by clicking on add current
	- Select target platform to be Linux under Build Settings window
		- *Note: Might take a while for the editor to switch from Windows to Linux*
	- Select the corresponding architecture to run on. (x86_64 for the lab computers)
	- Click on build to compile the executable along with the data files into a selected folder.


###Run executable on Linux:
- **Notes:**
	- To run the executable on a Linux machine, the following folder and files NEEDS to be 
		under the same directory
		- **Executable file** 		*(i.e DefendAman_release.x86_64)*
		- **Executable data folder** 	*(i.e DefendAman_release_Data)*
		- **ClientLibrary.so** 		*(For C++ networking)*
		- **MapGenerationLibrary.so** 	*(For C++ random map generation)*

- **Run game executable:**
	- Give game executable file execution permission.
	- Double click on executable or run it through terminal, which will open the player selection
	  window for configuring resolution and game quality.
	- **IMPORTANT: Resolution setting needs to match the monitor, which is 16x9 for lab computers, 
	   and quality has to be simplest.** 
		- If an incorrect resolution and quality is selected, various issues will arise due 
		  to the compatability of Unity and Linux
	 



