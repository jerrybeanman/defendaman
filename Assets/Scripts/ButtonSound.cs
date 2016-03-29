using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour {


    public AudioClip startSound;
    public AudioClip backSound;
    public AudioClip readySound;
    public AudioClip clickSound;
//    AudioClip Sound;

    private Button button { get { return GetComponent<Button>(); } }
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

	// Use this for initialization
	void Start () {
        
	    gameObject.AddComponent<AudioSource>();
        //source.clip = Sound;
        source.playOnAwake = false;
        /*
        backSound = (AudioClip) Resources.Load("../Sounds/UI/ui_back.wav");

        startSound = (AudioClip)Resources.Load("/Assets/Sounds/UI/ui_start.wav");
        readySound = (AudioClip)Resources.Load("/Assets/Sounds/UI/ui_ready.wav");
        clickSound = (AudioClip)Resources.Load("/Sounds/UI/ui_click.wav");
*/
    }
    /*
	public void PlaySound() {
        source.PlayOneShot(Sound);
        //Sleep(1);
    }
    */
    public void startSoundPlay()
    {
        source.clip = startSound;
        source.PlayOneShot(startSound);
    }
    public void backSoundPlay()
    {
        source.clip = backSound;
        source.PlayOneShot(backSound);
    }
    public void readySoundPlay()
    {
        source.clip = readySound;
        source.PlayOneShot(readySound);
    }
    public void clickSoundPlay()
    {
        source.clip = clickSound;
        source.PlayOneShot(clickSound);
    }
}
