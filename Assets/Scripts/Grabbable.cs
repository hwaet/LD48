using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour {
    public new Collider collider;
    public Collider[] colliders;
    public new Rigidbody rigidbody;

    public delegate void PickupEventHandler(HandBehavior hand);

    public event PickupEventHandler OnPickup;

    public delegate void DropEventHandler(HandBehavior hand);

    public event DropEventHandler OnDrop;

    private bool held;
    public HandBehavior holder;

    private int layerBak;

    public bool Held {
        get { return held; }
    }

    // Start is called before the first frame update
    void Start() {
        this.rigidbody = GetComponent<Rigidbody>();
        this.colliders = GetComponents<Collider>();
        this.collider = GetComponent<Collider>();
        layerBak = this.gameObject.layer;
    }

    public void Pickup(HandBehavior hand) {
        this.holder = hand;
        StartCoroutine(PickupCoroutine(hand));
    }

    IEnumerator PickupCoroutine(HandBehavior hand) {
        while (this.collider == null) {
            yield return new WaitForEndOfFrame();
        }
        gameObject.layer = hand.gameObject.layer;
        foreach (Collider collider in colliders) {
            Physics.IgnoreCollision(collider, hand.collider, true);
            collider.enabled = true;
        }
        Physics.IgnoreCollision(this.collider, hand.collider, true);
        this.collider.enabled = true;
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        transform.parent = hand.transform;
        held = true;
        OnPickup?.Invoke(hand);
    }

    public void Drop(HandBehavior hand) {
        StartCoroutine(DropCoroutine(hand));
    }

    IEnumerator DropCoroutine(HandBehavior hand) {
        OnDrop?.Invoke(hand);
        this.held = false;
        this.transform.parent = null;
        this.holder = null;
        this.gameObject.layer = layerBak;
        this.rigidbody.isKinematic = false;
        this.rigidbody.useGravity = true;
        yield return new WaitForSeconds(.3f);
        foreach (Collider collider in colliders) {
            Physics.IgnoreCollision(collider, hand.collider, false);
        }
        Physics.IgnoreCollision(this.collider, hand.collider, false);
    }
}
