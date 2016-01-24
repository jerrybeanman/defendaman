using UnityEngine;
using System.Collections;

public class PlayerEvents : MonoBehaviour 
{

	public delegate void PlayerHandler();

	public static event PlayerHandler OnHit;
	public static event PlayerHandler OnMove;

	public static void PlayerHit()
	{
		if(OnHit != null)
			OnHit();
	}

	public static void PlayerMove()
	{
		if(OnHit != null)
			OnMove();
	}
}