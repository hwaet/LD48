using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HandBehavior : MonoBehaviour
{
    public KeyCode interact;

    public enum Hand {
        Left,
        Right
    }

    public Hand hand;

    public float moveSpeed;
    public float dragSpeed;
    public float pickupSpeed;
    public float hoverHeight;
    public float pickUpDistance;
    private GameObject holding = null;
    

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
        if (holding == null) {
            RaycastHit hit;
            Debug.DrawRay(this.transform.position, -this.transform.up, Color.red);
            if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, 10f)) {
                //Debug.Log("SphereCast Hit:" + hit.transform.name);
                switch (hit.transform.tag) {
                    case "food":
                        FoodBehavior fb = hit.transform.GetComponent<FoodBehavior>();
                        if (!fb.cooking) {
                            StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        }
                        break;
                    case "fryBasket":
                    case "cooler":
                    case "plate":
                        StartCoroutine(PickupAnimation(hit.transform.gameObject));
                        break;
                }
            }
        }
        else {
            Grabbable grab = this.holding.GetComponent<Grabbable>();
            if (grab != null) {
                grab.Drop(this);
            }
            this.holding = null;
            Debug.Log("Hand Dropped");
        }
    }

    IEnumerator PickupAnimation(GameObject target) {
        pickupState = PickupState.Seeking;
        pickupTarget = target;
        Vector3 currPos = transform.position;
        Vector3 vel = Vector3.zero;

        Debug.Log("Hand is going in");
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

        Debug.Log("Hand is backing out");
        while(transform.position.y < hoverHeight) {
            vel = this.transform.up;
            vel *= pickupSpeed;
            rigidbody.velocity = vel;
            yield return new WaitForFixedUpdate();
        }

        rigidbody.velocity = Vector3.zero;
        Debug.Log("Done Picking Up");
        pickupState = PickupState.Idle;
        yield break;
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.LogFormat("Hand Hit: {0}", collision.gameObject.name);
        if (PickingUp) {
            if (collision.gameObject == pickupTarget) {
                //if the target is food, basket, or plate
                switch (pickupTarget.tag) {
                    case "food":
                    case "fryBasket":
                    case "plate":
                        Grabbable grab = pickupTarget.GetComponent<Grabbable>();
                        if(grab != null) {
                            grab.Pickup(this);
                        }
                        holding = pickupTarget;
                        pickupTarget = null;
                        Debug.LogFormat("Grabbed the {0}", holding.name);
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
                Debug.Log("Hit the table, Aborting pick up");
                this.pickupState = PickupState.Returning;
            }
            //if we hit the prep surface abort pickup
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.LogFormat("Hand Entered a Trigger: {0}",other.transform.name);
        if (PickingUp) {
            if(other.gameObject == pickupTarget) {
                switch (pickupTarget.tag) {
                    case "cooler":
                        CoolerBehavior cb = pickupTarget.GetComponent<CoolerBehavior>();
                        GameObject newFood = Instantiate(cb.foodPrefab, this.transform.position, Quaternion.identity) as GameObject;
                        this.holding = newFood;
                        Grabbable grab = newFood.GetComponent<Grabbable>();
                        grab.Pickup(this);
                        pickupTarget = null;
                        Debug.LogFormat("Grabbed a new {0}", this.holding.name);
                        break;
                }
            }
        }
    }
}
