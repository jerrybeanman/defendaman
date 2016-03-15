using UnityEngine;
using System.Collections;

public class ColourizeScreen : MonoBehaviour {
    private GameObject death { get; set; }
    private GameObject injured { get; set; }
    private float injuryDuration = .1f;

    public static ColourizeScreen instance;
    
    void Awake()
    {
        if (instance == null)        
            instance = this;               
        else if (instance != this)          
            Destroy(gameObject);   			
    }

	// Use this for initialization
	void Start () {
        foreach(Transform child in transform)
        {
            switch(child.name)
            {
                case "Death":
                    death = child.gameObject;
                    break;
                case "Injured":
                    injured = child.gameObject;
                    break;
                default:
                    break;
            }
        }
        death.SetActive(false);
        injured.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlayerHurt()
    {
        injured.SetActive(true);
        Invoke("UndoPlayerHurt", injuryDuration);
    }

    public void PlayerDied()
    {
        death.SetActive(true);
    }

    public void PlayerRevived()
    {
        death.SetActive(false);
    }

    private void UndoPlayerHurt()
    {
        injured.SetActive(false);
    }
}
