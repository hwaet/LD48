﻿using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SceneManagement;
using UnityEngine;

[CreateAssetMenu]
public class StageSettings_ld48 : StageSettings
{
    public string stageName = "name";

    public List<Order> recipesOrders;

    public GameObject ChickenPrefab;
    public GameObject DuckPrefab;
    public GameObject TurkeyPrefab;
    public GameObject DuckenPrefab;
    public GameObject TurduckenPrefab;
    public GameObject PlatePrefab;


    //[Header("SceneContainers")]
    //public SceneContainer nextSceneContainer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
