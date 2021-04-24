using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool PickingUp {
        get {
            return pickupState != PickupState.Idle;
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
    public LayerMask frierZoneMask;
    public LayerMask coolerZoneMask;
    public LayerMask foodZoneMask;
    public LayerMask plateZoneMask;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PickingUp) {
            Vector3 vel = rigidbody.velocity;
            if (hand == Hand.Left) {
                vel.x = Input.GetAxis("LeftHandHorizontal");
                vel.z = Input.GetAxis("LeftHandVertical");
            }
            else {
                vel.x = Input.GetAxis("RightHandHorizontal");
                vel.z = Input.GetAxis("RightHandVertical");
            }
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
        if (HoldingSomething) {
            switch (holding.tag) {
                case "food":
                    //drop it, contextually?
                    //maybe do the stuffing?
                    Debug.LogFormat("Hand Dropped {0}:", holding.name);
                    holding.Drop(this);
                    holding = null;
                    break;
                case "fryBasket":
                    if(zone == Zone.Frier) {
                        Debug.LogFormat("Hand Dropped {0}:", holding.name);
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
            RaycastHit hit;
            switch (zone) {
                case Zone.Frier:
                    if(Physics.Raycast(this.transform.position, -this.transform.up, out hit, 10, frierZoneMask)) {
                        if (hit.transform.tag == "fryBasket") {
                            StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        }
                    }
                    break;

                case Zone.Cooler:
                    if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, 10, coolerZoneMask)) {
                        if (hit.transform.tag == "cooler") {
                            StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        }
                    }
                    break;


                case Zone.Prep:
                case Zone.Breading:
                    if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, 10, foodZoneMask)) {
                        if (hit.transform.tag == "food") {
                            StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        }
                    }
                    break;

                case Zone.Platting:
                    if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, 10, plateZoneMask)) {
                        if (hit.transform.tag == "plate") {
                            StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        }
                    }
                    break;
            }
        }
    }

    IEnumerator PickupAnimation(GameObject target) {
        pickupState = PickupState.Seeking;
        pickupTarget = target;
        Vector3 currPos = transform.position;
        Vector3 vel = Vector3.zero;

        Debug.LogFormat("{0} Hand is going in", hand);
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

        Debug.LogFormat("{0} Hand is backing out", hand);
        while(transform.position.y < hoverHeight) {
            vel = this.transform.up;
            vel *= pickupSpeed;
            rigidbody.velocity = vel;
            yield return new WaitForFixedUpdate();
        }

        rigidbody.velocity = Vector3.zero;
        Debug.LogFormat("{0} Hand Done Picking Up", hand);
        pickupState = PickupState.Idle;
        yield break;
    }

    IEnumerator DumpBasket() {
        Debug.LogFormat("{0} Hand is dumping the basket", hand);
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
        yield break;
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.LogFormat("{0} Hand Hit: {1}", hand, collision.gameObject.name);
        if (PickingUp) {
            if (collision.gameObject == pickupTarget) {
                //if the target is food, basket, or plate
                switch (pickupTarget.tag) {
                    case "food":
                    case "fryBasket":
                    case "plate":
                        Grabbable grab = pickupTarget.GetComponent<Grabbable>();
                        grab.Pickup(this);
                        holding = grab;
                        pickupTarget = null;
                        Debug.LogFormat("Hand {0} Grabbed the {1}", hand, holding.name);
                        break;

                    /*case "cooler":
                        CoolerBehavior cb = pickupTarget.GetComponent<CoolerBehavior>();
                        GameObject newFood = Instantiate(cb.foodPrefab, this.transform.position, Quaternion.identity, this.transform) as GameObject;
                        this.holding = newFood;
                        newFood.GetComponent<FoodBehavior>().Hold();
                        pickupTarget = null;
                        Debug.LogFormat("Grabbed a new {0}", this.holding.name);
                        break;*/
                }
            }

            if(collision.gameObject.name == "PrepSurface") {
                Debug.LogFormat("{0} Hand Hit the table, Aborting pick up", hand);
                this.pickupState = PickupState.Returning;
            }
            //if we hit the prep surface abort pickup
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.LogFormat("{0} Hand Entered a Trigger: {1}", hand, other.transform.name);
        if (PickingUp) {
            if (other.gameObject == pickupTarget) {
                switch (pickupTarget.tag) {
                    case "cooler":
                        CoolerBehavior cb = pickupTarget.GetComponent<CoolerBehavior>();
                        GameObject newFood = Instantiate(cb.foodPrefab, this.transform.position, Quaternion.identity) as GameObject;
                        Grabbable grab = newFood.GetComponent<Grabbable>();
                        grab.Pickup(this);
                        this.holding = grab;
                        pickupTarget = null;
                        Debug.LogFormat("{0} Hand Grabbed a new {1}", hand, this.holding.name);
                        break;
                }
            }
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
}
