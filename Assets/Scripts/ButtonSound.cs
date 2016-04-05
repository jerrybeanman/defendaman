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

        source.playOnAwake = false;

        backSound = (AudioClip)Resources.Load("Music/UI/ui_back");


        startSound = (AudioClip)Resources.Load("Music/UI/ui_start");
        readySound = (AudioClip)Resources.Load("Music/UI/ui_ready");
        clickSound = (AudioClip)Resources.Load("Music/UI/ui_click");

    }


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
