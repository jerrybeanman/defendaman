using UnityEngine;
using System.Collections;

public class BaseClass{

	/* Name of the class. Ex: "Archer, warrior.." */
	private string _className;

	/* Short description of the class.*/
	private string _classDescription;

	/* Base stats that all classes share*/
	private PlayerBaseStat _classStat;
	
	
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

	private PlayerBaseStat ClassStat
	{
		get {return this._classStat; }
		set
		{
			this._classStat.CurrentHp = value.CurrentHp;
			this._classStat.MaxHp = value.MaxHp;
			this._classStat.MoveSpeed = value.MoveSpeed;
			this._classStat.AtkPower = value.AtkPower;
		}
	}




	[System.Serializable]
	public class PlayerBaseStat
	{
		public float CurrentHp;
		public float MaxHp;
		public float MoveSpeed;
		public float AtkPower;
	}
}
