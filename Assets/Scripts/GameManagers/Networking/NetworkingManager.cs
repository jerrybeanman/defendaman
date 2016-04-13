using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.UI;

/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    NetworkingManager.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--
--  DATE:           January 20th, 2016
--
--  REVISIONS:      February 5th, 2016: Added linking to C++ libraries
--                  March 10th, 2016: Refactored GameManager logic out into its own class
--                  March 20th, 2016: Refactored to singleton patern
--
--  DESIGNERS:      Carson Roscoe
--
--  PROGRAMMER:     Carson Roscoe
--
--  NOTES:
--  This class is a singleton class used to handle the communication between the C++
--  networking code and the C# game code. 
--  The class has 2 uses:
--    * To send/receive data from the Networking Team's clientside code, and
--    * Notifying subscribed objects when new data is updated
--
--  To subscribe for an objects updates from server, you would call the public Subscribe method.
--  This method takes in three things:
--    * Callback method, which is a void method that takes in a JSONClass as a parameter
--    * DataType you want to receive, e.g. DataType.Player for data of a player
--    * int ID of which of the DataType you want to receive info from, e.g. ID 1 on DataType.Player is Player 1's data
--
--  e.g. NetworkingManager.Subscribe((JSONClass [json]) => {Debug.Log("Got Player 1's Data");}, DataType.Player, 1);
---------------------------------------------------------------------------------------*/
public class NetworkingManager : MonoBehaviour {
    #region Variables

    //Holds the subscriber data
    private static Dictionary<Pair<DataType, int>, List<Action<JSONClass>>> _subscribedActions = new Dictionary<Pair<DataType, int>, List<Action<JSONClass>>>();

    //List of JSON strings to be sent on the next available TCP packet 
    private static List<string> jsonTCPObjectsToSend = new List<string>();

    //List of JSON strings to be sent on the next available UDP packet
    private static List<string> jsonUDPObjectsToSend = new List<string>();
    
    //Pointer to the C++ TCPClient objects' address stored in the heap of the unmanaged code
    public static IntPtr TCPClient { get; private set; }
    
    //Pointer to the C++ UDPClient objects' address stored in the heap of the unmanaged code
    public static IntPtr UDPClient { get; private set; }

    //Static reference to the instantiated version of this class, used for singleton patern
	public static NetworkingManager instance;

    //Used to store last packet received, either TCP or UDP
    private string result;

    //Flag used to denote whether this frame will send a packet or not. Alternates true and false every frame
    //so we send 30 packets a second instead of 60, since 60 was an uneeded amount as our eyes can only notice
    //up to 24 frames per second
    bool skip = false;

