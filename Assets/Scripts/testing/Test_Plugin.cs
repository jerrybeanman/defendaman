using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public struct PlayerStuff
{
	public int Dexterity;
	public float MaxHealth;
	[MarshalAs(UnmanagedType.U1)]	
	public bool IsHit;
}

public class Test_Plugin : MonoBehaviour {
	[DllImport("MySharedLibrary.so", CallingConvention = CallingConvention.Cdecl)]
	private static extern PlayerStuff getPlayerStuff();

	[DllImport("MySharedLibrary.so")]
	private static extern int gettwo();
	
	/* Drag your first sprite here */ 
	public Sprite sprite1; 
	/* Drag your second sprite here */
	public Sprite sprite2;

	private SpriteRenderer spriteRenderer;

	/* PlayerStuff is the same structure in C++ */
	private PlayerStuff _playerStuff;

	/* First thing that is called when the scene runs */
	void Awake()
	{
		/* Initialize the PlayerStuff structure through C++ wrapper function */
		_playerStuff = getPlayerStuff();
	}

	void Start()
	{
		/* we are accessing the SpriteRenderer that is attached to the Gameobject */
		spriteRenderer = GetComponent<SpriteRenderer>();

		/* if the sprite on spriteRenderer is null */
		if (spriteRenderer.sprite == null)
			/* set the sprite to sprite1 */
			spriteRenderer.sprite = sprite1;
	}

	
	void Update()
	{
		/* If the space bar is pushed down */
		if (Input.GetKeyDown(KeyCode.Space)) 
		{
			/* Change sprite */
			ChangeTheDarnSprite(); 
		}
	}

	/* Gets called for rendering and handling GUI events */
	void OnGUI()
	{
		/* Displays _playerStuff as labels */
		PrintPlayerStuff();
	}

	void ChangeTheDarnSprite()
	{
		/* Testing for C++ wrapper function, creates a child process and returns 2 */
		if (gettwo() == 2)
		{
			if (spriteRenderer.sprite == sprite1) // if the spriteRenderer sprite = sprite1 then change to sprite2
			{
				spriteRenderer.sprite = sprite2;
			}
			else
			{
				spriteRenderer.sprite = sprite1; // otherwise change it back to sprite1
			}
		}
	}

	void PrintPlayerStuff()
	{


		/* Display Dexterity*/
		GUI.Label(new Rect(10, 10, 100, 20), _playerStuff.Dexterity.ToString());
		Debug.Log(_playerStuff.Dexterity);

		/* Display MaxHealth */
		GUI.Label(new Rect(10, 30, 100, 20), _playerStuff.MaxHealth.ToString());
		Debug.Log(_playerStuff.MaxHealth);

		/* Display if the player is hit or not */
		GUI.Label(new Rect(10, 50, 100, 20), (_playerStuff.IsHit) ? "Hit\n" : "Not Hit\n");
		Debug.Log((_playerStuff.IsHit) ? "Hit\n" : "Not Hit\n");
		
	}
}
