using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T Rnd<T>(this List<T> anyArray)
    {
        if (anyArray == null || anyArray.Count == 0)
            return default;
        
        if(anyArray.Count == 1)
            return anyArray[0];
        
        return anyArray[Random.Range(0, anyArray.Count)];
    }
    
    public static T Rnd<T>(this T[] anyArray)
    {
        if (anyArray == null || anyArray.Length == 0)
            return default;
        
        if(anyArray.Length == 1)
            return anyArray[0];
        
        return anyArray[Random.Range(0, anyArray.Length)];
    }
}
