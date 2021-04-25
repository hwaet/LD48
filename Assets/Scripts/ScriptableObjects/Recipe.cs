using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Recipe : ScriptableObject
{
    public FoodBehavior.FoodType foodType;
    public List<FoodBehavior.BreadingType> BreadingLayers;

    public override string ToString() {
        return string.Format("Done {0} with {1}", foodType, string.Join(",", BreadingLayers));
    }
}
