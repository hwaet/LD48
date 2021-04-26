using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Order : ScriptableObject
{
    public string recipeName;
    public Sprite icon;
    public List<Recipe> FoodItems;
    public float patience;

/*    public List<FoodBehavior.FoodType> foodTypeList;

    private void OnValidate()
    {
        foodTypeList.Clear();
        foreach (Recipe food in FoodItems)
        {
            foodTypeList.Add(food.foodName);
        }
        foodTypeList.Sort();
    }*/

    public bool CheckDelivery(PlateBehavior deliveredPlate) {
        //check size of food list
        //if (deliveredPlate.contents.Count != FoodItems.Count) return false;

        //check names of foods
        //List<FoodBehavior.FoodType> foodTypes = new List<FoodBehavior.FoodType>();
        List<Recipe> satisfied = new List<Recipe>();
        
        foreach (FoodBehavior food in deliveredPlate.contents) {

            //REALLY Hard constraint that all food must be perfectly done to be considered
            if(food.doneness != FoodBehavior.Doneness.Cooked) {
                continue;
            }

            foreach(Recipe recipe in FoodItems) {
                // if this recipe is already satsified skip it
                if (satisfied.Contains(recipe)) {
                    continue;
                }
                //if the food type isn't the target skip it
                if (recipe.foodType != food.foodType) {
                    continue;
                }

                // if different breading count skip it
                if(recipe.BreadingLayers.Count != food.BreadingLayers.Count) {
                    continue;
                }

                if (recipe.BreadingLayers.SequenceEqual(food.BreadingLayers)) {
                    satisfied.Add(recipe);
                    break;
                }
            }
        }
        return satisfied.Count == FoodItems.Count;
    }
}
