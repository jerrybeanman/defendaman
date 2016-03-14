using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Globalization;
using SimpleJSON;

public class HUD_Manager : MonoBehaviour {
	#region Subclasses
	[System.Serializable]
	public class PlayerProfile 	{ public Image Health;			public Animator HealthAnimator; 	}
	[System.Serializable]
	public class AllyKing 		{ public Image Health;			public Animator HealthAnimator; 	}
	[System.Serializable]
	public class EnemyKing 		{ public Image Health;			public Animator HealthAnimator; 	}
	[System.Serializable]
	public class Currency 		{ public Text  Amount;			public Animator CurrencyAnimator; 	}
	[System.Serializable]
	public class MainSkill 		{ public Image ProgressBar;		public float CoolDown; 				}
	[System.Serializable]
	public class SubSkill 		{ public Image ProgressBar;		public float CoolDown; 				}
	[System.Serializable]
	public class PassiveSkill 	{ public Image ProgressBar;		public float CoolDown; 				}
	[System.Serializable]
	public class MessageHistory { public GameObject Container; 	public GameObject AllyMessage; public GameObject EnemyMessage; }
	#endregion

	// Singleton object
	public static HUD_Manager 	instance; 

	// HUD elements
	public Currency				currency;
	public PlayerProfile 		playerProfile;
	public AllyKing				allyKing;
	public EnemyKing			enemyKing;
	public MainSkill			mainSkill;
	public SubSkill				subSkill;
	public PassiveSkill			passiveSkill;
	public MessageHistory		messageHistory;

	// Singleton pattern
	void Awake()
	{
		if (instance == null)				//Check if instance already exists
			instance = this;				//if not, set instance to this
		else if (instance != this)			//If instance already exists and it's not this:
			Destroy(gameObject);   			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 
		DontDestroyOnLoad(gameObject);		//Sets this to not be destroyed when reloading scene
	}

	void Start()
	{
		// NOTE:: For testing purposes
		GameData.MyPlayer.TeamID = 1;

		NetworkingManager.Subscribe(UpdateChatCallBack, DataType.UI, 1);
	}

	void UpdateChatCallBack(JSONClass data)
	{
		int team = data[NetworkKeyString.TeamID].AsInt;
		string username = data[NetworkKeyString.UserName];
		string message = data[NetworkKeyString.Message];

		UpdateChat(team);
	}

	public void UpdateChat(int team)
	{
		// NOTE:: For Testing purposes
		string username= "[Dong]"; string message= "[herro]";
		GameObject childObject;
		// Ally Message
		if(team == GameData.MyPlayer.TeamID)
		{
			foreach(Transform child in messageHistory.AllyMessage.transform)
			{
				if(child.name == "Name")
					child.GetComponent<Text>().text = username;
				else
					child.GetComponent<Text>().text = message;
			}
			childObject = Instantiate (messageHistory.AllyMessage) as GameObject;								//Instantitate arrow
			childObject.transform.SetParent (messageHistory.Container.transform, false);						//Make arrow a child object of InputHistory
		}else
		{
			foreach(Transform child in messageHistory.EnemyMessage.transform)
			{
				if(child.name == "Name")
					child.GetComponent<Text>().text = username;
				else
					child.GetComponent<Text>().text = message;
			}
			childObject = Instantiate (messageHistory.EnemyMessage) as GameObject;								//Instantitate arrow
			childObject.transform.SetParent (messageHistory.Container.transform, false);				//Make arrow a child object of InputHistory
		}
	}


	// For rechargin skills whenever they are used
	void Update()
	{

		if(mainSkill.ProgressBar.fillAmount  < 1)
		{
			mainSkill.ProgressBar.fillAmount  += Time.deltaTime / mainSkill.CoolDown;
			mainSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, mainSkill.ProgressBar.fillAmount);
		}
			
		if(subSkill.ProgressBar.fillAmount < 1)
		{
			subSkill.ProgressBar.fillAmount += Time.deltaTime / subSkill.CoolDown;
			subSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, subSkill.ProgressBar.fillAmount);
		}
	}
	
	/*----------------------------------------------------------------------------
    --	Update player hp on the HUD, and triggers the "TakeDmg" animation
    --
    --	Interface:  public void UpdatePlayerHealth(float Dmg)
    --	            [dmg] Amount of HP added or removed, has to be less than 1. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdatePlayerHealth(float Dmg)
	{
		if((Dmg < 0 && playerProfile.Health.fillAmount <= 0) || (Dmg > 0 && playerProfile.Health.fillAmount >= 1))
			return;
		playerProfile.Health.fillAmount += Dmg;
		playerProfile.HealthAnimator.SetTrigger("TakeDmg");
	}

	/*----------------------------------------------------------------------------
    --	Update ally king's hp on the HUD, and triggers the "TakeDmg" animation
    --
    --	Interface:  public void UpdateAllyKingHealth(float Dmg)
    --	            [dmg] Amount of HP added or removed, has to be less than 1. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateAllyKingHealth(float Dmg)
	{
		if((Dmg < 0 && allyKing.Health.fillAmount <= 0) || (Dmg > 0 && allyKing.Health.fillAmount >= 1))
			return;
		allyKing.Health.fillAmount += Dmg;
		allyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	/*----------------------------------------------------------------------------
    --	Update enemy king's hp on the HUD, and triggers the "TakeDmg" animation
    --
    --	Interface:  public void UpdateEnemyKingHealth(float Dmg)
    --	            [dmg] Amount of HP added or removed, has to be less than 1. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateEnemyKingHealth(float Dmg)
	{
		if((Dmg < 0 && enemyKing.Health.fillAmount <= 0) || (Dmg > 0 && enemyKing.Health.fillAmount >= 1))
			return;
		enemyKing.Health.fillAmount += Dmg;
		enemyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	/*----------------------------------------------------------------------------
    --	Update currency value on the HUD, and triggers the "UpdateCurrency" animation
    --
    --	Interface:  public void UpdateCurrency(float amount)
    --	            [amount] Amount of currency added or removed. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateCurrency(int amount)
	{
		if((amount < 0 && int.Parse(currency.Amount.text) <= 0))
			return;
		int num = int.Parse(currency.Amount.text, NumberStyles.AllowThousands) + amount;
		currency.Amount.text = String.Format("{0:n0}", num);
		currency.CurrencyAnimator.SetTrigger("UpdateCurrency");
	}

	/*----------------------------------------------------------------------------
    --	Sets fill amount of main skill's progress bar to empty whenever a skill is 
    --  being used. The fill amount will be recharged in Update() corresponding to 
    --  the CoolDown value
    --
    --	Interface:  public void UseMainSkill(float CoolDown)
    --	            [CoolDown] Skill cool down value in seconds. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UseMainSkill(float CoolDown)
	{
		if(mainSkill.ProgressBar.fillAmount >= 1)
		{
			mainSkill.ProgressBar.fillAmount = 0f;
			mainSkill.CoolDown = CoolDown;
		}
	}

	/*----------------------------------------------------------------------------
    --	Sets fill amount of sub skill's progress bar to empty whenever a skill is 
    --  being used. The fill amount will be recharged in Update() corresponding to 
    --  the CoolDown value
    --
    --	Interface:  public void UseSubSkill(float CoolDown)
    --	            [CoolDown] Skill cool down value in seconds. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UseSubSkill(float CoolDown)
	{
		if(subSkill.ProgressBar.fillAmount >= 1)
		{
			subSkill.ProgressBar.fillAmount = 0f;
			subSkill.CoolDown = CoolDown;
		}
	}
}
