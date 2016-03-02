using UnityEngine;
using System.Collections;

public abstract class BaseClass : MonoBehaviour {

	/* Name of the class. Ex: "Archer, warrior.." */
	protected string _className;

    /* Short description of the class.*/
    protected string _classDescription;

    /* Base stats that all classes share*/
    protected PlayerBaseStat _classStat = new PlayerBaseStat();

    public int team { get; set; }
	
	
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

    public abstract float basicAttack();
    public abstract float[] specialAttack();

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
