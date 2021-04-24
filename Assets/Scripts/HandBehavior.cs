using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HandBehavior : MonoBehaviour
{
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
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

        if(Input.GetKey(interact)) {
            if (holding == null) {
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, -this.transform.up, out hit)) {
                    switch(hit.transform.tag) {
                        case "food":
                        case "basket":
                            this.holding = hit.transform.gameObject;
                            hit.transform.parent = this.transform;
                            break;
                    }
                }
            }
            else {
                this.holding.transform.parent = null;
                this.holding = null;
            }
        }
    }
}
