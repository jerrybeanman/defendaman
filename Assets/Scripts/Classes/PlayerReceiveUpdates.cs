using UnityEngine;
using System.Collections;
using SimpleJSON;


//Carson
//Enemy/Allies updating script
public class PlayerReceiveUpdates : MonoBehaviour {
    public int playerID;
    private Vector2 lastPosition;
    BaseClass baseClass;
    Animator animator;

	// Use this for initialization
	void Start () {
        lastPosition = transform.position;
        NetworkingManager.Subscribe(update_position, DataType.Player, playerID);
        NetworkingManager.Subscribe(took_damage, DataType.Hit, playerID);
        NetworkingManager.Subscribe(died, DataType.Killed, playerID);
        NetworkingManager.Subscribe(use_potion, DataType.Potion, playerID);
        NetworkingManager.Subscribe(new_stats, DataType.StatUpdate, playerID);
        GameData.PlayerPosition.Add(playerID, transform.position);
        baseClass = GetComponent<BaseClass>();
        animator = gameObject.GetComponent<Animator>();
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

		//TODO: Figure out why this commented below was not working
        /*if (pos_changed(player["x"].AsFloat, player["y"].AsFloat))
            animator.SetBool("moving", true);
        else
            animator.SetBool("moving", false);
        transform.position.Set(player["x"].AsFloat, player["y"].AsFloat, -10f);
        transform.rotation.Set(0, 0, player["rotationZ"].AsFloat, player["rotationW"].AsFloat);
        GameData.PlayerPosition[playerID] = transform.position;
        lastPosition = transform.position;*/
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
        if (GameData.LobbyData[playerID].TeamID == GameData.MyPlayer.TeamID)
            GameData.EnemyTeamKillCount++;
        else
            GameData.AllyTeamKillCount++;

        NetworkingManager.Unsubscribe(DataType.Player, playerID);
        GameData.PlayerPosition.Remove(playerID);
        Destroy(gameObject);

        if (playerID == GameData.AllyKingID)
        {
            GameManager.instance.GameLost();
        }

        if (playerID == GameData.EnemyKingID)
        {
            GameManager.instance.GameWon();
        }
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

    void new_stats(JSONClass packet)
    {
        var classStat = gameObject.GetComponent<BaseClass>().ClassStat;
        classStat.AtkPower = packet["AtkPower"].AsFloat;
        classStat.Defense = packet["Defense"].AsFloat;
    }
}
