using System.Collections.Generic;

public static class PlayerSessionProgress
{
    public static int Point;
    public static HashSet<int> CollectedCheckpoints = new();

    public static void Reset()
    {
        Point = 0;
        CollectedCheckpoints.Clear();
    }
}