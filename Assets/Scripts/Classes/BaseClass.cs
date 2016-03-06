using UnityEngine;
using System.Collections;
using SimpleJSON;

public abstract class BaseClass : MonoBehaviour {

	/* Name of the class. Ex: "Archer, warrior.." */
	protected string _className;

    /* Short description of the class.*/
    protected string _classDescription;

    /* Base stats that all classes share*/
    protected PlayerBaseStat _classStat = new PlayerBaseStat();

    public int team;
    public int playerID;

    void Start ()
    {
        NetworkingManager.Subscribe(receiveAttackFromServer, DataType.Trigger, playerID);
    }
	
	
	public string ClassName
	{
		get { return this._className; }
		set { this._className = value;}
	}

	public string ClassDescription
	{
		get { return this._classDescription; }
		set { this._classDescription = value;}
	}

	public PlayerBaseStat ClassStat
	{
		get
        {
            if (this._classStat == null)
            {
                Debug.Log("Classstat was not set");
                this._classStat = new PlayerBaseStat();
            }
            return this._classStat;
        }

		protected set
		{
			this._classStat.CurrentHp = value.CurrentHp;
			this._classStat.MaxHp = value.MaxHp;
			this._classStat.MoveSpeed = value.MoveSpeed;
			this._classStat.AtkPower = value.AtkPower;
		}
	}

    public float doDamage(float damage)
    {
        //TODO: add defense to calculation
        ClassStat.CurrentHp -= damage;
        if(ClassStat.CurrentHp > ClassStat.MaxHp)
        {
            ClassStat.CurrentHp = ClassStat.MaxHp;
        }
        Debug.Log(ClassStat.CurrentHp + "/" + ClassStat.MaxHp + " HP");
        return ClassStat.CurrentHp;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var attack = other.gameObject.GetComponent<Trigger>();

        if (attack != null)
        {
           
            if (attack.teamID == team)
            {
                return;
            }
            if (doDamage(attack.damage) <= 0.0f)
            {
                //death
                Destroy(gameObject);
            }
        }

    }

    void receiveAttackFromServer(JSONClass playerData)
    {
        Vector2 directionOfAttack = new Vector2(playerData["DirectionX"].AsFloat, playerData["DirectionY"].AsFloat);
        switch (playerData["Attack"].AsInt)
        {
            case 0:
                basicAttack(directionOfAttack);
                //Regular attack
                break;
            case 1:
                specialAttack(directionOfAttack);
                //Regular special attack
                break;
            case 2:
                //Aman special attack
                break;
            default:
                break;
        }
    }

    public abstract float basicAttack(Vector2 dir);
    public abstract float[] specialAttack(Vector2 dir);

    [System.Serializable]
	public class PlayerBaseStat
	{
		public float CurrentHp;
		public float MaxHp;
		public float MoveSpeed;
		public float AtkPower;
        //TODO: defensive stats, etc.
	}
}
