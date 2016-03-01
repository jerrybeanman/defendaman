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

    public float damaged(float damage)
    {
        //TODO: add defense to calculation
        this._classStat.CurrentHp -= damage;
        return this._classStat.CurrentHp;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var attack = other.gameObject.GetComponent<Trigger>();
        if (attack.teamID == team)
        {
            return;
        }
        if (damaged(attack.damage) <= 0.0f)
        {
            //death
            Destroy(gameObject);
        }
        Debug.Log("I got hit");
    }


    [System.Serializable]
	public class PlayerBaseStat
	{
		public float CurrentHp;
		public float MaxHp;
		public float MoveSpeed;
		public float AtkPower;
        //TODO: defensive stat
	}
}
