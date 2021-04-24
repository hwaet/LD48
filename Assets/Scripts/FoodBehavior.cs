using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FoodBehavior : MonoBehaviour
{
    public enum Doneness {
        Raw,
        Cooked,
        Burnt
    }

    public string FoodType;
    public float cookTime;
    public Doneness doneness;
    public bool cooking = false;

    public List<string> BreadingLayers = new List<string>();
    public List<FoodBehavior> Stuffings = new List<FoodBehavior>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCook() {
        StartCoroutine(Cooking());
    }

    public void StopCook() {
        cooking = false;
    }

    public void Stuff(FoodBehavior other) {
        Stuffings.Add(other);
    }

    private void OnTriggerEnter(Collider other) {
       switch(other.tag) {
            case "frier":
                StartCook();
                break;
            case "egg":
            case "bread":
            case "spice":
                BreadingLayers.Add(other.tag);
                break;
            case "service":
                break;

        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "frier") {
            StopCook();
        }
    }

    IEnumerator Cooking() {
        float startTime = Time.time;
        cooking = true;
        while(cooking) {
            switch (doneness) {
                case Doneness.Raw:
                    if(Time.time - startTime > cookTime) {
                        doneness = Doneness.Cooked;
                    }
                    break;
                case Doneness.Cooked:
                    if (Time.time - startTime > cookTime * 2) {
                        doneness = Doneness.Burnt;
                    }
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

}
