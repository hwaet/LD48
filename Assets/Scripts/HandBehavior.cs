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
        float xVal = (Input.GetKey(right) ? 1 : 0) - (Input.GetKey(left) ? 1 : 0);
        float yVal = (Input.GetKey(down) ? 1 : 0) - (Input.GetKey(up) ? 1 : 0);
        Vector2 velocity = new Vector2(xVal, yVal);
        velocity = velocity.normalized;
        velocity *= holding == null ? moveSpeed : dragSpeed;
        rigidbody.velocity = velocity;

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
