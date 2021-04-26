using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour {

	public bool fadeOut=false;
	public float fadeOutTime = 1f;
	public bool fadeIn=false;
	public float fadeInTime = 2f;
	//public Settings globalSettings;

	float origVolume;
	float maxVolume;
	AudioSource audioSource;

	// Use this for initialization
	void Awake () {
		audioSource = GetComponent<AudioSource>();
	}

	void Start() {
		fadeIn=true;
	}

	private void OnEnable()
	{
		SceneWrangler.fadeMusic += startFadeOutWrapper;
	}

	private void OnDisable()
	{
		SceneWrangler.fadeMusic -= startFadeOutWrapper;
	}

	// Update is called once per frame
	void Update () {
		if (audioSource!=null) {
			if (fadeOut==true) {
				StartCoroutine("startFadeOut");
			}
			if (fadeIn==true) {
				StartCoroutine("startFadeIn");
			}
		}

		updateVolume();
	}

	public void updateVolume() {
		//if (globalSettings!=null) {
		//	maxVolume = globalSettings.musicVolume;
		maxVolume = 1;
		if ((fadeIn==false) &&  (fadeIn==false)) {
			audioSource.volume = maxVolume;
		}
		//}
	}

	public void startFadeOutWrapper() {
		fadeOut=true;
	}

	public void startFadeInWrapper() {
		print ("fadeIn");
		fadeIn=true;
	}

	IEnumerator startFadeOut() {
		fadeOut=false;
		DontDestroyOnLoad(this.gameObject);

		for (float a=maxVolume; a>=0; a=a-(Time.deltaTime/fadeOutTime)) {
			audioSource.volume = a;
			yield return null;
		}
			
		Object.Destroy(this.gameObject);
	}

	IEnumerator startFadeIn() {
		fadeIn=false;
		origVolume = maxVolume;
		audioSource.volume = 0f;

		for (float a=0; a<=origVolume; a=a+(Time.deltaTime/fadeInTime)) {
			audioSource.volume = a;
			yield return null;
		}
	}
}
