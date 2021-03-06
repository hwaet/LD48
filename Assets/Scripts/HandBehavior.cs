using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class HandBehavior : MonoBehaviour
{
    public enum Zone {
        Cooler,
        Breading,
        Frier,
        Prep,
        Platting,
        Delivery,
        Trash,
        Other
    }

    public enum Hand {
        Left,
        Right
    }

    public Hand hand;
    public Zone zone;

    public float moveSpeed;
    public float dragSpeed;
    public float pickupSpeed;
    public float hoverHeight;
    public float pickUpDistance;

    public float dumpRotationTime;
    public Quaternion dumpAngle;

    private Grabbable holding = null;
    
    public enum PickupState {
        Idle,
        Seeking,
        Returning
    }

    private PickupState pickupState = PickupState.Idle;
    private bool dumping = false;
    public bool Animating {
        get {
            return pickupState != PickupState.Idle && !dumping;
        }
    }

    public bool HoldingSomething {
        get {
            return holding != null;
        }
    }


    private GameObject pickupTarget = null;

    public new Rigidbody rigidbody;
    public new Collider collider;

    [Header("Layer Masks")]
    public LayerMask frierMask;
    public LayerMask coolerMask;
    public LayerMask foodMask;
    public LayerMask plateMask;
    public LayerMask trashMask;
    private SceneWrangler sceneWrangler;
    private ParticleSystem particleSystem;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        List<SceneWrangler> wranglers = FindObjectsOfType<SceneWrangler>().ToList();
        foreach (SceneWrangler wrangler in wranglers)
        {
            if (wrangler.levelState == LevelLoadingProcess.Idle) this.sceneWrangler = wrangler;
        }

        foreach(Transform child in transform) {
            if(child.name == "taDaParticles") {
                particleSystem = child.GetComponentInChildren<ParticleSystem>();
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Animating) {
            Vector3 vel = rigidbody.velocity;
            if (hand == Hand.Left) {
                vel.x = Input.GetAxis("LeftHandHorizontal");
                vel.z = Input.GetAxis("LeftHandVertical");
            }
            else {
                vel.x = Input.GetAxis("RightHandHorizontal");
                vel.z = Input.GetAxis("RightHandVertical");
            }
            vel.y = 0;
            vel = vel.normalized;
            vel *= holding == null ? moveSpeed : dragSpeed;
            rigidbody.velocity = vel;
        }
        
    }

    private void Update() {
        switch (hand) {
            case Hand.Left:
                if (Input.GetButtonDown("LeftHandInteract")) {
                    InteractPressed();
                }
                break;
            case Hand.Right:
                if (Input.GetButtonDown("RightHandInteract")) {
                    InteractPressed();
                }
                break;
        }
    }

    void InteractPressed () {
        if (Animating) {
            return;
        }
        if (HoldingSomething) {
            switch (holding.tag) {
                case "food":
                    holding.Drop(this);
                    holding = null;
                    break;
                case "plate":
                    holding.Drop(this);
                    holding.rigidbody.AddForce(rigidbody.velocity, ForceMode.Impulse);
                    holding = null;
                    break;
                case "fryBasket":
                    if (zone == Zone.Frier) {
                        holding.Drop(this);
                        holding = null;
                        break;
                    }
                    else {
                        StartCoroutine(DumpBasket());
                    }
                    break;
            }
        }
        else {
            RaycastHit[] hits;
            int layerMask = frierMask | trashMask;
            string[] tagPriority;
            switch (zone) {
                case Zone.Cooler:
                    layerMask |= coolerMask;
                    tagPriority = new string[] { "fryBasket", "cooler", "trash" };
                    break;
                case Zone.Platting:
                    layerMask |= plateMask | foodMask;
                    tagPriority = new string[] { "fryBasket", "food", "trash", "plate" };
                    break;
                case Zone.Breading:
                case Zone.Prep:
                    layerMask |= foodMask;
                    tagPriority = new string[] { "food", "fryBasket", "trash" };
                    break;
                case Zone.Frier:
                    tagPriority = new string[] { "fryBasket", "trash" };
                    break;
                default:
                    layerMask |= foodMask;
                    tagPriority = new string[] { "fryBasket", "trash", "food" };
                    break;
            }
            Debug.Log("SphereCasting!");
            hits = Physics.SphereCastAll(this.transform.position, 1, -this.transform.up, 10, layerMask);
            Debug.LogFormat("We hit:{0}", string.Join(",", hits));
            foreach (string checkTag in tagPriority) {
                foreach (RaycastHit hit in hits) {
                    if (hit.transform.CompareTag(checkTag)) {
                        StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        break;
                    }
                }
                if (pickupState == PickupState.Seeking) break;
            }
        }
    }

    IEnumerator PickupAnimation(GameObject target) {
        Grabbable targetGrabbable = target.GetComponent<Grabbable>();
        if (targetGrabbable != null && targetGrabbable.Held) {
            yield break;
        }
        switch (target.tag) {
            case "fryBasket":
                yield return PickupAndRotate(target);
                yield break;
            case "plate":
                PlateBehavior plate = target.GetComponent<PlateBehavior>();
                if (plate.Open) {
                    plate.Close();
                    yield break;
                }
                break;
        }

        pickupState = PickupState.Seeking;
        pickupTarget = target;
        Vector3 currPos = transform.position;
        Vector3 vel = Vector3.zero;

        //Debug.LogFormat("{0} Hand is going in", hand);
        while (pickupState == PickupState.Seeking) {
            if((currPos - transform.position).magnitude > pickUpDistance || HoldingSomething) {
                pickupState = PickupState.Returning;
                break;
            }
            vel = target.transform.position - this.transform.position;
            vel = vel.normalized;
            vel *= pickupSpeed;
            rigidbody.velocity = vel;
            yield return new WaitForFixedUpdate();
        }

        //Debug.LogFormat("{0} Hand is backing out", hand);
        while(transform.position.y < hoverHeight) {
            rigidbody.velocity = Vector3.up * pickupSpeed;
            yield return new WaitForFixedUpdate();
        }

        rigidbody.velocity = Vector3.zero;
        //Debug.LogFormat("{0} Hand Done Picking Up", hand);
        pickupState = PickupState.Idle;
        yield break;
    }


    IEnumerator PickupAndRotate(GameObject target) {
        pickupState = PickupState.Seeking;
        pickupTarget = target;
        Vector3 currPos = transform.position;
        Vector3 vel;

        float translationDistance = (transform.position - target.transform.position).magnitude;
        Quaternion targetRotation = target.transform.rotation;

        //Debug.LogFormat("{0} Hand is going in", hand);
        while (pickupState == PickupState.Seeking) {
            if ((currPos - transform.position).magnitude > pickUpDistance || HoldingSomething) {                
                break;
            }
            vel = target.transform.position - transform.position;
            vel = vel.normalized;
            vel *= pickupSpeed;
            rigidbody.velocity = vel;
            rigidbody.MoveRotation(Quaternion.Slerp(Quaternion.identity,
                                                    targetRotation,
                                                    1 - (transform.position - target.transform.position).magnitude / translationDistance));
            yield return new WaitForFixedUpdate();
        }

        pickupState = PickupState.Returning;
        translationDistance = hoverHeight - transform.position.y;
        rigidbody.rotation = targetRotation;

        //Debug.LogFormat("{0} Hand is backing out", hand);
        while (transform.position.y < hoverHeight) {
            rigidbody.velocity = Vector3.up * pickupSpeed;
            rigidbody.MoveRotation(Quaternion.Slerp(targetRotation,
                                        Quaternion.identity,
                                        1 - (hoverHeight - transform.position.y) / translationDistance));
            //Debug.Log("Returning");
            yield return new WaitForFixedUpdate();
        }

        rigidbody.velocity = Vector3.zero;
        rigidbody.rotation = Quaternion.identity;
        pickupState = PickupState.Idle;
        yield break;
    }

    IEnumerator DumpBasket() {
        if (dumping) {
            yield break;
        }
        if(pickupState != PickupState.Idle) {
            Debug.LogError("What is going on !?");
        }
        dumping = true;
        float callTime = Time.fixedTime;
        while(Time.fixedTime - callTime < dumpRotationTime) {
            rigidbody.MoveRotation(Quaternion.Slerp(Quaternion.identity, dumpAngle, (Time.fixedTime - callTime) / dumpRotationTime));
            yield return new WaitForFixedUpdate();
        }
        callTime = Time.fixedTime;
        while (Time.fixedTime - callTime < dumpRotationTime) {
            rigidbody.MoveRotation(Quaternion.Slerp(dumpAngle,Quaternion.identity, (Time.fixedTime - callTime) / dumpRotationTime));
            yield return new WaitForFixedUpdate();
        }
        rigidbody.rotation = Quaternion.identity;
        dumping = false;
        yield break;
    }

    private void OnCollisionEnter(Collision collision) {
        //Debug.LogFormat("{0} Hand Hit: {1}", hand, collision.gameObject.name);
        switch (pickupState) {
            case PickupState.Seeking:
                if (collision.gameObject == pickupTarget) {
                    //if the target is food, basket, or plate
                    switch (pickupTarget.tag) {
                        case "food":
                        case "fryBasket":
                        case "plate":
                            Grabbable grab = pickupTarget.GetComponent<Grabbable>();
                            if (grab.Held) {
                                break;
                            }
                            grab.Pickup(this);
                            holding = grab;
                            pickupTarget = null;
                            //Debug.LogFormat("Hand {0} Grabbed the {1}", hand, holding.name);
                            break;
                    }
                }
                break;

            case PickupState.Idle:
                if (collision.gameObject.tag == "hand" && this.HoldingSomething && this.holding.tag == "food") {
                    //Debug.LogFormat("Hand {0} hit the other hand", hand);
                    HandBehavior otherHand = collision.gameObject.GetComponent<HandBehavior>();

                    if (otherHand.HoldingSomething && otherHand.holding.tag == "food") {
                        FoodBehavior thisFood = holding.GetComponent<FoodBehavior>();
                        FoodBehavior otherFood = otherHand.holding.GetComponent<FoodBehavior>();
                        Debug.LogFormat("We both have food! I Have {0} They have {1}", thisFood.foodType, otherFood.foodType);
                        StageSettings_ld48 settings = (StageSettings_ld48)sceneWrangler.currentSceneContainer.stageSettings;
                        switch (thisFood.foodType, otherFood.foodType) {

                            case (FoodBehavior.FoodType.Duck, FoodBehavior.FoodType.Chicken):
                                if(otherFood.doneness != FoodBehavior.Doneness.Raw) {
                                    //form a ducken
                                    Debug.Log("Form the Ducken!");
                                    GameObject ducken = Instantiate(settings.DuckenPrefab, thisFood.transform.position, thisFood.transform.rotation) as GameObject;
                                    Grabbable duckenGrabbable = ducken.GetComponent<Grabbable>();
                                    particleSystem.Play();
                                    this.holding = duckenGrabbable;
                                    duckenGrabbable.Pickup(this);
                                    otherHand.holding = null;
                                    Destroy(thisFood.gameObject);
                                    Destroy(otherFood.gameObject);
                                }
                                break;
                            case (FoodBehavior.FoodType.Turkey, FoodBehavior.FoodType.Ducken):
                                if (otherFood.doneness != FoodBehavior.Doneness.Raw) {
                                    //form the turducken
                                    Debug.Log("Form the Turducken!");
                                    GameObject turducken = Instantiate(settings.TurduckenPrefab, thisFood.transform.position, thisFood.transform.rotation) as GameObject;
                                    Grabbable turduckenGrabbable = turducken.GetComponent<Grabbable>();
                                    particleSystem.Play();
                                    this.holding = turduckenGrabbable;
                                    turduckenGrabbable.Pickup(this);
                                    otherHand.holding = null;
                                    Destroy(thisFood.gameObject);
                                    Destroy(otherFood.gameObject);

                                 
                                }
                                break;

                        }


                    }
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Debug.LogFormat("{0} Hand Entered a Trigger: {1}", hand, other.transform.name);
        switch (pickupState) {
            case PickupState.Seeking:
                if (other.gameObject == pickupTarget) {
                    switch (pickupTarget.tag) {
                        case "cooler":
                            CoolerBehavior cb = pickupTarget.GetComponent<CoolerBehavior>();
                            GameObject newFood = Instantiate(cb.foodPrefab, this.transform.position, Quaternion.identity) as GameObject;
                            Grabbable grab = newFood.GetComponent<Grabbable>();
                            this.holding = grab;
                            grab.Pickup(this);
                            pickupTarget = null;
                            //Debug.LogFormat("{0} Hand Grabbed a new {1}", hand, this.holding.name);
                            break;
                    }
                }
                break;
        }

        if(other.tag == "zone") {
            switch (other.name) {
                case "CoolerZone":
                    zone = Zone.Cooler;
                    break;
                case "BreadingZone":
                    zone = Zone.Breading;
                    break;
                case "FrierZone":
                    zone = Zone.Frier;
                    break;
                case "PrepZone":
                    zone = Zone.Prep;
                    break;
                case "PlatingZone":
                    zone = Zone.Platting;
                    break;
                case "DeliveryZone":
                    zone = Zone.Delivery;
                    break;
                case "TrashZone":
                    zone = Zone.Trash;
                    break;
                default:
                    zone = Zone.Other;
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "zone") {
            switch (other.name) {
                case "CoolerZone":
                    zone = Zone.Cooler;
                    break;
                case "BreadingZone":
                    zone = Zone.Breading;
                    break;
                case "FrierZone":
                    zone = Zone.Frier;
                    break;
                case "PrepZone":
                    zone = Zone.Prep;
                    break;
                case "PlatingZone":
                    zone = Zone.Platting;
                    break;
                case "DeliveryZone":
                    zone = Zone.Delivery;
                    break;
                case "TrashZone":
                    zone = Zone.Trash;
                    break;
                default:
                    zone = Zone.Other;
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "zone") {
            zone = Zone.Other;
        }
    }

    public void SwapHeldItem(GameObject gob) {
        Grabbable grabbable = gob.GetComponent<Grabbable>();
        this.holding = grabbable;
        grabbable.Pickup(this);
    }
}
