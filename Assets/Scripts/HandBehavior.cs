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
    private GameObject holding = null;


    private Rigidbody rigidbody;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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

    private void Update() {
        if (Input.GetKeyDown(interact)) {
            Debug.Log("Hand Interacting!");
            if (holding == null) {
                RaycastHit hit;
                if (Physics.SphereCast(this.transform.position, 1, -this.transform.up, out hit, 5)) {
                    Debug.Log("SphereCast Hit:" + hit);
                    switch (hit.transform.tag) {
                        case "food":
                        case "basket":
                            Debug.LogFormat("Hand got a: {0}", hit.transform.tag);
                            this.holding = hit.transform.gameObject;
                            Rigidbody rb = holding.GetComponent<Rigidbody>();
                            if (rb != null) {
                                rb.isKinematic = true;
                            }
                            Vector3 pos = this.holding.transform.position;
                            pos.y = 1;
                            this.holding.transform.position = pos;
                            hit.transform.parent = this.transform;
                            break;
                    }
                }
            }
            else {
                Debug.Log("Hand Dropped");
                this.holding.transform.parent = null;
                this.holding = null;
                Rigidbody rb = holding.GetComponent<Rigidbody>();
                if(rb != null) {
                    rb.isKinematic = false;
                }
            }
        }
    }
}
