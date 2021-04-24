using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RecipeComponent : ScriptableObject
{
    public string foodName;
    public List<string> breadings;
    public RecipeComponent stuffings;

}
