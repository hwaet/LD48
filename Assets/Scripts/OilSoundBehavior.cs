using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSoundBehavior : MonoBehaviour
{
    public int foodsInOil = 0;
    public float fadeDelay = .2f;
    public float fadeTime = 1;
    public float ambientLevel = .1f;
    public float lastFoodExitTime = float.NaN;
    public bool calm = true;


    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (calm) {
            audioSource.volume = ambientLevel;
        }
        else if(foodsInOil > 0) {
            audioSource.volume = 1;
        }
        else {
            if(float.IsNaN(lastFoodExitTime)) {
                lastFoodExitTime = Time.time;
            }
            else if(Time.time - lastFoodExitTime > fadeDelay) {
                audioSource.volume = Mathf.Clamp((Time.time - lastFoodExitTime - fadeDelay) / fadeTime, ambientLevel, 1);
            }
            else if(Time.time -lastFoodExitTime > fadeDelay + fadeTime) {
                calm = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("food")) {
            foodsInOil += 1;
            lastFoodExitTime = float.NaN;
            calm = false;
        }
    }

    private void OnTriggerExit(Collider other) {
        Debug.Log("Oil on Trigger Exit");
        if (other.CompareTag("food")) {
            foodsInOil -= 1;
        }
    }

}
