using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryBehavior : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("check order")]
    public void checkOrder(FoodBehavior deliveredFood)
    {
        Debug.Log("Delivered this food:" + deliveredFood.name);
    }

    public void checkOrder(PlateBehavior deliveredFood) {
        Debug.Log("Delivered this food:" + deliveredFood.name);
    }
}
