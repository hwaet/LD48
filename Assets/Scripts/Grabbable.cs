using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public new Collider collider;
    public new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    public void Drop(HandBehavior hand) {
        StartCoroutine(DropCoroutine(hand));
    }

    IEnumerator DropCoroutine(HandBehavior hand) {
        this.transform.parent = null;
        this.rigidbody.isKinematic = false;
        this.rigidbody.useGravity = true;
        yield return new WaitForSeconds(.3f);
        Physics.IgnoreCollision(this.collider, hand.collider, false);
        

    }
}
