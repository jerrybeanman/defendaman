using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour {

	
	private Animator _animator;

	public void Awake()
	{
		_animator = GetComponent<Animator>();
		PlayerEvents.OnHit += this.OnHit;
		PlayerEvents.OnMove += this.OnMove;
		
	}

	public void Destroy()
	{
		PlayerEvents.OnHit -= this.OnHit;
		PlayerEvents.OnMove -= this.OnMove;
		
	}

	public void OnHit()
	{

	}

	public void OnMove()
	{

	}


}
