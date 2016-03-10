using UnityEngine;
using System.Collections;
using SimpleJSON;

public abstract class BaseClass : MonoBehaviour {
    //Cooldowns
    public float[] cooldowns { get; protected set; }

	/* Name of the class. Ex: "Archer, warrior.." */
	protected string _className;

    /* Short description of the class.*/
    protected string _classDescription;

    /* Base stats that all classes share*/
    protected PlayerBaseStat _classStat = new PlayerBaseStat();

    public int team;
    public int playerID;
    private int yourPlayerID;
    private int allyKingID;
    private int enemyKingID;

    void Start ()
    {
        var networkingManager = GameObject.Find("GameManager").GetComponent<NetworkingManager>();
        yourPlayerID = networkingManager.player.GetComponent<BaseClass>().playerID;
        allyKingID = GameData.AllyKingID;
        enemyKingID = GameData.EnemyKingID;

        NetworkingManager.Subscribe(receiveAttackFromServer, DataType.Trigger, playerID);

        HUD_Manager.instance.subSkill.CoolDown = cooldowns[0];
        HUD_Manager.instance.mainSkill.CoolDown = cooldowns[1];
        HUD_Manager.instance.playerProfile.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
        if (playerID == allyKingID)
            HUD_Manager.instance.allyKing.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
        if (playerID == enemyKingID)
            HUD_Manager.instance.enemyKing.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
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
            this._classStat.Defense = value.Defense;
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
        
        if (yourPlayerID == playerID) {
            HUD_Manager.instance.UpdatePlayerHealth(-(damage / ClassStat.MaxHp));
        }

        if (playerID == allyKingID) {
            HUD_Manager.instance.UpdateAllyKingHealth(-(damage / ClassStat.MaxHp));
        }

        if (playerID == enemyKingID) {
            HUD_Manager.instance.UpdateEnemyKingHealth(-(damage / ClassStat.MaxHp));
        }

        //gameObject.GetComponentInChildren<Transform>().localScale = new Vector3(.5f, 1, 1);

        if (ClassStat.CurrentHp <= 0.0f)
        {
            //death
            Destroy(gameObject);
        }

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
            doDamage(attack.damage);
        }

    }

    void receiveAttackFromServer(JSONClass playerData)
    {
        if (playerData["ID"] == GameData.MyPlayerID)
            return;
        Vector2 directionOfAttack = new Vector2(playerData["DirectionX"].AsFloat, playerData["DirectionY"].AsFloat);
        switch (playerData["Attack"].AsInt)
        {
            case 0:
                HUD_Manager.instance.UseMainSkill(cooldowns[0]);
                basicAttack(directionOfAttack);
                //Regular attack
                break;
            case 1:
                HUD_Manager.instance.UseSubSkill(cooldowns[1]);
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

    public virtual float basicAttack(Vector2 dir)
    {
        HUD_Manager.instance.UseMainSkill(cooldowns[0]);
        return cooldowns[0];
    }

    public virtual float specialAttack(Vector2 dir)
    {
        HUD_Manager.instance.UseSubSkill(cooldowns[1]);
        return cooldowns[1];
    }

    [System.Serializable]
	public class PlayerBaseStat
	{
		public float CurrentHp;
		public float MaxHp;
		public float MoveSpeed;
		public float AtkPower;
        public float Defense;
        //TODO: defensive stats, etc.
	}
}
