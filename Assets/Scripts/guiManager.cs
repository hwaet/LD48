using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class guiManager : MonoBehaviour
{
    SceneWrangler wrangler;
    public SceneContainer menuContainer;

    [Header("Order Display")]   
    public RectTransform orderListRect;
    public GameObject orderPrefab;
    public Sprite breading1;
    public Sprite breading2;

    [ContextMenu("update")]
    public void UpdateOrderList()
    {
        for (int i=0; i <  orderListRect.transform.childCount; i++)
        {
            print("delete" + (orderListRect.transform.GetChild(i).name));
            GameObject.Destroy(orderListRect.transform.GetChild(i).gameObject);
        }
        DeliveryBehavior delivery = FindObjectOfType<DeliveryBehavior>();
        foreach (Order order in delivery.activeOrderList)
        {
            GameObject go = GameObject.Instantiate(orderPrefab, orderListRect);
            //go.transform.SetParent(orderListRect);
            OrderGUI goGui = go.GetComponent<OrderGUI>();

            Recipe recipe = order.FoodItems[0]; //TODO make recusive

            List<Image> goGuiImages = new List<Image>();
            goGuiImages.Add(goGui.BreadingIcon1);
            goGuiImages.Add(goGui.BreadingIcon2);

            goGui.RecipeIcon.sprite = order.icon;
            if (recipe.BreadingLayers.Count > 1)
            {
                for (int i=1; i< recipe.BreadingLayers.Count; i++)
                {
                    if (recipe.BreadingLayers[i] == FoodBehavior.BreadingType.Breading)
                    {
                        goGuiImages[i-1].sprite = breading1;
                        goGuiImages[i - 1].color = Color.white;
                    }

                    if (recipe.BreadingLayers[i] == FoodBehavior.BreadingType.Spicy)
                    {
                        goGuiImages[i-1].sprite = breading2;
                        goGuiImages[i - 1].color = Color.white;
                    }
                }
            }
            orderListRect.ForceUpdateRectTransforms();
            LayoutRebuilder.ForceRebuildLayoutImmediate(orderListRect);
            LayoutRebuilder.MarkLayoutForRebuild(orderListRect);
        }
    }

    //void OnEnable ()
    //{
    //    SceneWrangler.sceneLoadComplete += updateWrangler;
    //}

    //private void OnDisable()
    //{
    //    SceneWrangler.sceneLoadComplete -= updateWrangler;
    //}

    // Update is called once per frame
    void updateWrangler()
    {
        wrangler = FindObjectOfType<SceneWrangler>();
    }

    public void restartLevel()
    {
        updateWrangler();
        wrangler.restart();
    }

    public void quitLevel()
    {
        updateWrangler();
        Assert.IsNotNull(menuContainer, "menu must be assigned in the inspector");
        wrangler.setTarget(menuContainer);
        wrangler.loadTargetContainer();
    }
}
