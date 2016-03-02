using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Globalization;

public class HUD_Manager : MonoBehaviour {
	[System.Serializable]
	public class PlayerProfile 	{ public Image Health;		public Animator HealthAnimator; }
	[System.Serializable]
	public class AllyKing 		{ public Image Health;		public Animator HealthAnimator; }
	[System.Serializable]
	public class EnemyKing 		{ public Image Health;		public Animator HealthAnimator; }
	[System.Serializable]
	public class Currency 		{ public Text  Amount;		public Animator CurrencyAnimator; }
	[System.Serializable]
	public class MainSkill 		{ public Image ProgressBar;	public float CoolDown; }
	[System.Serializable]
	public class SubSkill 		{ public Image ProgressBar;	public float CoolDown; }
	[System.Serializable]
	public class PassiveSkill 	{ public Image ProgressBar;	public float CoolDown; }
	
	public static HUD_Manager 	instance; 

	public Currency				currency;
	public PlayerProfile 		playerProfile;
	public AllyKing				allyKing;
	public EnemyKing			enemyKing;
	public MainSkill			mainSkill;
	public SubSkill				subSkill;
	public PassiveSkill			passiveSkill;

	void Awake()
	{
		if (instance == null)				//Check if instance already exists
			instance = this;				//if not, set instance to this
		else if (instance != this)			//If instance already exists and it's not this:
			Destroy(gameObject);   			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 
		DontDestroyOnLoad(gameObject);		//Sets this to not be destroyed when reloading scene
	}

	void Update()
	{
		mainSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, mainSkill.ProgressBar.fillAmount);
		if(mainSkill.ProgressBar.fillAmount  < 1)
			mainSkill.ProgressBar.fillAmount  += Time.deltaTime / mainSkill.CoolDown;

		subSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, subSkill.ProgressBar.fillAmount);
		if(subSkill.ProgressBar.fillAmount < 1)
			subSkill.ProgressBar.fillAmount += Time.deltaTime / subSkill.CoolDown;

		passiveSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, passiveSkill.ProgressBar.fillAmount);
		if(passiveSkill.ProgressBar.fillAmount < 1)
			passiveSkill.ProgressBar.fillAmount += Time.deltaTime / passiveSkill.CoolDown;
	}

	public void UpdatePlayerHealth(float Dmg)
	{
		playerProfile.Health.fillAmount += Dmg;
		playerProfile.HealthAnimator.SetTrigger("TakeDmg");
	}

	public void UpdateAllyKingHealth(float Dmg)
	{
		allyKing.Health.fillAmount += Dmg;
		allyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	public void UpdateEnemyKingHealth(float Dmg)
	{
		enemyKing.Health.fillAmount += Dmg;
		enemyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	public void UpdateCurrency(int amount)
	{
		int num = int.Parse(currency.Amount.text, NumberStyles.AllowThousands) + amount;
		currency.Amount.text = String.Format("{0:n0}", num);
		currency.CurrencyAnimator.SetTrigger("UpdateCurrency");
	}

	public void UseMainSkill(float CoolDown)
	{
		if(subSkill.ProgressBar.fillAmount >= 1)
		{
			mainSkill.ProgressBar.fillAmount = 0f;
			mainSkill.CoolDown = CoolDown;
		}
	}

	public void UseSubSkill(float CoolDown)
	{
		if(subSkill.ProgressBar.fillAmount >= 1)
		{
			subSkill.ProgressBar.fillAmount = 0f;
			subSkill.CoolDown = CoolDown;
		}
	}

	public void UsePassive(float Cooldown)
	{
		if(passiveSkill.ProgressBar.fillAmount >= 1)
		{
			passiveSkill.ProgressBar.fillAmount = 0f;
			passiveSkill.CoolDown = Cooldown;
		}
	}
}
