using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

/*Carson
Being used to denote what type of data we are sending/receiving for a given JSON object.
e.g. Player is valued at 1. If we receive a JSON object for type Player ID 1, that is "Player 1's" data.
     Projectile is defined at 2. If we receive a JSON object for type Projectile ID 3, that is "Projectile 3's" data
    
Enviroment does not have an ID associated with it, since it is one entity. The ID we use for it will always default to 0

Note: Does not start at value 0. Reason being, if JSON parser fails, it returns 0 for fail, so checking
for fail does not work 
*/
public enum DataType {
    Player = 1, Projectile = 2, Enviroment = 3
}

/*Carson
Class used for handling sending/receiving data. The class has 2 uses:
* To send/receive data from the Networking Team's clientside code, and
* Notifying subscribed objects when new data is updated

To subscribe for an objects updates from server, you would call the public Subscribe method.
This method takes in three things:
    Callback method, which is a void method that takes in a JSONClass as a parameter
    DataType you want to receive, e.g. DataType.Player for data of a player
    int ID of which of the DataType you want to receive info from, e.g. ID 1 on DataType.Player is Player 1's data

e.g. NetworkingManager.Subscribe((JSONClass json) => {Debug.Log("Got Player 1's Data");}, DataType.Player, 1);
*/
public class NetworkingManager : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

        /*Subscribe(
            (JSONClass json) =>
            {
                Debug.Log("Player " + json["ID"] + " x:" + json["x"].AsInt + " y:" + json["y"].AsInt);
            }, DataType.Player, 1);

        Subscribe(
            (JSONClass json) =>
            {
                Debug.Log("Player " + json["ID"] + " x:" + json["x"].AsInt + " y:" + json["y"].AsInt);
            }, DataType.Player, 3);

        Subscribe(
            (JSONClass json) =>
            {
                Debug.Log("Projectile " + json["ID"] + " x:" + json["x"].AsInt + " y:" + json["y"].AsInt);
            }, DataType.Projectile, 2);

        Subscribe(
            (JSONClass json) =>
            {
                Debug.Log("Enviroment Data:" + json["data"]);
            }, DataType.Enviroment);*/
    }

    //Dummy data for the sake of testing.
    int _x = -12, _y = -4;

    // Update is called once per frame
    void Update()
    {
        //Fake receiving of data every frame 
        update_data("["
                 + "{DataType : 1, ID : 1, x : " + _x + ", y : " + _y + "}"
                 + "]");
        _x += 2;
        _y += 1;
    }

    //Code for subscribing to updates from client-server system.
    #region SubscriberSystem

    //Holds the subscriber data
    private static Dictionary<Pair, List<Action<JSONClass>>> _subscribedActions = new Dictionary<Pair, List<Action<JSONClass>>>();
    /*
    To subscribe for an objects updates from server, you would call the public Subscribe method.
    This method takes in three things:
    Callback method, which is a void method that takes in a JSONClass as a parameter
    DataType you want to receive, e.g. DataType.Player for data of a player
    int ID of which of the DataType you want to receive info from, e.g. ID 1 on DataType.Player is Player 1's data

    e.g. NetworkingManager.Subscribe((JSONClass json) => {Debug.Log("Got Player 1's Data");}, DataType.Player, 1);
    */
    public static void Subscribe(Action<JSONClass> callback, DataType dataType, int id = 0) {
        Pair pair = new Pair(dataType, id);

        if (!(_subscribedActions.ContainsKey(pair))) {
            _subscribedActions.Add(pair, new List<Action<JSONClass>>());
        }
        List<Action<JSONClass>> val = null;
        _subscribedActions.TryGetValue(pair, out val);
        if (val != null)
        {
            //Add our callback to the list of entries under that pair of datatype and ID.
            _subscribedActions[pair].Add(callback);
        }
    }

    private void update_data(string JSONGameState) {
        var gameObjects = JSON.Parse(JSONGameState).AsArray;
        foreach (var node in gameObjects.Children) {
            var obj = node.AsObject;
            int dataType = obj["DataType"].AsInt;
            int id = obj["ID"].AsInt;

            if (id != 0 || dataType == (int)DataType.Enviroment) {
                Pair pair = new Pair((DataType)dataType, id);
                if (_subscribedActions.ContainsKey(pair)) {
                    foreach (Action<JSONClass> callback in _subscribedActions[pair]) {
                        callback(obj);
                    }
                }
            }
        }
    }

    /* Carson
       Used as a grouping of DataType & type's ID to denote a specific object, used with the subscribing system.
       e.g. a pair of first = DataType.Player, second = 1 represents "Player 1"
    */
    public class Pair {
        //First 
        public DataType first;
        public int second;

        private static readonly IEqualityComparer Item1Comparer = EqualityComparer<DataType>.Default;
        private static readonly IEqualityComparer Item2Comparer = EqualityComparer<int>.Default;

        public Pair(DataType first, int second)
        {
            this.first = first;
            this.second = second;
        }

        public override string ToString()
        {
            return string.Format("<{0}, {1}>", first, second);
        }

        public static bool operator ==(Pair a, Pair b)
        {
            if (IsNull(a) && !IsNull(b))
                return false;

            if (!IsNull(a) && IsNull(b))
                return false;

            if (IsNull(a) && IsNull(b))
                return true;

            return
                a.first.Equals(b.first) &&
                a.second.Equals(b.second);
        }

        public static bool operator !=(Pair a, Pair b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            int multiplier = 21;
            hash = hash * multiplier + first.GetHashCode();
            hash = hash * multiplier + second.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Pair;
            if (object.ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(first, other.first) &&
                       Item2Comparer.Equals(second, other.second);
        }

        private static bool IsNull(object obj)
        {
            return object.ReferenceEquals(obj, null);
        }
    }

    #endregion

    //Code for communicating with client-server system.
    #region CommunicationWithClientSystem

    private void send_data() {

    }

    #endregion
}
