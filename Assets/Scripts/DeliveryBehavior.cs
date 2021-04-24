using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryBehavior : MonoBehaviour
{
    SceneWrangler sceneWrangler;
    public List<Recipe> orderList;

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
        foreach (Recipe recipe in settings.recipesOrders)
        {
            orderList.Add(recipe);
        }
    }

    // Update is called once per frame
    public void checkOrder(PlateBehavior deliveredPlate)
    {
        compareAgainstOrder(deliveredPlate.contents);
        Debug.Log("Delivered this plate:" + deliveredPlate.name);
    }

    public void checkOrder(FoodBehavior deliveredFood)
    {
        List<FoodBehavior> tempFoodPlate = new List<FoodBehavior>();
        tempFoodPlate.Add(deliveredFood);
        compareAgainstOrder(tempFoodPlate);
        Debug.Log("Delivered this food:" + deliveredFood.name);
    }

    [ContextMenu("check order")]
    public void compareAgainstOrder(List<FoodBehavior> deliveredFood)
    {
        foreach (Recipe recipe in orderList)
        {
            bool successfullDelivery = recipe.compareAgainstFood(deliveredFood);

            if (successfullDelivery == true)
            {
                Debug.Log("You have successfully filled an order:" + recipe.recipeName);
                orderList.Remove(recipe);
                
                foreach (FoodBehavior food in deliveredFood)
                {
                    GameObject.Destroy(food.gameObject);
                }
                return;
                
            }
        }
        
    }

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
