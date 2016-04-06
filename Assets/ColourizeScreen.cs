using UnityEngine;
using System.Collections;

public class ColourizeScreen : MonoBehaviour {
    private GameObject death { get; set; }
    private GameObject injured { get; set; }
    private GameObject healed { get; set; }
    private float injuryDuration = .1f;
    private float healedDuration = .15f;

    

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
                case "Healed":
                    healed = child.gameObject;
                    break;
                default:
                    break;
            }
        }
        death.SetActive(false);
        injured.SetActive(false);
        healed.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlayerHurt()
    {
        healed.SetActive(false);
        injured.SetActive(true);
        Invoke("UndoPlayerHurt", injuryDuration);
    }

    public void PlayerDied()
    {
        death.SetActive(true);
    }

    public void PlayerHealed()
    {
        injured.SetActive(false);
        healed.SetActive(true);
        Invoke("UndoPlayerHealed", healedDuration);
    }

    public void PlayerRevived()
    {
        death.SetActive(false);
    }

    private void UndoPlayerHurt()
    {
        injured.SetActive(false);
    }

    private void UndoPlayerHealed()
    {
        healed.SetActive(false);
    }
}
