using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadingBehavior : MonoBehaviour
{

    public FoodBehavior.BreadingType breading;
    private ParticleSystem particles;


    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other) {
        if(other.tag == "food") {
            particles.Play();
        }
    }
}
