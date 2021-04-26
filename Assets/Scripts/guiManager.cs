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
    public Texture2D breading1;
    public Texture2D breading2;

    [ContextMenu("update")]
    void UpdateOrderList()
    {
        for (int i=0; i <  orderListRect.transform.childCount; i++)
        {
            print("delete" + (orderListRect.transform.GetChild(i).name));
            GameObject.Destroy(orderListRect.transform.GetChild(i).gameObject);
        }
        DeliveryBehavior delivery = FindObjectOfType<DeliveryBehavior>();
        foreach (Order order in delivery.activeOrderList)
        {
            GameObject go = GameObject.Instantiate(orderPrefab);
            go.transform.SetParent(orderListRect);
            OrderGUI goGui = go.GetComponent<OrderGUI>();
            
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
