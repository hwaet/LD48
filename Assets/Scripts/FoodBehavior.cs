using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Grabbable))]
public class FoodBehavior : MonoBehaviour
{
    public enum Doneness {
        Raw,
        Cooked,
        Burnt
    }

    public enum FoodType {
        Chicken,
        Duck,
        Turkey,
        Ducken,
        Turducken
    }

    public FoodType foodType;
    public float cookTime;
    public Doneness doneness;

    float cookingValue;
    List<Material> materials;

    public FoodType[] stuffableWith;
    
    [HideInInspector()]
    public bool cooking = false;
    [HideInInspector()]
    public List<string> BreadingLayers = new List<string>();
    [HideInInspector()]
    public List<FoodBehavior> Stuffings = new List<FoodBehavior>();

    private new Rigidbody rigidbody;
    private new Collider collider;

    // Start is called before the first frame update
    void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
        this.collider = GetComponent<Collider>();
        this.materials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hold(GameObject hand) {
        StartCoroutine(HoldCoroutine(hand));
    }

    private IEnumerator HoldCoroutine(GameObject hand) {
        while(this.rigidbody == null) {
            yield return new WaitForEndOfFrame();
        }
        Collider handCollider = hand.GetComponent<Collider>();
        Physics.IgnoreCollision(this.collider, handCollider, true);
/*
        this.collider.enabled = false;
        this.rigidbody.useGravity = false;
        this.rigidbody.isKinematic = true;
*/
        yield break;
    }

    public void Release(GameObject hand) {
        /*      this.rigidbody.useGravity = true;
              this.rigidbody.isKinematic = false;
              this.collider.enabled = true;*/
        StartCoroutine(ReleaseCoroutine(hand));
    }

    private IEnumerator ReleaseCoroutine(GameObject hand) {
        yield return new WaitForSeconds(.2f);
        Collider handCollider = hand.GetComponent<Collider>();
        Physics.IgnoreCollision(this.collider, handCollider, false);
        yield break;
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
                DeliveryBehavior delivery = other.transform.GetComponent<DeliveryBehavior>();
                delivery.checkOrder(this);
                //Debug.LogFormat("Delivered: {0} {1} with: ", doneness, this.FoodType, this.BreadingLayers.ToString());
                //Destroy(this.gameObject);
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
            cookingValue = (Time.time - startTime) / cookTime;
            UpdateMaterial();
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

    void UpdateMaterial()
    {
        foreach (Material material in materials)
        {
            material.SetFloat("cookedState", cookingValue);
        }
    }

}
