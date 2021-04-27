using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Grabbable), typeof(AudioSource))]
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
    //[HideInInspector()]
    public List<BreadingType> BreadingLayers; 

    private new Rigidbody rigidbody;
    private new Collider collider;
    private Grabbable grabbable;
    private AudioSource audioSource;
    private ParticleSystem whiteSmoke;
    private ParticleSystem blackSmoke;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
        this.collider = GetComponent<Collider>();
        this.materials = gameObject.GetComponent<MeshRenderer>().materials.ToList();
        foreach(Transform child in transform) {
            
            if(child.name == "whiteSmokeParticles") {
                whiteSmoke = child.GetComponent<ParticleSystem>();
            }
            else if(child.name == "blackSmokeParticles") {
                blackSmoke = child.GetComponent<ParticleSystem>();
            }
            else { 
                this.materials.AddRange(child.GetComponent<MeshRenderer>().materials.ToList());
            }
        }
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
        if(collision.gameObject.layer == 9) {
            audioSource.Play();
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

    //Code copied from: https://www.arduino.cc/reference/en/language/functions/math/map/
    float map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    IEnumerator Cooking() {
        float startTime = Time.time;
        cooking = true;
        float bouancy = map(this.transform.position.y, -.5f, -1.1f, 0, 1.3f);
        while(cooking) {
            cookingValue += Time.deltaTime;
            UpdateMaterial();
            switch (doneness) {
                case Doneness.Raw:
                    //impulse *= Random.Range(1, 1.1f);
                    if(cookingValue > cookTime) {
                        doneness = Doneness.Cooked;
                        Debug.Log("Cooked");
                        whiteSmoke?.Play();
                    }
                    break;
                case Doneness.Cooked:
                    //impulse *= Random.Range(1, 1.05f);
                    if (cookingValue > cookTime * 2) {
                        doneness = Doneness.Burnt;
                        Debug.Log("Burnt");
                        whiteSmoke?.Stop();
                        blackSmoke?.Play();
                    }
                    break;
                case Doneness.Burnt:
                    bouancy = 0;
                    break;
            }

            rigidbody.AddForce(-Physics.gravity * bouancy, ForceMode.Acceleration);

            if (Time.time - startTime > cookTime * 5) {
                Destroy(this.gameObject);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void UpdateMaterial()
    {
        foreach (Material material in materials)
        {
            material.SetFloat("cookedState", cookingValue / (cookTime*2));
        }
    }


    public override string ToString() {
        return string.Format("{0} {1} with {2}", doneness, foodType, string.Join(",",BreadingLayers));
    }
}
