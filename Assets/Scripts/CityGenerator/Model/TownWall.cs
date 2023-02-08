using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownWall
{
    public Vector3 start;
    public Vector3 end;
    public GameObject gameObject;

    public Vector3 center
    {
        get
        {
            return (start + end) / 2;
        }
    }

    public float distance
    {
        get
        {
            return Vector3.Distance(start, end);
        }
    }
}
