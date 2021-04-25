using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

[CreateAssetMenu]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Texture2D icon;
    public List<RecipeComponent> FoodItems;
    public List<FoodBehavior.FoodType> foodTypeList;

    private void OnValidate()
    {
        foodTypeList.Clear();
        foreach (RecipeComponent food in FoodItems)
        {
            foodTypeList.Add(food.foodName);
        }
        foodTypeList.Sort();
    }

    public bool compareAgainstFood(List<FoodBehavior> deliveredFood)
    {
        //check size of food list
        if (deliveredFood.Count != FoodItems.Count) return false;

        //check names of foods
        List<FoodBehavior.FoodType> foodTypes = new List<FoodBehavior.FoodType>();
        foreach (FoodBehavior food in deliveredFood)
        {
            foodTypes.Add(food.foodType);
        }
        foodTypes.Sort();
        if (areListsEqual(foodTypeList,foodTypes) == false)
        {
            Debug.Log(this.recipeName + "Name mismatch on food");
            return false; 
        }

        //checks donenes
        foreach (FoodBehavior food in deliveredFood)
        {
            if (food.doneness != FoodBehavior.Doneness.Cooked)
            {
                Debug.Log(this.recipeName + "Food is over/under done");
                return false;
            }
        }

        //checks breadiness
        //TODO

        //checks stuffing
        //TODO

        return true;
    }

    bool areListsEqual(List<FoodBehavior.FoodType> lista, List<FoodBehavior.FoodType> listb)
    {
        if (lista.Count != listb.Count) return false;

        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i] != listb[i]) return false;
        }

        return true;
    }
}
