using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour {
    public new Collider collider;
    public new Rigidbody rigidbody;

    public delegate void PickupEventHandler(HandBehavior hand);

    public event PickupEventHandler OnPickup;

    public delegate void DropEventHandler(HandBehavior hand);

    public event DropEventHandler OnDrop;

    private bool held;

    public bool Held {
        get { return held; }
    }

    // Start is called before the first frame update
    void Start() {
        this.rigidbody = GetComponent<Rigidbody>();
        this.collider = GetComponent<Collider>();
    }

    public void Pickup(HandBehavior hand) {
        StartCoroutine(PickupCoroutine(hand));
    }

    IEnumerator PickupCoroutine(HandBehavior hand) {
        while (this.collider == null) {
            yield return new WaitForEndOfFrame();
        }
        Physics.IgnoreCollision(this.collider, hand.collider, true);
        this.collider.enabled = true;
        this.rigidbody.isKinematic = true;
        this.rigidbody.useGravity = false;
        this.transform.parent = hand.transform;
        this.held = true;
        OnPickup?.Invoke(hand);
    }

    public void Drop(HandBehavior hand) {
        StartCoroutine(DropCoroutine(hand));
    }

    IEnumerator DropCoroutine(HandBehavior hand) {
        OnDrop?.Invoke(hand);
        this.held = false;
        this.transform.parent = null;
        this.rigidbody.isKinematic = false;
        this.rigidbody.useGravity = true;
        yield return new WaitForSeconds(.3f);
        Physics.IgnoreCollision(this.collider, hand.collider, false);
    }

    public void ResetRotation(float rotateTime) {
        StartCoroutine(ResetRotationCoroutine(rotateTime));
    }

    IEnumerator ResetRotationCoroutine(float rotateTime) {
        float callTime = Time.time;
        Quaternion callRot = transform.rotation;

        while(Time.time - callTime < rotateTime) {
            this.transform.localRotation = Quaternion.Slerp(callRot, Quaternion.identity, (Time.time - callTime) / rotateTime);
            yield return new WaitForEndOfFrame();
        }
        this.transform.rotation = Quaternion.identity;
    }
}
