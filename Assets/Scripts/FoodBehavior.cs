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

    private Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hold() {
        StartCoroutine(HoldCoroutine());
    }

    private IEnumerator HoldCoroutine() {
        while(this.rigidbody == null) {
            yield return new WaitForEndOfFrame();
        }
        this.rigidbody.useGravity = false;
        this.rigidbody.isKinematic = true;
        yield break;
    }

    public void Release() {
        this.rigidbody.useGravity = true;
        this.rigidbody.isKinematic = false;
    }

    public void StartCook() {
        Debug.Log("Start Cooking");
        StartCoroutine(Cooking());
    }

    public void StopCook() {
        Debug.Log("Stop Cooking");
        cooking = false;
    }

    public void Stuff(FoodBehavior other) {
        Stuffings.Add(other);
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.transform.tag == "plate") {
            this.rigidbody.isKinematic = true;
            this.transform.parent = collision.transform;
        }

    }

    private void OnTriggerEnter(Collider other) {
       switch(other.tag) {
            case "oil":
                StartCook();
                break;
            case "egg":
            case "bread":
            case "spice":
                Debug.LogFormat("Adding {0} to Breading Layers", other.tag);
                BreadingLayers.Add(other.tag);
                break;
            case "delivery":
                Debug.LogFormat("Delivered: {0} {1} with: ", doneness, this.FoodType, this.BreadingLayers.ToString());
                Destroy(this.gameObject);
                break;

        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "oil") {
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
                        Debug.Log("Cooked");
                    }
                    break;
                case Doneness.Cooked:
                    if (Time.time - startTime > cookTime * 2) {
                        doneness = Doneness.Burnt;
                        Debug.Log("Burnt");
                    }
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

}
