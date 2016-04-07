using UnityEngine;
using System.Collections;

public class ColourizeScreen : MonoBehaviour {
	public GameObject death;
	public GameObject injured;
	public GameObject healed;
    private float injuryDuration = .1f;
    private float healedDuration = .15f;


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
