using UnityEngine;
using System.Collections;
using SimpleJSON;


//Carson
//Dummy class to emulate a player 1 object subscribing for data updates & handling receiving this data.
public class NetworkingManager_test1 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        NetworkingManager.Subscribe(update_position, DataType.Player, 1);
	}

    void update_position(JSONClass player1) {
        Vector2 position = new Vector2(player1["x"].AsFloat, player1["y"].AsFloat);
        transform.position = position;
    }
}
