using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class PlateBehavior : MonoBehaviour
{

    public List<FoodBehavior> contents = new List<FoodBehavior>();

    public List<Vector3> snapSlots = new List<Vector3>();

    public bool plateIsUsed = false;

    public bool OrderFull {
        get {
            return contents.Count == snapSlots.Count;
        }
    }

    private bool isOpen = true;

    public bool Open {
        get { return isOpen; }
    }

    public float closeTime = 1;

    private Transform lid;
    private Grabbable grabbable;
    private bool inDelivery = false;
    private SceneWrangler sceneWrangler;

    // Start is called before the first frame update
    void Start()
    {
        lid = transform.GetChild(0);
        grabbable = GetComponent<Grabbable>();
        grabbable.OnPickup += OnPickup;
        grabbable.OnDrop += OnDrop;
        this.sceneWrangler = FindObjectOfType<SceneWrangler>();

        List<SceneWrangler> wranglers = FindObjectsOfType<SceneWrangler>().ToList();
        foreach (SceneWrangler wrangler in wranglers)
        {
            if (wrangler.levelState == LevelLoadingProcess.Idle) this.sceneWrangler = wrangler;
        }
    }

    private void OnPickup(HandBehavior hand) {
        StageSettings_ld48 settings = (StageSettings_ld48)sceneWrangler.currentSceneContainer.stageSettings;
        Instantiate(settings.PlatePrefab, this.transform.position, this.transform.rotation);
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
        isOpen = false;

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
        if (other.tag == "delivery") {
            inDelivery = true;
        }
        if (!grabbable.Held) {
            DeliverMe();
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "delivery") {
            inDelivery = false;
        }
    }

    private void OnDrop(HandBehavior hand) {
        if (inDelivery) {
            DeliverMe();
        }
    }

    private void DeliverMe() {
        GameObject deliveryZone = GameObject.FindGameObjectWithTag("delivery");
        if ((deliveryZone != null) && (contents.Count != 0) && (plateIsUsed==false)) {
            plateIsUsed = true;
            DeliveryBehavior deliveryBehavior = deliveryZone.GetComponent<DeliveryBehavior>();
            deliveryBehavior.evaluatePlate(this);
        }
    }


}
