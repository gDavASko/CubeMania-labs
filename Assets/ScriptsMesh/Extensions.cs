using System.Collections.Generic;

public static class Extensions
{
    private static System.Random rnd = new System.Random();
    
    public static T Rnd<T>(this List<T> anyArray)
    {
        if (anyArray == null || anyArray.Count == 0)
            return default;
        
        if(anyArray.Count == 1)
            return anyArray[0];
        
        return anyArray[rnd.Next(int.MaxValue) % anyArray.Count];
    }
    
    public static T Rnd<T>(this T[] anyArray)
    {
        if (anyArray == null || anyArray.Length == 0)
            return default;
        
        if(anyArray.Length == 1)
            return anyArray[0];
        
        return anyArray[rnd.Next(int.MaxValue) %  anyArray.Length];
    }
}
