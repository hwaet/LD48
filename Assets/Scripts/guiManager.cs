using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class guiManager : MonoBehaviour
{
    SceneWrangler wrangler;
    public SceneContainer menuContainer;

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
