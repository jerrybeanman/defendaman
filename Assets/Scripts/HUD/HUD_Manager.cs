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

	public static HUD_Manager 	instance; 

	public Currency				currency;
	public PlayerProfile 		playerProfile;
	public AllyKing				allyKing;
	public EnemyKing			enemyKing;

	void Awake()
	{
		if (instance == null)				//Check if instance already exists
			instance = this;				//if not, set instance to this
		else if (instance != this)			//If instance already exists and it's not this:
			Destroy(gameObject);   			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 
		DontDestroyOnLoad(gameObject);		//Sets this to not be destroyed when reloading scene
	}

	public void PlayerTakeDmg(float Dmg)
	{
		playerProfile.Health.fillAmount -= Dmg;
		playerProfile.HealthAnimator.SetTrigger("TakeDmg");
	}

	public void AllyKingTakeDmg(float Dmg)
	{
		allyKing.Health.fillAmount -= Dmg;
		allyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	public void EnemyKingTakeDmg(float Dmg)
	{
		enemyKing.Health.fillAmount -= Dmg;
		enemyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	public void UpdateCurrency(int amount)
	{
		int num = int.Parse(currency.Amount.text, NumberStyles.AllowThousands) + amount;
		currency.Amount.text = String.Format("{0:n0}", num);
		currency.CurrencyAnimator.SetTrigger("UpdateCurrency");
	}
}
