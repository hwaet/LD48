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
        None,
        Chicken,
        Duck,
        Turkey,
        Ducken,
        Turducken
    }

    public enum BreadingType {
        Fresh,
        Breading,
        Crispy,
        Spicy
    }

    public FoodType foodType;
    public float cookTime;
    public Doneness doneness;

    float cookingValue;
    List<Material> materials;
    
    [HideInInspector()]
    public bool cooking = false;
    [HideInInspector()]
    public List<BreadingType> BreadingLayers; 

    private new Rigidbody rigidbody;
    private new Collider collider;
    private Grabbable grabbable;

    // Start is called before the first frame update
    void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
        this.collider = GetComponent<Collider>();
        this.materials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
        this.grabbable = GetComponent<Grabbable>();
        this.BreadingLayers = new List<BreadingType> { BreadingType.Fresh };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCook() {
        Debug.Log("Start Cooking");
        StartCoroutine(Cooking());
    }

    public void StopCook() {
        Debug.Log("Stop Cooking");
        cooking = false;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.tag == "plate") {
            this.rigidbody.isKinematic = true;
            this.transform.parent = collision.transform;
        }
    }

    private void OnTriggerEnter(Collider other) {
       switch(other.tag) {
            case "oil":
                StartCook();
                break;
            case "breading":
                BreadingBehavior bb = other.GetComponent<BreadingBehavior>();
                if(BreadingLayers[BreadingLayers.Count-1] != bb.breading) {
                    this.BreadingLayers.Add(bb.breading);
                    Debug.LogFormat("Adding {0} to Breading Layers", other.tag);
                }
                
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
            cookingValue += Time.deltaTime;
            UpdateMaterial();
            switch (doneness) {
                case Doneness.Raw:
                    if(cookingValue > cookTime) {
                        doneness = Doneness.Cooked;
                        Debug.Log("Cooked");
                    }
                    break;
                case Doneness.Cooked:
                    if (cookingValue > cookTime * 2) {
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
            material.SetFloat("cookedState", cookingValue / cookTime);
        }
    }


    public override string ToString() {
        return string.Format("{0} {1} with {2}", doneness, foodType, string.Join(",",BreadingLayers));
    }
}
