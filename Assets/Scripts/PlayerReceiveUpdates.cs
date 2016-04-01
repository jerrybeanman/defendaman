using UnityEngine;
using System.Collections;
using SimpleJSON;


//Carson
//Enemy/Allies updating script
public class PlayerReceiveUpdates : MonoBehaviour {
    public int playerID;
    private Vector2 lastPosition;
    BaseClass baseClass;

	// Use this for initialization
	void Start () {
        lastPosition = transform.position;
        NetworkingManager.Subscribe(update_position, DataType.Player, playerID);
        NetworkingManager.Subscribe(took_damage, DataType.Hit, playerID);
        NetworkingManager.Subscribe(died, DataType.Killed, playerID);
        NetworkingManager.Subscribe(use_potion, DataType.Potion, playerID);
        GameData.PlayerPosition.Add(playerID, transform.position);
        baseClass = GetComponent<BaseClass>();

    }

    void update_position(JSONClass player) {
        Vector3 position = new Vector3(player["x"].AsFloat, player["y"].AsFloat, -10f);
        if (pos_changed(player["x"].AsFloat, player["y"].AsFloat))
            gameObject.GetComponent<Animator>().SetBool("moving", true);
        else
            gameObject.GetComponent<Animator>().SetBool("moving", false);
        transform.position = position;
        Quaternion rotation = new Quaternion(0, 0, player["rotationZ"].AsFloat, player["rotationW"].AsFloat);
        transform.rotation = rotation;
        GameData.PlayerPosition[playerID] = position;
        lastPosition = transform.position;
    }

    bool pos_changed(float newX, float newY)
    {
        var precision = .0001;
        return (Mathf.Abs(newX - lastPosition.x) > precision ||
            Mathf.Abs(newY - lastPosition.y) > precision);
    }

    void took_damage(JSONClass packet)
    {
        baseClass.ClassStat.CurrentHp = packet["NewHP"].AsFloat;

        GameManager.instance.PlayerTookDamage(playerID, packet["NewHP"].AsFloat, baseClass.ClassStat);

        if (baseClass.ClassStat.CurrentHp <= 0.0f)
        {
            NetworkingManager.Unsubscribe(DataType.Player, playerID);
            GameData.PlayerPosition.Remove(playerID);
            Destroy(gameObject);
        }
    }

    void died(JSONClass packet)
    {
        NetworkingManager.Unsubscribe(DataType.Player, playerID);
        GameData.PlayerPosition.Remove(playerID);
        Destroy(gameObject);
    }

    void use_potion(JSONClass packet)
    {
        var baseClass = gameObject.GetComponent<BaseClass>();

        var damage = packet["Damage"].AsInt;
        var armour = packet["Armour"].AsInt;
        var health = packet["Health"].AsInt;
        var speed = packet["Speed"].AsInt;
        var duration = packet["Duration"].AsInt;

        baseClass.UsePotion(damage, armour, health, speed, duration);
    }
}
