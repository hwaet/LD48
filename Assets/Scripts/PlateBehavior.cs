using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlateBehavior : MonoBehaviour
{

    public List<FoodBehavior> contents = new List<FoodBehavior>();

    public List<Vector3> snapSlots = new List<Vector3>();

    public bool OrderFull {
        get {
            return contents.Count == snapSlots.Count;
        }
    }

    public float closeTime = 1;

    private Transform lid;

    // Start is called before the first frame update
    void Start()
    {
        lid = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Close() {
        StartCoroutine(CloseAnim());
    }

    IEnumerator CloseAnim() {
        float callTime = Time.fixedTime;
        Quaternion startRot = transform.rotation;
        while (Time.fixedTime - callTime < closeTime) {
            lid.rotation = Quaternion.Slerp(startRot, Quaternion.identity, (Time.fixedTime - callTime) / closeTime);
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.LogFormat("{0} Landed on a plate", collision.transform.name);
        if(!OrderFull && collision.transform.tag == "food") {
            FoodBehavior fb = collision.transform.GetComponent<FoodBehavior>();
            contents.Add(fb);
            Collider collider = collision.collider;
            collider.enabled = false;
            collider.transform.parent = this.transform;
            collider.transform.localPosition = snapSlots[contents.Count - 1];
            collider.transform.rotation = Quaternion.identity;

            if(contents.Count == snapSlots.Count) {
                Close();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "delivery") {
            DeliveryBehavior delivery = other.transform.GetComponent<DeliveryBehavior>();
            delivery.checkOrder(this);
            //Debug.LogFormat("Delivered: {0} {1} with: ", doneness, this.FoodType, this.BreadingLayers.ToString());
            //Destroy(this.gameObject);
        }

    }

}
