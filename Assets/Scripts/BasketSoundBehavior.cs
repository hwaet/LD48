using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketSoundBehavior : MonoBehaviour
{

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
        
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == 9) {
            audioSource.Play();
        }
    }
}
