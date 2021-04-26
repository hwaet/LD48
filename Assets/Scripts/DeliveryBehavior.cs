using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryBehavior : MonoBehaviour
{
    SceneWrangler sceneWrangler;
    public bool enforceTime = false;
    public Queue<Order> orderQueue = new Queue<Order>();

    public List<Order> activeOrderList = new List<Order>();
    public List<float> orderAddTimes = new List<float>();
    public float orderAddDelay;
    private float lastAddTime;

    bool armVictoryState = false;

    // Start is called before the first frame update
    void Start()
    {
        //reloadOrderList();
    }

    private void OnEnable()
    {
        SceneWrangler.sceneLoadComplete += reloadOrderList;
    }

    private void OnDisable()
    {
        SceneWrangler.sceneLoadComplete -= reloadOrderList;
    }

    private void Update() {

        if (enforceTime) {
            List<int> toRemove = new List<int>();
            for(int i = 0; i < activeOrderList.Count; i++) {
                if(Time.time - orderAddTimes[i] > activeOrderList[i].patience) {
                    toRemove.Add(i);
                }
            }
            foreach(int i in toRemove) {
                orderAddTimes.RemoveAt(i);
                activeOrderList.RemoveAt(i);
            }
            
            if (orderQueue.Count > 0) {
                if (Time.time - lastAddTime > orderAddDelay || activeOrderList.Count == 0) {
                    activeOrderList.Add(orderQueue.Dequeue());
                    orderAddTimes.Add(Time.time);
                }
            }
            else {
                checkForVictory();
            }
        }
        else {
            checkForVictory();
        }
        
    }

    [ContextMenu("reload orders")]
    private void reloadOrderList()
    {
        List<SceneWrangler> wranglers = FindObjectsOfType<SceneWrangler>().ToList();
        foreach (SceneWrangler wrangler in wranglers)
        {
            if (wrangler.levelState == LevelLoadingProcess.Idle) sceneWrangler = wrangler;
        }


        print("reloading order list");
        armVictoryState = true;
        StageSettings_ld48 settings = (StageSettings_ld48)sceneWrangler.currentSceneContainer.stageSettings;
        orderQueue.Clear();
        activeOrderList.Clear();
        orderAddTimes.Clear();
        if (enforceTime) {
            foreach (Order order in settings.recipesOrders) {
                orderQueue.Enqueue(order);
            }
            activeOrderList.Add(orderQueue.Dequeue());
            orderAddTimes.Add(Time.time);
            lastAddTime = Time.time;
        }
        else {
            print("test" + sceneWrangler.currentSceneContainer);
            activeOrderList.AddRange(settings.recipesOrders);
        }
    }


  /*  private void OnTriggerEnter(Collider other) {
        if(other.tag == "plate") {
            PlateBehavior plate = other.GetComponent<PlateBehavior>();
            evaluatePlate(plate);
        }
    }*/

    // Update is called once per frame
    public void evaluatePlate(PlateBehavior plate) {
        //compareAgainstOrder(plate.contents);
        Debug.LogFormat("Recieved this plate: {0}", string.Join(",", plate.contents));

        Debug.Log("Checking against Orders:");
        foreach(Order order in activeOrderList) {
            Debug.Log(string.Join(",", order.FoodItems));
        }

        int orderHit = -1;
        for(int i = 0; i < activeOrderList.Count; i++) {
            if (activeOrderList[i].CheckDelivery(plate)) {
                orderHit = i;
                break;
            }
        }

        if(orderHit > -1) {
            Debug.Log("Successful Delivery!");
            activeOrderList.RemoveAt(orderHit);
            if (enforceTime) {
                orderAddTimes.RemoveAt(orderHit);
            }
            StartCoroutine(OrderResult(plate, true));
            
        }
        else {
            Debug.Log("Incorrect Delivery!");
            //could be fun to throw the plate back rather than just destroy it
            StartCoroutine(OrderResult(plate, false));
        }
    }

    IEnumerator OrderResult(PlateBehavior plate, bool success) {
        yield return new WaitForSeconds(1);
        if (success) {
            Destroy(plate.gameObject);
        }
        else {
            Rigidbody plateBody = plate.GetComponent<Rigidbody>();
            plateBody.AddForce((Vector3.up - Vector3.forward) * 10, ForceMode.Impulse);
        }

    }

    [ContextMenu("check for victory condition")]
    public void checkForVictory()
    {
        if ((activeOrderList.Count == 0) && (armVictoryState == true))
        {
            Debug.Log("You win this stage!!!!!");
            sceneWrangler.loadTargetContainer();
        }
    }
}
