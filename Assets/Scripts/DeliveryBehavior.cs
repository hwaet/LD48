using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryBehavior : MonoBehaviour
{
    SceneWrangler sceneWrangler;
    public List<Order> orderList;

    // Start is called before the first frame update
    void Start()
    {
        reloadOrderList();
    }

    private void Update()
    {
        checkForVictory();
    }

    [ContextMenu("reload orders")]
    private void reloadOrderList()
    {
        sceneWrangler = FindObjectOfType<SceneWrangler>();
        StageSettings_ld48 settings = (StageSettings_ld48)sceneWrangler.currentSceneContainer.stageSettings;
        orderList.Clear();
        foreach (Order recipe in settings.recipesOrders)
        {
            orderList.Add(recipe);
        }
    }


    private void OnTriggerEnter(Collider other) {
        if(other.tag == "plate") {
            PlateBehavior plate = other.GetComponent<PlateBehavior>();
            evaluatePlate(plate);
        }
    }

    // Update is called once per frame
    public void evaluatePlate(PlateBehavior plate) {
        //compareAgainstOrder(plate.contents);
        Debug.LogFormat("Recieved this plate: {0}", string.Join(",", plate.contents));

        Debug.Log("Checking against Orders:");
        foreach(Order order in orderList) {
            Debug.Log(string.Join(",", order.FoodItems));
        }

        int orderHit = -1;
        for(int i = 0; i < orderList.Count; i++) {
            if (orderList[i].CheckDelivery(plate)) {
                orderHit = i;
                break;
            }
        }

        if(orderHit > -1) {
            Debug.Log("Successful Delivery!");
            orderList.RemoveAt(orderHit);
            Destroy(plate.gameObject);
        }
        else {
            Debug.Log("Incorrect Delivery!");
            //could be fun to throw the plate back rather than just destroy it
            Destroy(plate.gameObject);
        }



    }

/*    public void checkOrder(FoodBehavior deliveredFood)
    {
        List<FoodBehavior> tempFoodPlate = new List<FoodBehavior>();
        tempFoodPlate.Add(deliveredFood);
        compareAgainstOrder(tempFoodPlate);
        Debug.Log("Delivered this food:" + deliveredFood.name);
    }
*/
  /*  [ContextMenu("check order")]
    public void compareAgainstOrder(List<FoodBehavior> deliveredFood)
    {
        foreach (Order order in orderList)
        {
            bool successfullDelivery = order.CheckDeliver(deliveredFood);

            if (successfullDelivery == true)
            {
                Debug.Log("You have successfully filled an order:" + order.recipeName);
                orderList.Remove(order);
                
                foreach (FoodBehavior food in deliveredFood)
                {
                    GameObject.Destroy(food.gameObject);
                }
                return;
                
            }
        }
        
    }*/

    [ContextMenu("check for victory condition")]
    public void checkForVictory()
    {
        if (orderList.Count == 0)
        {
            Debug.Log("You win this stage!!!!!");
            sceneWrangler.loadTargetContainer();
        }
    }
}