    #endregion

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Awake
    --
    -- DATE: March 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Awake(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Awake is invoked before Start in the creation of a script. In this case, it is used to initiate our singleton pattern
    -- by setting instance if it is not set, and if it is set destroying this script since we do not want two singletons
    -- of the same object.
    ---------------------------------------------------------------------------------------------------------------------*/
    void Awake() {
        //Check if instance already exists
        if (instance == null)
            //if not, set instance to this
            instance = this;
        //If instance already exists and it's not this:
        else if (instance != this)
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 
            Destroy(gameObject);
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);		
	}

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Start
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Start is called after Awake during the instantiation of all Unity game objects. We used it here to see if we
    -- have this script attached to GameManager or NetworkingServer. Only one should exist at any given momment, and
    -- if both exist then that means are testing on Windows via skipping the menu and need to handle it as such.
    -- 
    -- Here is also where we call the TCP_CreateClient function linked to the C++ networking code and return its pointer
    -- address for storing.
    ---------------------------------------------------------------------------------------------------------------------*/
    void Start() {
        //If the game has not started
		if (!GameData.GameStart) {
            //Find GameManager singleton
            var gameManager = GameObject.Find("GameManager") as GameObject;
            //Find NetworkingServer singleton
            var networkManager = GameObject.Find("NetworkingServer") as GameObject;
            //If they both exist, we are in a impossible state only achieved through testing on Windows, delete one of them
            if (gameManager != null && networkManager != null) {
                Destroy(GameManager.instance);
                Destroy(gameManager);
            }
            //Try to create a TCPClient object. This can only be done on Linux as the networking library is Linux only
			try {
				TCPClient = TCP_CreateClient();
			} catch (Exception) {
				//On Windows, don't worry about it. This will only ever fail on Windows.
			}
		}
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: ResetConnections
    --
    -- DATE: April 3rd, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void ResetConnections(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- wrapper around the C++ cleanup code for both the TCP and UDP connections. We pass the reference to our TCPClient
    -- and UDPClient objects into the appropriate cleanup call and invoke them both. This is only called after successfully
    -- completing a game and returning to the main menu.
    ---------------------------------------------------------------------------------------------------------------------*/
    public void ResetConnections() {
        //Call C++ cleanup TCP client function passing in a pointer to our TCPClient object
        TCP_DisposeClient(TCPClient); 
        //Call C++ cleanup UDP client function passing in a pointer to our UDPClient object
        UDP_DisposeClient(UDPClient);
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Update
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Update(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- This method is invoked 60 frames per second in our game.
    -- We use this method in NetworkingManager to handle receiving the stored TCP & UDP packets from the C++ code
    -- and passing it over to our update_data method calls for handling updating our world.
    -- After which, we invoke our send_data method, which will send out our current data to the server for echo'ing.
    ---------------------------------------------------------------------------------------------------------------------*/
    void Update() {
        //If we are in a game (not in the lobby)
        if (GameData.GameStart) {
            //Alternating the skip flag so we only invoke the logic of this call 30 times per second instead of 60
            if (skip) {
                skip = false;
                return;
            }
            skip = true;

            string packet;

            //While there is TCP data stored in a circular buffer, get it and feed it to update_data
            while ((packet = receive_data_tcp()).Length > 4)
                update_data(packet);

            //While there is UDP data stored in a circular buffer, get it and feed it to update_data
            while ((packet = receive_data_udp()).Length > 4)
                update_data(packet);
            
            //Send our TCP and UDP data for this frame
            send_data();
        }
    }

    ////Code for subscribing for updates from client-server system////
    #region SubscriberSystem

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Subscribe
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Subscribe(Action<JSONClass> callback function pointer or lambda for when an update is received
    --                           DataType dataType of ID to subscribe for (e.g. Kill update, or Environment update
    --                           (optional) int id of the update you want. e.g. DataType.Player ID 1 is updates for Player 1)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- To subscribe for an objects updates from server, you would call the public Subscribe method.
    -- This method takes in three things:
    --   * Callback method, which is a void method that takes in a JSONClass as a parameter
    --   * DataType you want to receive, e.g. DataType.Player for data of a player
    --   * int ID of which of the DataType you want to receive info from, e.g. ID 1 on DataType.Player is Player 1's data
    --
    -- e.g. NetworkingManager.Subscribe((JSONClass json) => {Debug.Log("Got Player 1's Data");}, DataType.Player, 1);
    ---------------------------------------------------------------------------------------------------------------------*/
    public static void Subscribe(Action<JSONClass> callback, DataType dataType, int id = 0) {
        //The DataType/ID combination that is used as the "Key" in our hashmap of update callbacks by DataType/ID combination
        Pair<DataType, int> pair = new Pair<DataType, int>(dataType, id);

        //If we have not seen this pair in the dictioanry yet, add it along with a list of callbacks
        if (!(_subscribedActions.ContainsKey(pair))) {
            _subscribedActions.Add(pair, new List<Action<JSONClass>>());
        }

        //Add our callback to the list of entries under that pair of datatype and ID.
        _subscribedActions[pair].Add(callback);
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Unsubscribe
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Unsubscribe(DataType dataType of ID to unsubscribe for (e.g. Kill update, or Environment update
    --                            (optional) int id of the update you want. e.g. DataType.Player ID 1 is updates for Player 1)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- This method is used to unsubscribe for updates. It will remove ALL callback functions from the subscription dictionary.
    -- This is used when we no longer want updates for a particular DataType/ID combination. An example would be that,
    -- if player 1 died, we no longer want to listen for movement updates for that player, so we invoke this method with
    -- DataType.Player set and id set to 1.
    ---------------------------------------------------------------------------------------------------------------------*/
    public static void Unsubscribe(DataType dataType, int id = 0) {
        //DataType/ID pair turned into the key for our dictionary
        Pair<DataType, int> pair = new Pair<DataType, int>(dataType, id);
        //If we have this key in the dictionary, remove it.
        if (_subscribedActions.ContainsKey(pair)) {
            _subscribedActions.Remove(pair);
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: ClearSubscriptions
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void ClearSubscriptions()
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- This method is used when we want to clear ALL subscriptions for updated data. This is used when we are reseting
    -- the game, for example.
    ---------------------------------------------------------------------------------------------------------------------*/
    public static void ClearSubscriptions() {
        _subscribedActions.Clear();
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: update_data
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void update_data(string of the packet received from either the TCP or UDP client code)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- This method is invoked whenever we receive any UDP or TCP packet. All packets are a JSON array of objects, where
    -- each object contains a DataType/ID combination used to denote what this packet is meant to update. This method
    -- takes our JSON array and foreach's through the JSON objects. It then determines what DataType/ID pair each JSON
    -- object is meant to update, and then invokes the appropriate callbacks to pass this information into.
    --
    -- Effectively, a packet may look like so:
    --   [{DataType : 1, ID : 1, x : 50, y : 60},{DataType : 1, ID : 2, x : 70, y : 90}]
    -- which will be split into two different JSON objects (one of DataType 1 ID 1, the other of DataType 1 ID 2).
    -- After being split it would go "DataType 1 is Player data, ID 1 is for player 1. Player 1's x and y is not 50,60"
    -- and do the same logic for the second JSON object as well.
    ---------------------------------------------------------------------------------------------------------------------*/
    public void update_data(string JSONGameState) {
        JSONArray gameObjects = null;
        try {
            //Attempt to parse our packet into a JSON array. If it fails, this will throw an exception.
            gameObjects = gameObjects = JSON.Parse(JSONGameState).AsArray;
        } catch (Exception e) {
            //On failing to parse the data, log the packet so we can see what went wrong
            Debug.Log("[ERROR: Invalid packet: " + JSONGameState);
        }

        //If it managed to be parsed but the data was empty, display the error and return
        if (gameObjects == null || JSONGameState == "[]") {
            Debug.Log("[ERROR] NetworkingManager.update_data received a packet that is null");
            return;
        }

        //If the data was confirmed to be a non-empty JSON array, foreach through it
        foreach (var node in gameObjects.Children) {
            var obj = node.AsObject;
            //Get the DataType valie from the object
            int dataType = obj["DataType"].AsInt;
            //Get the ID from the object
            int id = obj["ID"].AsInt;

            //If the DataType is environment data, hand it to them directly as the environment team parses their own data differently
            if (dataType == (int)DataType.Environment) {
                GameObject.FindGameObjectWithTag("GameManager").GetComponent<MapManager>().HandleEvent(id, obj);
            }

            //ID being 0 is an error as ID's start at 1, with the exception of Environment data and StartGame data.
            //If we have a valid DataType/ID amounts
            if (id != 0 || (dataType == (int)DataType.Environment || dataType == (int)DataType.StartGame)) {
                //Create out DataType/ID pair for checking for appropriate callbacks
                Pair<DataType, int> pair = new Pair<DataType, int>((DataType)dataType, id);
                //If our dictionary fo subscribed actions contains our DataType/ID combination
                if (_subscribedActions.ContainsKey(pair)) {
                    //Foreach through all of the callbacks and invoke them all by passing in this JSON object of data
                    foreach (Action<JSONClass> callback in _subscribedActions[pair]) {
                        callback(obj);
                    }
                }
            }
        }
    }

    #endregion

    ////Code for communicating with client-server system////
    #region CommunicationWithClientSystem
    //Hookup the C++ library's TCP_CreateClient function to be usable in C#
    [DllImport("ClientLibrary.so")]
    public static extern IntPtr TCP_CreateClient();

    //Hookup the C++ library's TCP_Dispose function to be usable in C#
    [DllImport("ClientLibrary.so")]
	public static extern void TCP_DisposeClient(IntPtr client);
    //Wraper for our TCP_DisposeClient method so outside objects don't need to access our client pointer
	public static void TCP_DisposeClient(){
		TCP_DisposeClient(TCPClient);
	}

    //Hookup the C++ library's TCP_ConnectToServer function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern int TCP_ConnectToServer(IntPtr client, string ipAddress, short port);
    //Wrapper for our TCP_ConnectToServer function so outside objects don't need to access our client pointer
    public static int TCP_ConnectToServer(string ipAddress, short port) {
        return TCP_ConnectToServer(TCPClient, ipAddress, port);
    }

    //Hookup the C++ library's TCP_Send function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern int TCP_Send(IntPtr client, string message, int size);
    //Wrapper for our TCP_Send function so outside objects don't need to access our client pointer
    public static int TCP_Send(string message, int size) {
        return TCP_Send(TCPClient, message, size);
    }

    //Hookup the C++ library's TCP_GetData function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern IntPtr TCP_GetData(IntPtr client);
    //Wrapper for our TCP_GetData function so outside objects don't need to access our client pointer
    public static IntPtr TCP_GetData() {
        return TCP_GetData(TCPClient);
    }

    //Hookup the C++ library's TCP_StartReadThread function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern int TCP_StartReadThread(IntPtr client);
    //Wrapper for our TCP_StartReadThread function so outside objects don't need to access our client pointer
    public static int TCP_StartReadThread() {
        return TCP_StartReadThread(TCPClient);
    }

    //Hookup the C++ library's UDP_CreateClient function to be usable in C#
    [DllImport("ClientLibrary.so")]
    public static extern IntPtr UDP_CreateClient();

    //Hookup the C++ library's UDP_DisposeClient function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern void UDP_DisposeClient(IntPtr client);
    //Wrapper for our UDP_DisposeClient function so outside objects don't need to access our client pointer
    public static void UDP_DisposeClient() {
        UDP_DisposeClient(UDPClient);
    }

    //Hookup the C++ library's UDP_ConnectToServer function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern int UDP_ConnectToServer(IntPtr client, string ipAddress, short port);
    //Wrapper for our UDP_ConnectToServer function so outside objects don't need to access our client pointer
    public static int UDP_ConnectToServer(string ipAddress, short port) {
        return UDP_ConnectToServer(UDPClient, ipAddress, port);
    }

    //Hookup the C++ library's UDP_Send function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern int UDP_Send(IntPtr client, string message, int size);
    //Wrapper for our UDP_SendData function so outside objects don't need to access our client pointer
    public static int UDP_SendData(string message, int size) {
        return UDP_Send(UDPClient, message, 512);
    }

    //Hookup the C++ library's UDP_GetData function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern IntPtr UDP_GetData(IntPtr client);
    //Wrapper for our UDP_GetData function so outside objects don't need to access our client pointer
    public static IntPtr UDP_GetData() {
        return UDP_GetData(UDPClient);
    }

    //Hookup the C++ library's UDP_StartReadThread function to be usable in C#
    [DllImport("ClientLibrary.so")]
    private static extern int UDP_StartReadThread(IntPtr client);
    //Wrapper for our UDP_StartReadThread function so outside objects don't need to access our client pointer
    public static int UDP_StartReadThread() {
        return UDP_StartReadThread(UDPClient);
    }

    //Hookup the C++ library's GenerateMap function to be usable in C#
    [DllImport("MapGenerationLibrary.so")]
    private static extern IntPtr GenerateMap(int seed);

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: GenerateMapInJSON
    --
    -- DATE: March 15th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: string GenerateMapInJSON(int seed to generate the world with)
    --
    -- RETURNS: string of the world map generated in JSON form
    --
    -- NOTES:
    -- Originally the server was going to generate the map and hand it over to the C# code, so the map generation was
    -- written in C++ and the C# code was built to parse this map data as a JSON object. After we tested, we realized
    -- the map generation was roughly 80,000 bytes on a 100x100 map. The fear of that packet being corrupted led us
    -- to move the generation code over to the client side and have the server send a single seed to all clients instead.
    --
    -- The logic stayed the same on handling the map, which is why we simply invoke the C++ function for generating the map
    -- and fake "receiving" the packet locally.
    ---------------------------------------------------------------------------------------------------------------------*/
    public static string GenerateMapInJSON(int seed) {
        //If we are on Linux, call the C++ function to generate the map
        if (Application.platform == RuntimePlatform.LinuxPlayer)
            return Marshal.PtrToStringAnsi(GenerateMap(seed));
        //Otherwise we are on windows, return a previously-generated one for testing purpose.
        else
            return "[{DataType:3,ID:0,\"mapWidth\":100,\"mapHeight\":100,\"mapIDs\":[[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,105,105,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,0,105,105,105,105,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,106,103,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,105,105,105,105,105,104,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,106,106,106,106,106,106,106,106,106,105,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,105,105,106,106,105,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,104,105,105,106,106,105,106,105,106,106,105,105,0,0,0,0,0,0,0,0,0,0,0,0,0,105,106,0,0,0,105,0,0,0,0,0,0,0,0,106,105,104,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,105,106,105,106,106,106,105,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,105,105,106,105,106,107,106,105,106,106,106,104,105,0,0,0,0,0,0,0,0,0,0,105,106,106,106,106,106,106,106,106,105,0,0,0,0,0,0,106,105,105,106,106,106,106,106,106,0,0,0,0,0,0,0,0,0,0,0,0,107,106,0,0,0,0],[0,0,0,106,106,105,106,105,105,105,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,105,105,105,106,106,106,106,106,106,107,106,106,106,106,106,0,0,0,0,0,0,0,0,106,105,105,106,106,106,105,106,105,106,105,0,0,0,0,0,105,105,105,105,106,106,106,105,106,106,106,106,0,0,0,0,0,0,0,0,0,107,107,106,105,0,0,0],[0,0,106,106,107,106,106,106,105,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,105,106,106,105,106,105,106,105,105,107,106,107,105,106,105,106,0,0,0,0,0,0,0,0,105,105,105,106,105,106,106,106,106,105,106,0,0,0,0,0,106,105,106,104,106,106,106,106,105,106,105,106,106,0,0,0,0,0,0,0,0,106,106,106,105,105,0,0],[0,0,106,105,106,105,106,106,106,105,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,106,106,106,106,105,105,106,105,105,106,106,107,106,107,106,106,106,0,0,0,0,0,0,0,106,104,104,105,106,106,106,106,106,106,105,0,0,0,0,0,105,106,106,105,105,106,106,105,106,106,106,106,106,106,0,0,0,0,0,0,0,106,106,106,105,106,0,0],[0,0,0,105,105,106,106,106,106,105,105,106,0,0,0,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,106,106,106,106,106,106,105,105,106,106,105,105,106,106,106,107,107,106,105,0,0,0,0,0,0,105,106,105,106,106,106,106,107,105,107,105,0,0,0,0,0,105,106,105,105,106,106,105,105,105,106,106,106,106,0,0,0,0,0,0,0,106,106,106,106,105,0,0],[0,0,0,105,106,105,106,106,106,106,106,105,105,106,106,104,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,105,107,106,106,106,106,106,106,106,105,105,104,105,104,106,106,106,107,106,107,106,0,0,0,0,0,105,105,106,106,106,106,106,106,106,106,106,105,0,0,0,0,105,106,106,106,106,105,105,106,106,106,106,105,106,106,0,0,0,0,0,0,105,106,106,106,0,0,0],[0,0,0,105,106,106,106,106,106,106,106,106,106,105,105,106,105,106,106,106,105,105,0,0,0,0,0,0,0,0,106,106,106,107,106,106,106,107,106,106,106,105,106,105,105,105,106,106,106,105,106,107,106,0,0,0,105,105,106,106,107,106,105,105,106,106,106,106,106,106,0,0,106,105,105,105,106,106,106,105,106,106,106,105,105,106,106,105,106,0,0,0,106,107,105,106,0,0,0,0],[0,0,0,0,106,107,106,105,106,106,106,106,106,105,105,105,105,106,106,106,104,106,106,0,0,0,0,0,0,106,107,106,106,105,105,106,106,107,106,106,106,106,105,105,106,106,106,106,106,106,106,105,106,106,106,107,106,106,106,106,105,105,105,105,106,106,105,105,105,105,106,104,105,106,106,104,106,105,105,105,105,105,106,105,105,106,106,105,106,106,107,107,107,106,106,106,0,0,0,0],[0,0,0,0,107,106,106,106,106,106,106,106,106,106,105,105,106,105,106,106,106,106,106,0,0,0,0,0,106,106,106,106,106,106,105,105,106,106,106,106,106,106,105,106,105,106,0,107,106,107,106,106,106,105,106,106,107,106,106,106,106,106,106,106,106,106,106,105,105,106,105,104,105,105,106,106,105,105,105,106,106,106,106,105,105,105,106,105,105,106,106,107,106,106,106,106,105,0,0,0],[0,0,0,0,106,106,106,106,106,107,106,106,106,107,106,105,105,105,106,106,106,106,0,0,0,0,0,0,105,105,106,106,105,105,105,105,106,106,106,107,107,106,105,106,106,0,0,0,106,106,106,106,106,107,106,105,106,106,107,106,106,106,106,106,106,106,106,105,106,106,106,106,105,106,104,105,105,105,105,106,106,106,106,105,104,105,105,106,105,107,105,106,106,106,106,105,105,105,0,0],[0,0,0,0,106,104,106,107,107,107,107,107,106,106,106,106,105,106,106,106,106,106,0,0,0,0,0,0,0,103,105,106,106,106,104,105,106,106,107,107,106,107,106,107,106,0,0,0,105,106,105,106,106,106,105,105,105,105,106,105,105,106,104,105,105,106,106,106,106,106,105,106,106,106,105,106,105,106,105,105,105,106,106,105,106,105,105,106,106,105,106,105,106,106,105,105,105,105,0,0],[0,0,0,0,104,106,106,107,107,107,107,107,107,106,106,105,105,106,106,105,105,0,0,0,0,0,0,0,0,0,106,106,106,106,105,105,105,106,106,107,106,106,106,105,105,0,0,0,0,106,106,106,105,105,106,106,104,107,104,106,104,105,104,105,105,106,106,106,106,106,106,106,106,106,106,106,106,105,105,106,106,105,106,105,106,106,106,106,105,106,106,106,105,107,107,106,105,105,0,0],[0,0,0,0,104,105,106,106,107,106,106,106,106,106,106,105,106,104,106,106,0,0,0,0,0,0,0,0,0,0,0,105,105,105,106,106,105,106,106,106,106,106,106,106,106,106,0,0,0,0,106,105,105,106,106,105,106,106,106,105,105,106,104,105,105,105,105,106,105,106,106,106,106,105,106,106,106,106,106,105,106,106,106,106,105,106,106,106,106,105,106,107,106,106,107,107,107,105,0,0],[0,0,0,106,104,105,105,106,107,106,106,106,106,107,106,105,105,107,106,107,0,0,0,0,0,0,0,0,0,0,0,0,106,105,105,106,106,105,105,106,106,106,107,106,106,106,0,0,0,0,106,105,105,106,104,105,106,106,106,0,0,0,105,105,106,106,105,106,106,106,106,106,107,106,106,106,106,106,106,106,106,106,106,106,106,105,0,0,0,106,107,107,105,105,106,106,106,106,0,0],[0,0,106,107,106,105,105,106,105,107,106,105,105,107,107,105,105,106,105,105,0,0,0,0,0,0,0,0,0,0,0,0,105,105,105,106,105,105,105,106,107,106,106,105,106,105,105,0,0,0,106,105,104,105,104,105,105,106,0,0,0,0,0,0,107,105,106,106,107,106,107,106,107,106,106,106,106,106,106,106,106,106,105,105,106,0,0,0,0,0,107,105,105,105,106,106,106,106,0,0],[0,0,106,105,105,106,105,105,105,107,105,104,106,106,106,106,105,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,106,104,105,105,105,106,105,106,106,105,106,105,106,106,105,0,0,0,106,105,105,105,106,106,105,0,0,0,0,0,0,0,0,106,106,106,106,107,107,107,107,106,106,106,106,105,106,106,106,106,105,105,0,0,0,0,0,0,106,105,105,106,106,105,106,106,0,0],[0,0,105,105,105,105,105,106,105,106,105,106,105,105,106,105,106,106,106,0,0,0,0,0,0,0,0,0,0,0,0,106,104,106,106,107,106,106,106,106,106,106,106,106,106,107,106,0,0,106,106,107,104,106,106,105,0,0,0,0,0,0,0,0,0,106,106,106,106,106,107,107,106,107,107,106,106,106,106,106,106,106,106,106,0,0,0,0,0,0,0,106,105,106,106,105,106,106,0,0],[0,0,105,106,106,105,105,105,106,106,105,104,105,105,106,106,106,106,106,107,0,0,0,0,0,0,0,0,0,0,0,107,105,103,106,106,106,107,106,106,106,106,107,106,106,106,106,106,106,105,106,106,106,106,106,107,0,0,0,0,0,0,0,0,106,105,106,106,106,106,106,107,106,106,106,107,107,106,106,107,106,106,106,106,0,0,0,0,0,0,0,106,107,106,105,106,106,106,0,0],[0,0,0,106,106,106,105,105,105,105,106,105,105,106,106,106,106,106,106,107,105,0,0,0,0,0,0,0,0,0,0,106,105,105,105,106,106,106,106,106,107,106,106,106,106,106,106,106,106,106,106,105,106,105,106,105,0,0,0,0,0,0,0,0,106,106,106,106,106,106,105,107,106,106,105,107,107,106,106,106,106,106,106,105,0,0,0,0,0,0,0,106,106,106,106,105,105,104,104,0],[0,0,0,0,106,105,105,106,106,106,105,105,105,105,106,105,106,106,106,106,106,107,0,0,0,0,0,0,0,0,0,106,106,105,105,104,107,106,106,105,106,106,105,106,104,105,106,106,107,104,105,106,106,105,106,106,0,0,0,0,0,0,0,106,106,106,106,106,105,106,104,105,105,106,106,107,107,106,106,106,106,105,106,106,105,0,0,0,0,0,0,106,106,106,106,106,106,105,106,0],[0,0,0,0,107,106,106,106,106,106,105,105,106,105,105,106,106,107,107,106,106,106,106,0,0,0,0,0,0,0,0,106,106,106,107,106,107,107,106,106,106,105,105,106,104,105,105,106,106,106,106,106,106,105,105,105,106,0,0,0,0,0,0,105,106,105,105,106,106,106,106,105,104,103,106,106,107,107,106,106,106,105,106,107,106,0,0,0,0,0,0,106,106,106,106,106,105,105,105,0],[0,0,0,0,106,105,107,105,106,106,106,107,105,105,106,106,106,106,106,106,105,105,106,105,0,0,0,0,0,0,0,107,106,107,106,107,107,106,105,105,106,105,105,105,105,106,105,106,105,107,106,106,106,106,105,106,106,106,0,0,0,0,0,0,105,106,106,106,105,106,106,105,104,105,107,106,106,107,107,106,106,106,104,106,106,0,0,0,0,0,0,104,106,107,106,106,106,105,105,0],[0,0,0,0,106,105,106,105,105,106,105,106,106,106,105,106,106,106,105,106,104,106,106,106,106,0,0,0,0,0,107,107,107,107,106,105,106,105,105,106,104,105,105,105,106,107,107,106,106,106,107,107,106,106,106,106,106,106,105,105,0,0,0,0,106,106,105,106,107,106,106,105,104,105,106,106,106,107,107,106,106,106,106,106,106,106,0,0,0,0,106,106,106,107,106,106,105,106,0,0],[0,0,0,0,106,106,106,106,106,107,106,106,103,104,106,106,105,106,104,105,106,106,105,105,106,0,0,0,0,106,106,107,106,106,107,106,106,105,106,105,106,105,106,106,107,106,105,105,105,106,105,106,106,106,106,106,105,105,104,105,106,0,0,106,106,106,106,106,107,106,106,105,106,105,106,105,105,106,106,106,106,106,106,105,106,107,106,0,0,106,106,106,106,107,107,106,105,106,0,0],[0,0,0,0,107,106,106,106,105,106,106,105,105,106,105,105,105,106,106,105,106,106,105,105,105,0,0,0,106,106,107,106,107,107,106,107,105,106,105,105,105,106,105,107,106,106,106,105,106,106,106,105,106,106,106,106,106,106,106,105,105,106,106,107,105,106,107,105,106,107,106,106,106,106,106,106,105,105,105,106,105,106,106,106,106,106,106,106,105,106,106,106,106,106,106,106,106,106,0,0],[0,0,0,0,0,107,105,104,106,106,105,106,105,105,106,106,106,106,106,105,107,107,105,105,106,0,0,0,106,106,106,106,106,106,106,106,107,106,105,105,105,107,107,107,106,106,106,105,106,105,105,106,106,106,106,106,105,105,105,105,106,106,106,107,107,107,106,106,106,106,106,106,106,106,106,105,105,106,105,105,106,105,106,106,106,107,106,105,106,105,106,106,107,106,106,106,106,106,0,0],[0,0,0,0,0,0,105,105,104,105,106,105,106,106,106,106,106,106,106,107,107,107,105,106,106,0,0,0,106,106,105,106,0,0,106,106,106,106,105,105,0,0,105,107,106,106,105,105,105,105,106,106,106,106,105,104,106,104,105,105,106,106,106,106,107,107,107,106,107,106,106,105,106,104,105,105,105,105,105,105,106,106,106,106,105,106,106,104,105,105,106,106,107,106,106,106,106,0,0,0],[0,0,0,0,0,0,105,106,104,105,106,106,107,106,106,106,106,106,106,107,106,106,106,105,105,0,0,0,0,105,106,0,0,0,0,106,106,105,105,0,0,0,0,105,106,106,106,105,106,106,105,106,106,106,106,106,106,105,105,106,105,106,106,105,105,106,105,106,105,106,106,106,107,104,106,107,106,105,106,106,106,106,106,106,105,106,106,106,106,106,106,106,106,106,105,106,105,0,0,0],[0,0,0,0,0,0,106,105,106,106,106,106,106,106,106,105,107,106,106,107,106,106,106,106,105,0,0,0,0,0,0,0,0,0,0,106,106,106,107,0,0,0,0,106,106,107,106,106,106,106,105,104,106,107,106,106,106,105,104,105,106,106,106,106,106,106,106,105,106,106,107,107,106,106,105,107,106,106,106,107,106,106,106,105,104,106,106,106,106,106,106,106,105,105,106,105,104,0,0,0],[0,0,0,0,0,0,106,106,106,106,105,105,106,106,106,106,106,107,106,107,107,106,106,106,105,105,0,0,0,0,0,0,0,0,0,105,104,106,106,107,0,0,106,105,106,106,106,105,106,0,0,0,106,107,106,106,104,105,105,106,106,106,106,107,106,105,106,105,105,106,106,106,106,106,107,107,107,107,107,107,107,106,105,105,105,105,106,105,106,106,106,106,104,105,105,106,106,105,0,0],[0,0,0,0,0,0,105,104,106,105,106,105,105,107,106,107,107,106,105,107,106,106,106,106,105,106,0,0,0,0,0,0,0,0,0,0,105,106,107,106,105,105,105,106,106,106,106,0,0,0,0,0,0,0,0,106,106,106,106,106,106,106,107,107,107,106,106,106,106,106,106,106,106,106,107,107,107,107,107,106,106,106,105,105,104,106,105,106,106,107,106,105,105,105,105,107,106,106,0,0],[0,0,0,0,0,0,105,106,104,106,106,106,106,106,107,106,107,106,106,106,106,106,106,105,106,106,0,0,0,0,0,0,0,0,0,0,105,106,107,106,107,106,105,106,106,106,0,0,0,0,0,0,0,0,0,0,107,106,106,106,106,107,106,106,107,106,106,106,104,106,105,105,105,105,106,106,106,107,107,106,107,106,106,106,105,105,106,106,106,106,106,106,106,106,106,106,106,0,0,0],[0,0,0,0,0,105,105,105,105,106,106,106,105,105,106,106,107,106,106,107,105,106,105,105,106,106,0,0,0,0,0,0,0,0,0,0,107,106,106,105,107,105,105,106,106,106,0,0,0,0,0,0,0,0,0,0,0,107,107,106,107,107,107,106,107,106,106,106,105,105,105,106,105,107,106,106,106,106,105,105,106,105,106,106,106,106,106,106,106,105,106,107,106,106,105,105,106,0,0,0],[0,0,0,0,105,105,105,106,106,106,106,104,106,106,108,106,106,106,105,105,106,104,102,106,105,106,0,0,0,0,0,0,0,0,0,0,107,106,105,105,105,105,105,106,105,106,0,0,0,0,0,0,0,0,0,0,0,107,107,105,107,107,106,106,106,105,106,106,105,105,105,105,105,105,107,107,106,106,106,105,105,106,106,106,106,106,106,106,106,106,106,106,106,106,104,107,105,107,0,0],[0,0,0,0,106,106,106,106,106,106,106,106,106,107,105,106,106,106,106,105,105,104,106,106,106,106,0,0,0,0,0,0,0,0,0,0,0,105,105,105,106,106,106,105,106,105,0,0,0,0,0,0,0,0,0,0,0,0,107,106,106,106,105,106,106,106,105,105,105,105,105,106,105,105,106,106,106,104,106,107,106,106,106,105,106,106,106,106,106,106,107,107,107,105,106,106,105,106,0,0],[0,0,0,105,105,105,106,106,106,106,106,106,106,106,105,105,106,105,106,106,106,105,105,106,106,106,106,0,0,0,0,0,0,0,0,0,0,0,105,105,105,106,106,106,105,105,106,0,0,0,0,0,0,0,0,0,0,0,104,106,107,106,106,106,106,105,0,0,106,105,105,106,105,105,104,106,106,106,105,105,106,106,106,105,106,105,106,106,106,104,107,106,106,105,105,104,104,106,0,0],[0,0,0,105,105,105,105,106,106,105,106,106,106,106,105,106,106,105,106,106,106,106,106,106,106,105,106,106,0,0,0,0,0,0,0,0,0,0,105,106,106,104,106,105,105,106,106,105,0,0,0,0,0,0,0,0,0,105,105,105,106,106,106,106,106,0,0,0,0,106,105,105,104,105,105,105,105,105,105,106,107,106,106,105,105,106,105,105,105,105,106,106,106,106,105,105,105,106,106,0],[0,0,0,106,106,106,106,106,106,106,106,105,106,106,105,106,105,106,0,0,106,106,106,106,107,106,106,105,0,0,0,0,0,0,0,0,0,0,105,106,106,105,106,106,106,106,105,106,105,0,0,0,0,0,0,0,106,105,105,105,106,106,106,106,0,0,0,0,0,0,106,106,104,106,105,105,106,106,106,106,107,106,106,106,106,105,106,106,106,106,106,106,107,106,106,107,106,105,104,0],[0,0,0,105,106,105,105,106,106,106,106,105,106,106,106,105,106,0,0,0,0,105,106,106,107,106,105,105,0,0,0,0,0,0,0,0,0,0,105,106,106,106,106,106,107,106,106,106,106,107,0,0,0,0,0,106,105,105,106,106,106,106,106,0,0,0,0,0,0,0,105,104,106,107,106,106,105,106,107,106,106,106,106,106,106,105,105,106,106,106,106,106,105,107,107,105,106,106,104,0],[0,0,0,106,106,106,106,105,106,106,106,106,106,106,106,106,106,0,0,0,0,106,106,106,105,106,105,0,0,0,0,0,0,0,0,0,0,0,106,106,107,105,106,107,107,106,106,104,105,106,106,0,0,0,105,105,106,106,107,106,106,106,106,0,0,0,0,0,0,106,105,106,106,107,107,105,106,107,107,107,106,106,106,106,106,105,106,106,106,106,106,106,106,106,106,106,106,106,0,0],[0,0,0,105,106,105,105,105,106,106,106,106,106,106,106,106,106,0,0,0,0,106,107,105,106,106,105,0,0,0,0,0,0,0,0,0,0,106,105,105,107,107,107,107,106,106,106,106,106,106,105,107,107,107,105,105,104,107,106,107,105,105,106,0,0,0,0,0,106,106,106,106,105,106,105,107,107,106,106,106,106,107,106,106,106,107,106,106,106,106,105,106,107,105,104,105,105,0,0,0],[0,0,0,106,105,106,105,106,105,106,106,106,106,106,106,105,105,0,0,0,106,106,106,106,106,106,106,105,0,0,0,0,0,0,0,0,0,105,105,106,106,107,107,107,106,106,106,106,105,106,106,107,107,107,106,104,106,106,106,106,105,105,106,0,0,0,0,0,106,106,106,106,106,106,106,105,107,106,104,106,106,105,106,106,105,106,106,106,106,106,106,106,106,105,105,106,105,0,0,0],[0,0,105,105,105,105,105,104,105,105,106,105,106,106,105,107,106,0,0,0,105,105,106,106,106,106,106,106,0,0,0,0,0,0,0,0,0,0,105,106,106,107,106,106,106,106,106,106,106,0,0,0,106,105,105,105,106,106,106,106,106,107,105,106,0,0,0,0,106,106,105,105,106,105,105,105,105,105,105,106,106,106,106,106,106,106,106,106,105,105,106,105,105,106,105,105,106,105,0,0],[0,0,105,104,106,106,103,105,105,106,105,106,106,105,105,106,107,0,0,0,0,106,105,106,106,106,106,0,0,0,0,0,0,0,0,0,0,0,105,106,106,105,105,105,105,105,106,106,0,0,0,0,0,106,106,105,106,106,106,106,107,106,106,106,106,0,0,0,106,106,105,105,105,106,106,105,105,106,106,106,106,106,106,106,106,106,106,105,105,105,106,106,106,106,104,106,106,105,0,0],[0,0,0,106,105,106,105,105,106,106,106,106,107,106,106,106,106,106,0,0,0,105,105,105,106,106,107,0,0,0,0,0,0,0,0,0,0,0,107,106,106,105,106,106,105,105,106,106,0,0,0,0,0,106,106,106,106,107,106,106,105,106,106,106,106,106,0,106,107,105,106,105,106,106,106,105,105,105,105,106,106,106,105,105,105,105,105,105,105,106,105,106,106,105,105,105,106,105,0,0],[0,0,0,106,106,106,106,106,106,106,106,106,107,107,106,107,106,107,106,105,105,106,105,105,106,106,107,0,0,0,0,0,0,0,0,0,0,0,105,106,105,104,105,106,106,106,105,107,106,0,0,0,0,106,106,106,107,106,106,105,105,106,106,107,106,106,106,105,105,107,106,106,106,106,106,105,105,105,105,106,106,106,106,105,106,105,106,105,107,106,105,105,106,106,106,106,106,105,0,0],[0,0,0,106,106,105,105,105,106,104,105,106,106,107,107,106,106,106,106,106,106,106,106,105,106,107,107,106,0,0,0,0,0,0,0,0,0,0,105,106,107,105,106,106,106,106,105,106,106,107,107,106,106,106,106,106,105,106,105,105,105,105,105,106,106,104,105,105,105,105,106,105,107,107,106,105,105,105,106,106,106,106,106,106,105,105,105,106,105,106,106,105,106,105,105,106,105,105,0,0],[0,0,106,106,106,106,106,106,106,106,105,105,106,106,107,106,106,106,105,104,105,106,105,106,106,107,107,105,105,0,0,0,0,0,0,0,0,0,0,105,106,106,105,106,106,106,106,106,107,107,106,105,106,106,106,106,105,106,105,106,106,105,105,106,106,106,106,105,105,105,106,105,106,105,107,106,105,106,104,105,105,106,105,105,105,105,106,106,104,105,105,106,105,107,106,105,106,105,0,0],[0,105,105,106,106,105,106,106,106,106,105,105,105,107,106,106,106,105,106,106,105,105,105,106,106,106,106,106,107,106,0,0,0,0,0,0,0,0,0,0,106,106,106,106,106,106,106,107,107,106,106,106,106,106,106,105,105,105,106,106,106,106,106,106,106,106,106,106,106,105,105,106,106,106,106,106,105,105,106,106,106,104,106,106,105,105,104,105,105,106,105,106,106,105,105,105,106,105,104,0],[0,106,105,105,105,105,105,105,106,106,105,105,106,107,106,106,106,106,105,106,106,105,106,106,107,106,106,106,106,106,106,0,0,0,0,0,0,0,0,0,0,106,104,106,106,106,105,106,107,106,107,106,106,106,106,105,105,105,106,105,105,105,106,106,106,106,106,106,104,105,106,106,106,106,106,106,106,106,106,0,0,106,105,105,106,105,105,105,106,106,106,106,106,105,106,106,106,105,105,0],[0,105,105,105,104,105,105,106,105,105,105,105,106,106,105,105,106,107,106,106,106,105,106,106,106,106,106,105,106,106,106,0,0,0,0,0,0,0,0,0,0,105,106,106,106,106,107,107,106,106,106,106,106,106,106,105,104,105,105,105,104,105,106,106,106,106,105,105,105,105,106,106,106,106,106,106,106,106,0,0,0,0,0,0,0,0,105,106,106,106,106,106,105,106,105,106,106,105,105,0],[0,106,105,105,106,106,105,106,106,105,105,105,106,105,106,106,106,106,105,106,105,106,105,106,106,105,106,105,106,106,106,0,0,0,0,0,0,0,0,0,0,0,106,106,106,106,106,106,107,107,106,106,106,106,105,104,105,105,105,105,105,105,105,105,105,105,105,105,106,106,106,106,106,106,106,105,105,106,0,0,0,0,0,0,0,0,0,106,106,106,106,105,105,105,106,106,104,105,105,0],[0,0,104,105,104,106,106,106,104,105,105,105,105,105,107,106,106,106,106,106,106,107,107,106,106,105,106,106,105,106,106,0,0,0,0,0,0,0,0,0,0,0,0,106,106,106,105,106,105,105,106,105,106,104,104,106,106,106,106,105,105,106,105,105,105,105,105,105,106,105,106,106,106,107,106,106,106,106,0,0,0,0,0,0,0,0,0,0,106,105,105,105,106,106,106,105,104,104,105,0],[0,0,106,105,105,106,106,106,104,106,106,105,105,105,105,106,106,106,106,106,106,107,105,106,105,106,106,105,106,105,105,0,0,0,0,0,0,0,0,0,0,0,0,105,107,106,106,106,106,105,106,105,106,105,105,106,106,105,106,105,106,106,106,105,105,105,105,105,105,106,105,106,105,106,106,105,106,106,106,0,0,0,0,0,0,0,0,0,0,106,105,105,106,105,105,103,104,106,105,0],[0,0,106,105,106,106,106,106,106,106,106,106,105,105,106,106,106,105,105,105,106,105,106,105,106,106,107,106,105,104,103,105,0,0,0,0,0,0,0,0,0,0,0,0,106,105,105,106,106,106,106,105,105,105,107,106,105,106,105,106,106,106,106,106,106,106,104,105,105,105,106,106,105,106,105,104,106,106,106,106,0,0,0,0,0,0,0,0,0,104,106,106,106,105,105,105,105,105,105,0],[0,106,106,105,106,106,106,106,106,106,106,106,105,105,106,106,106,105,104,104,105,106,105,106,105,106,106,106,105,105,105,104,105,106,106,0,0,0,0,0,0,0,0,0,105,106,106,105,105,105,105,105,105,105,106,105,106,106,106,105,106,106,106,106,106,106,106,105,105,105,106,107,106,106,106,105,106,106,107,105,0,0,0,0,0,0,0,0,0,106,106,106,106,107,106,104,105,106,105,0],[0,105,106,106,106,105,105,106,106,105,106,105,105,105,106,106,106,105,105,105,105,106,106,105,106,106,105,105,106,105,105,104,105,106,106,106,0,0,0,0,0,0,0,0,105,105,105,104,105,105,106,106,105,106,106,106,106,106,106,106,106,106,106,106,106,106,106,106,106,105,105,105,106,107,106,106,106,107,106,106,106,0,0,0,0,0,0,0,103,105,106,106,106,106,107,106,105,106,0,0],[0,105,106,106,106,106,105,105,105,105,105,106,105,105,105,0,0,103,105,105,106,107,106,106,106,106,105,106,105,106,106,106,106,106,105,107,0,0,0,0,0,0,0,0,106,105,105,105,105,106,106,105,106,107,106,106,106,106,107,106,105,105,105,107,106,107,106,106,106,105,105,105,106,105,106,107,106,106,107,106,106,0,0,0,0,0,0,0,105,106,106,106,106,107,106,107,106,105,0,0],[0,105,105,106,106,106,106,106,105,106,106,105,106,106,0,0,0,106,106,105,106,106,106,106,105,106,105,106,106,106,108,107,105,106,106,105,106,0,0,0,0,0,0,0,107,106,106,105,105,104,106,106,106,106,106,106,106,106,106,106,106,105,105,106,105,106,106,106,106,105,105,105,105,106,106,107,106,107,106,107,107,0,0,0,0,0,0,0,105,105,106,106,104,105,106,106,105,106,0,0],[0,105,106,107,106,106,106,106,106,106,106,106,106,105,106,0,106,106,106,106,106,106,106,106,106,106,106,105,106,106,107,107,107,106,105,107,106,105,0,0,0,0,0,0,106,106,106,105,105,104,105,105,104,105,105,107,106,106,107,106,106,106,105,106,106,105,105,106,106,106,106,105,105,106,105,106,107,105,106,106,106,0,0,0,0,0,0,107,106,106,106,105,106,106,107,105,106,106,0,0],[0,105,106,105,106,106,106,106,106,106,106,106,106,107,106,106,106,106,106,106,105,105,106,106,106,106,105,106,105,106,107,107,106,107,106,106,106,106,106,0,0,0,0,0,105,106,106,106,105,105,105,105,104,106,105,106,105,107,106,105,106,107,106,106,0,0,106,103,106,106,106,106,106,105,106,106,106,105,106,105,106,106,0,0,0,0,105,106,106,106,106,106,106,106,106,106,106,106,0,0],[0,0,106,105,105,106,107,105,106,106,105,106,106,106,106,106,105,106,106,106,105,105,106,105,106,106,106,106,105,106,106,107,107,106,106,106,105,105,106,106,106,0,0,0,0,105,106,105,106,105,105,105,105,105,106,106,106,106,107,106,106,106,106,0,0,0,0,106,106,106,106,107,106,106,105,106,106,105,106,106,105,106,106,0,0,106,106,106,106,106,105,105,106,106,106,104,105,0,0,0],[0,0,104,105,105,106,106,107,106,106,106,104,105,106,106,106,106,105,106,106,105,105,105,106,107,106,106,106,106,105,106,106,107,107,107,106,105,106,106,105,106,106,0,0,0,0,106,105,105,105,104,105,105,106,106,106,105,107,106,106,106,106,0,0,0,0,0,106,106,106,106,106,107,106,105,105,106,106,106,106,105,105,106,106,107,106,106,106,106,106,106,107,105,106,104,106,105,0,0,0],[0,0,0,105,105,106,105,107,106,106,106,106,106,106,106,106,106,106,105,106,105,105,106,106,107,106,106,106,106,106,106,107,105,104,105,106,106,106,106,106,107,106,0,0,0,0,0,105,105,105,105,106,106,106,105,105,106,106,105,105,105,0,0,0,0,0,106,106,105,105,105,105,107,107,105,105,105,106,106,106,105,106,106,106,106,0,0,0,106,106,107,107,106,106,104,107,0,0,0,0],[0,0,0,105,104,104,106,106,106,106,106,105,106,106,106,105,106,106,105,107,106,106,106,106,106,105,106,105,106,106,106,105,105,105,106,105,106,106,106,106,106,106,0,0,0,0,0,0,106,106,105,106,106,105,105,105,105,104,104,105,0,0,0,0,0,105,104,105,105,106,105,105,105,106,106,105,107,107,106,106,106,106,106,106,0,0,0,0,0,105,106,106,106,106,107,106,0,0,0,0],[0,0,0,103,105,106,105,107,106,105,106,106,106,106,105,106,106,106,106,106,107,106,106,106,103,106,106,105,105,105,106,105,105,105,106,106,106,106,106,106,105,106,106,0,0,0,0,0,0,106,106,106,107,106,106,105,105,105,105,0,0,0,0,0,106,105,104,105,106,106,106,106,106,105,105,106,106,107,105,106,105,106,106,106,0,0,0,0,0,105,106,106,106,106,105,105,0,0,0,0],[0,0,0,0,105,106,106,105,106,105,106,106,105,106,106,106,106,106,105,106,106,105,105,105,105,106,104,105,106,104,105,105,105,105,105,106,106,106,106,106,106,106,106,106,106,0,0,0,0,0,106,106,107,105,105,104,104,105,105,0,0,0,0,0,106,105,106,106,106,106,105,106,107,106,107,106,107,105,106,107,106,106,106,105,0,0,0,0,0,0,106,106,105,106,106,105,106,0,0,0],[0,0,0,0,105,105,105,106,104,105,105,105,106,106,106,106,106,105,105,104,104,105,105,105,106,106,106,106,104,107,106,107,105,106,106,105,106,106,106,106,106,105,106,106,106,105,0,0,0,0,106,106,106,105,105,105,103,105,105,105,0,0,0,0,105,106,105,106,105,106,107,106,106,107,106,106,106,105,106,107,106,105,106,0,0,0,0,0,0,0,0,106,106,106,106,106,106,105,0,0],[0,0,0,0,105,105,105,106,106,104,106,106,106,106,106,106,107,107,106,106,105,105,106,106,106,106,105,105,106,106,107,106,105,105,106,104,105,107,106,106,106,106,106,106,106,105,106,0,0,0,106,106,106,106,105,106,105,105,105,105,105,0,0,0,0,106,106,106,106,107,106,106,107,106,105,106,105,105,105,106,107,107,0,0,0,0,0,0,0,0,0,0,106,106,105,107,105,105,0,0],[0,0,0,105,105,105,106,106,106,104,105,105,105,0,0,0,107,107,106,106,106,106,105,106,106,106,0,0,0,106,106,106,106,105,105,106,106,106,106,106,107,106,106,106,106,105,105,0,0,0,0,106,106,106,105,106,106,106,105,106,105,106,0,0,0,0,106,106,106,107,106,106,106,106,105,106,106,106,105,106,106,106,0,0,0,0,0,0,0,0,0,0,106,105,107,107,106,106,0,0],[0,0,0,106,106,106,106,106,106,105,105,106,0,0,0,0,0,105,106,107,106,106,106,106,106,0,0,0,0,0,106,106,107,106,106,106,106,106,106,106,106,106,107,106,105,106,0,0,0,0,0,106,105,106,105,106,106,105,106,107,106,106,0,0,0,0,106,106,106,105,104,105,106,106,105,106,106,106,105,104,106,107,107,0,0,0,0,105,105,0,0,105,105,106,107,107,107,106,0,0],[0,0,0,106,106,106,106,107,106,106,106,106,0,0,0,0,0,106,107,107,106,107,0,0,0,0,0,0,0,0,107,106,105,105,106,106,106,106,106,107,106,107,105,107,106,0,0,0,0,0,0,106,106,106,106,106,106,105,106,106,104,106,106,0,0,0,0,105,105,105,106,106,106,106,106,105,106,106,103,105,106,106,106,106,0,0,105,105,105,104,106,106,104,105,106,107,106,0,0,0],[0,0,0,0,105,106,107,106,106,106,106,0,0,0,0,0,0,106,107,106,107,0,0,0,0,0,0,0,0,0,106,105,105,106,105,106,106,106,105,106,105,105,106,105,106,0,0,0,0,0,0,106,105,105,106,106,106,105,105,104,106,106,106,107,0,0,0,0,105,105,104,106,106,106,104,106,107,106,106,107,106,106,106,106,106,105,104,105,105,105,106,106,104,106,106,106,106,0,0,0],[0,0,0,0,0,105,106,106,106,106,105,0,0,0,0,0,0,106,107,106,106,0,0,0,0,0,0,0,0,106,106,106,105,106,106,106,106,106,106,106,106,106,106,107,105,105,0,0,0,0,0,106,106,106,106,106,105,103,105,106,107,106,106,106,0,0,0,0,0,105,106,106,106,107,106,107,106,106,106,106,107,106,106,104,105,105,104,105,105,105,105,106,104,106,106,106,104,0,0,0],[0,0,0,0,0,105,105,105,106,106,106,0,0,0,0,0,106,105,107,105,106,106,0,0,0,0,0,0,0,105,106,106,106,106,106,106,106,106,106,106,106,106,106,106,105,105,106,0,0,0,106,106,106,106,106,105,105,105,105,106,106,105,105,105,105,0,0,0,0,105,106,105,107,107,107,107,106,106,106,107,106,106,106,106,105,105,105,104,105,105,105,106,106,106,106,105,105,0,0,0],[0,0,0,0,0,105,105,105,106,106,106,0,0,0,0,0,105,105,105,106,106,106,0,0,0,0,0,0,0,106,106,105,106,106,105,106,106,106,106,106,106,106,106,106,106,106,104,105,105,104,105,106,106,106,106,105,105,105,106,106,106,105,104,105,105,105,0,0,0,0,106,106,107,106,107,106,107,106,106,106,106,106,106,105,105,106,105,106,104,104,106,106,106,106,106,105,105,105,0,0],[0,0,0,0,0,106,105,105,106,106,106,106,0,0,0,106,105,104,106,105,106,107,0,0,0,0,0,0,107,106,106,106,106,106,105,106,107,105,105,106,106,106,106,106,106,106,105,106,106,106,106,106,106,106,106,106,105,105,106,106,106,104,105,107,106,105,0,0,0,0,0,106,107,107,107,106,106,107,106,106,105,106,106,103,105,106,107,106,105,104,105,106,107,106,107,105,105,106,0,0],[0,0,0,0,0,0,106,106,105,106,106,106,106,106,106,106,106,106,106,107,107,106,107,0,0,0,0,0,106,105,104,106,107,106,106,106,106,104,105,105,106,106,106,106,106,106,105,106,106,106,106,106,106,106,106,106,106,106,106,106,105,105,106,106,106,0,0,0,0,0,0,0,107,105,106,106,0,0,106,106,106,105,105,105,106,106,107,106,106,104,106,106,106,107,105,105,106,105,0,0],[0,0,0,0,0,0,106,106,106,106,106,106,106,105,106,105,106,106,107,106,107,106,105,0,0,0,0,106,105,107,106,106,105,106,105,106,105,105,105,106,106,105,105,105,105,105,106,106,106,106,106,106,106,106,106,106,106,106,106,106,106,106,105,106,0,0,0,0,0,0,0,0,105,105,104,0,0,0,0,106,106,107,106,106,106,106,107,106,107,107,107,107,107,105,106,106,106,105,0,0],[0,0,0,0,0,0,104,105,105,106,106,106,106,105,105,106,106,106,106,106,106,106,0,0,0,0,106,106,106,105,106,106,106,104,105,105,105,105,105,106,106,105,106,105,106,106,106,106,107,106,106,105,106,106,106,106,106,104,106,106,106,106,104,105,0,0,0,0,0,0,0,0,104,105,105,0,0,0,0,106,106,106,106,106,106,105,107,107,106,106,106,107,107,105,105,106,105,105,0,0],[0,0,0,0,0,0,0,106,105,106,106,106,105,106,106,106,106,106,106,106,106,106,0,0,0,0,107,106,106,106,106,106,105,105,105,105,104,105,106,107,105,106,106,106,106,107,106,106,107,107,105,106,106,106,106,106,106,106,106,106,106,105,105,0,0,0,0,0,0,0,0,0,106,105,105,105,0,0,0,105,107,106,106,105,105,106,106,106,107,106,105,105,105,106,106,105,105,106,0,0],[0,0,0,0,0,0,0,106,106,106,106,106,104,106,107,106,106,106,105,105,106,106,105,0,0,106,106,106,106,106,106,106,106,105,106,106,106,105,106,106,106,105,105,106,106,107,106,106,106,105,106,106,106,106,106,106,106,106,106,106,105,106,0,0,0,0,0,0,0,0,0,0,106,105,106,104,0,0,0,104,103,105,106,105,105,105,106,106,106,106,106,105,105,106,105,106,106,106,0,0],[0,0,0,0,0,0,0,0,105,106,105,106,107,106,0,0,0,0,106,105,106,106,107,107,106,106,106,105,105,106,105,106,106,106,106,106,106,106,105,106,106,106,104,105,106,106,106,106,106,107,106,106,106,106,106,105,107,106,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,106,0,0,0,0,104,105,106,106,105,105,106,106,106,106,106,105,105,105,105,106,106,106,107,0,0],[0,0,0,0,0,0,0,0,105,106,107,105,107,0,0,0,0,0,0,106,106,106,106,107,106,105,106,106,106,106,105,106,106,107,106,105,107,106,106,105,106,106,104,106,106,105,106,106,107,107,106,106,106,106,105,107,106,106,0,0,0,0,0,0,0,0,0,104,106,0,0,0,0,0,0,0,0,0,0,106,106,106,105,105,106,0,0,0,106,106,105,105,105,105,106,105,107,107,0,0],[0,0,0,0,0,0,0,0,0,107,107,107,106,0,0,0,0,0,0,0,105,106,106,105,106,106,105,106,106,105,105,106,107,107,106,106,104,106,105,107,105,105,105,106,105,107,106,106,105,105,106,106,104,105,106,106,106,0,0,0,0,0,0,0,0,0,107,106,105,106,0,0,0,0,0,0,0,0,106,105,106,105,105,106,0,0,0,0,0,106,106,105,105,105,106,105,106,107,0,0],[0,0,0,0,0,0,0,0,0,107,107,107,106,106,0,0,0,0,0,0,0,105,106,105,105,106,105,105,106,105,106,105,107,106,107,106,106,105,106,106,105,105,105,107,107,107,106,106,106,106,106,106,105,106,106,106,106,0,0,0,0,0,0,0,0,0,107,107,106,105,105,0,0,0,0,0,0,106,106,106,106,106,106,106,0,0,0,0,0,0,106,105,106,105,105,105,106,106,0,0],[0,0,0,0,0,0,0,0,106,107,107,106,105,104,105,0,0,0,0,0,0,0,105,105,104,106,106,105,106,106,105,106,106,106,106,106,106,107,107,105,106,106,106,106,107,106,107,107,107,105,105,105,106,106,106,106,105,0,0,0,0,0,0,0,0,0,106,106,106,105,105,106,0,0,0,0,106,106,106,106,107,106,106,106,0,0,0,0,0,0,106,106,105,105,105,105,106,106,0,0],[0,0,0,0,0,0,0,106,106,107,106,107,106,105,106,0,0,0,0,0,0,0,0,105,105,106,106,107,106,106,106,106,105,106,106,106,107,107,107,106,106,106,106,106,106,106,106,107,107,106,106,105,106,107,106,106,105,104,0,0,0,0,0,0,0,106,106,106,106,106,106,106,0,0,0,0,107,107,107,107,106,105,106,106,0,0,0,0,0,0,0,106,105,106,105,105,105,106,0,0],[0,0,0,0,0,0,104,106,106,106,106,106,106,106,106,0,0,0,0,0,0,0,0,0,105,105,107,107,106,106,106,105,105,105,106,106,106,107,106,106,106,106,105,106,106,105,106,107,106,106,105,105,106,105,105,105,105,105,0,0,0,0,0,0,106,106,105,105,106,106,105,107,0,0,0,0,105,106,106,106,106,105,104,106,0,0,0,0,0,0,0,107,106,105,104,105,106,106,0,0],[0,0,0,0,0,0,106,106,106,106,106,106,106,106,106,0,0,0,0,0,0,0,0,0,105,105,107,107,107,105,106,105,105,106,106,107,106,106,106,106,106,107,105,105,106,105,106,105,106,106,105,105,106,106,106,106,106,105,0,0,0,0,0,0,106,106,106,106,106,106,105,105,105,0,0,106,106,106,106,105,106,104,106,105,0,0,0,0,0,0,0,106,105,105,106,106,106,106,0,0],[0,0,0,0,0,0,0,106,105,106,106,106,106,106,0,0,0,0,0,0,0,0,0,0,106,107,106,106,105,105,106,105,105,106,106,106,106,106,106,106,105,105,105,105,105,106,105,0,0,0,0,105,105,106,106,106,105,0,0,0,0,0,0,0,0,106,106,106,106,106,106,105,105,105,106,106,106,106,106,105,105,106,106,0,0,0,0,0,0,0,0,106,106,105,106,106,106,106,0,0],[0,0,0,0,0,0,0,0,105,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,105,105,105,105,106,106,105,106,106,106,104,104,105,106,105,105,106,106,106,0,0,0,0,0,0,0,106,106,106,106,105,0,0,0,0,0,0,0,0,106,106,106,106,105,105,105,104,104,104,105,107,106,106,106,0,0,0,0,0,0,0,0,0,0,0,105,104,106,107,107,106,106,0,0],[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,106,106,105,105,105,104,104,104,104,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,105,104,0,0,0,0,0,0,0,0,0,0,105,105,106,105,105,105,104,104,104,105,106,105,104,0,0,0,0,0,0,0,0,0,0,0,0,0,105,106,106,106,105,0,0,0],[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]],\"mapSceneryIDs\":[[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,9,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,6,8,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,6,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,4,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,6,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,4,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,8,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,9,-1,-1,-1,8,-1,-1,10,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,4,-1,-1,11,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,4,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,5,-1,6,-1,-1,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,3,-1,-1,8,-1,-1,-1,-1,-1,-1,10,-1,-1,1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,11,-1,-1,-1,-1],[-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,0,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,4,-1,-1,-1,200,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,5,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,6,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,8,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,7,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,4,-1,7,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,8,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,11,11,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,6,-1,12,-1,4,7,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,7,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,0,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,0,-1,-1,-1,11,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,1,6,-1,-1,-1,200,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,8,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,2,5,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,4,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,6,-1,2,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,8,-1,-1,-1,2,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,4,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,3,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,10,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,2,7,-1,-1,9,-1,2,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,0,-1,5,-1,-1,-1,-1,-1,8,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,6,12,-1,-1,11,4,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,10,-1,10,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,12,-1,-1,-1,-1,-1,5,7,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1],[-1,-1,12,-1,-1,5,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,10,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,2,7,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,12,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,0,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,1,12,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,9,-1,-1,-1,-1,-1,4,0,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,12,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,11,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,9,6,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,7,2,5,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,8,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,8,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,7,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,0,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,11,8,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,5,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,8,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,3,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1],[-1,-1,-1,0,-1,-1,-1,12,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,7,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,12,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,6,-1,-1,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,5,12,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,8,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,5,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,4,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,3,2,-1,-1,-1,-1,-1,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,0,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,-1,-1,6,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,11,-1,-1,5,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,4,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,10,-1,-1,-1,-1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,10,-1,-1,-1,-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1]],\"mapResources\":[[72,66],[73,59],[5,46],[85,63],[93,54],[72,44],[55,21],[18,16],[49,74],[75,19],[54,16],[56,93],[63,91],[89,67],[77,96],[17,88],[37,59],[86,58],[33,6],[33,65],[28,64],[26,91],[75,96],[54,82],[71,17],[18,37],[60,14],[73,35],[56,90],[19,33],[28,4],[35,91],[73,67],[63,51],[22,68],[28,22],[92,42],[64,91],[87,33],[29,84],[19,40],[69,51],[19,78],[44,23],[34,88],[78,83],[65,75],[29,59],[43,41],[8,36]]}]";
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: send_data
    --
    -- DATE:January 20th, 2016
    --
    -- REVISIONS: March 15th, 2016: Hooked it up with actual networking code
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: string send_data(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Creates our outgoing TCP and UDP packet to send to the server. It checks if the length of the packet is big enough
    -- that the data was not empty, and if that is the case it passes it over to the C++ TCP_Send and UDP_Send functions.
    ---------------------------------------------------------------------------------------------------------------------*/
    private void send_data() {
        //Create our TCP outgoing packet
        var tcp = create_sending_json(Protocol.TCP);
        //Create our UDP outgoing packet
        var udp = create_sending_json(Protocol.UDP);
        //If we are on Linux, try to send it
        if (Application.platform == RuntimePlatform.LinuxPlayer) {
			//Cant send empty packets to server, inefficient. Make sure data is in packets before sending
			if (tcp.Length > 8)
            	TCP_Send(tcp, tcp.Length);
            if (udp.Length > 8)
                UDP_SendData(udp, udp.Length);
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: receive_data_udp
    --
    -- DATE: March 15th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: string receive_data_udp(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Wraps around our UDP_GetData call. That call returns a pointer in unamanged memory to our char *. We use C#'s
    -- Marshal.PtrToStringAnsi function to convert this pointer into a C# string object as long as we are in Linux and
    -- return that. If we are in windows, return an empty JSON array, since we can't receive valid data on Windows.
    ---------------------------------------------------------------------------------------------------------------------*/
    private string receive_data_udp() {
        //On Linux, get a proper packet
        if (Application.platform == RuntimePlatform.LinuxPlayer) {
            //Convert the pointer to the char * into a C# string
            result = Marshal.PtrToStringAnsi(UDP_GetData());
        } else {
            //On Windows, return whatever JSON data we want to generate/test for
            result = "[]";
        }

        return result;
    }

   /*---------------------------------------------------------------------------------------------------------------------
   -- METHOD: receive_data_tcp
   --
   -- DATE: March 15th, 2016
   --
   -- REVISIONS: N/A
   --
   -- DESIGNER: Carson Roscoe
   --
   -- PROGRAMMER: Carson Roscoe
   --
   -- INTERFACE: string receive_data_tcp(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Wraps around our TCP_GetData call. That call returns a pointer in unamanged memory to our char *. We use C#'s
   -- Marshal.PtrToStringAnsi function to convert this pointer into a C# string object as long as we are in Linux and
   -- return that. If we are in windows, return an empty JSON array, since we can't receive valid data on Windows.
   ---------------------------------------------------------------------------------------------------------------------*/
    private string receive_data_tcp()  {
        //On Linux, get a proper packet
        if (Application.platform == RuntimePlatform.LinuxPlayer) {
            //Convert the pointer to a char * in unmanaged code over to a C# string
            result = Marshal.PtrToStringAnsi(TCP_GetData());
        } else {
            //On Windows, return whatever JSON data we want to generate/test for
            result = "[]";
        }

        return result;
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: create_sending_json
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: string create_sending_json(Protocol protocol to send, either TCP or UDP)
    --
    -- RETURNS: string that represents the next outgoing packet
    --
    -- NOTES:
    -- Any object inside Unity can call send_next_packet method to request sending a packet in the next frame. This method
    -- takes in a protocol (TCP or UDP) and creates the outgoing packet to send next based on what data was requested
    -- to be sent. This is essentially our packetize function
    ---------------------------------------------------------------------------------------------------------------------*/
    private string create_sending_json(Protocol protocol) {
        //Open JSON array
        string sending = "[";

        //If we are a UDP call, get our players current game data and add it to the list of things to send out
        if (protocol == Protocol.UDP) {
            if (GameManager.instance.player != null && GameData.MyPlayer != null) {
                //Add player data
                var memberItems = new List<Pair<string, string>>();
                memberItems.Add(new Pair<string, string>("x", GameManager.instance.player.transform.position.x.ToString()));
                memberItems.Add(new Pair<string, string>("y", GameManager.instance.player.transform.position.y.ToString()));
                memberItems.Add(new Pair<string, string>("rotationZ", GameManager.instance.player.transform.rotation.z.ToString()));
                memberItems.Add(new Pair<string, string>("rotationW", GameManager.instance.player.transform.rotation.w.ToString()));
                send_next_packet(DataType.Player, GameData.MyPlayer.PlayerID, memberItems, protocol);
            } else {
                return "[]";
            }
        }

        //Foreach item to send based on the given protocol, append it to our outgoing JSON array
        foreach (var item in protocol == Protocol.TCP ? jsonTCPObjectsToSend : jsonUDPObjectsToSend)  {
            sending += item;
        }

        //Clear our TCP or UDP data to send as we just appended it to out outgoing array
        if (protocol == Protocol.TCP)
            jsonTCPObjectsToSend.Clear();
        else
            jsonUDPObjectsToSend.Clear();

        //Close json array
        if (sending.Length > 2)
            sending = sending.Remove(sending.Length - 1, 1);
        sending += "]";

        return sending;
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: send_next_packet
    --
    -- DATE: January 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: string send_next_packet(DataType type of data we are sending out
    --                                    int id of data to be sent out
    --                                    List<Pair<string, string>> list of key/value pairs for the outoging JSON packet
    --                                    Protocol protocol to send the data with)
    --
    -- RETURNS: string that represents the next outgoing packet.
    --
    -- NOTES:
    -- Any object inside Unity can call send_next_packet method to request sending a packet in the next frame. When this
    -- object is invoked it takes a DataType and an ID which will be added as members to the JSON object. Next it takes
    -- a list of key/value pairs which will each be key/value pairs in the JSON object as well. Finally the protocol 
    -- parameter determines what to do with this packet. If the protocol is TCP, it adds it to the list of TCP JSON objects
    -- to send. For UDP it adds it to the UDP list. If it set to Protocol.NA then we will not send out this JSON object,
    -- but instead simply return it as the only object within a JSON array. This final "protocol" option is strictly used
    -- for testing or generating a packet that will be sent at a later time.
    ---------------------------------------------------------------------------------------------------------------------*/
    public static string send_next_packet(DataType type, int id, List<Pair<string, string>> memersToSend, Protocol protocol) {
        string sending = "";
        //If Protocol.NA, start with an open square bracket since we will return it in array form
        if (protocol == Protocol.NA)
            sending += "[";
        sending += "{";
        //Add the DataType and ID key/values to our JSON object
        sending += "\"DataType\":" + (int)type + ",\"ID\":" + id + ",";

        //Add the key/value pair for each member in the list to send
        foreach (var pair in memersToSend) {
            sending += "\"" + pair.first + "\":" + pair.second + ",";
        }

        //Close the object/array
        sending = sending.Remove(sending.Length - 1, 1);
        if (protocol != Protocol.NA)
            sending += "},";
        else
            sending += "}]";

        //If UDP ot TCP, add to appropriate outgoing list of JSON objects
        switch (protocol)
        {
            case Protocol.UDP:
                jsonUDPObjectsToSend.Add(sending);
                break;
            case Protocol.TCP:
                jsonTCPObjectsToSend.Add(sending);
                break;
        }
        return sending;
    }
    #endregion

    ////Game creation code
    #region StartOfGame

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: StartGame
    --
    -- DATE: March 20th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void StartGame(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Called by an outside source whenever we are leaving the lobby and starting a game. Essentially starts our
    -- UDP connection as TCP was alive from Lobby code.
    ---------------------------------------------------------------------------------------------------------------------*/
    public static void StartGame() {
        try {
            UDPClient = UDP_CreateClient();
            UDP_ConnectToServer(GameData.IP, 8000);
            UDP_StartReadThread();
			GameData.GameStart = true;
		} catch (Exception) {
            //On Windows, not an actual error, simply can't find the libraries since they are Linux only
        }
    }

    #endregion
}