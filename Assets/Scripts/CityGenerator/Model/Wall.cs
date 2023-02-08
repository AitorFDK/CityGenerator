using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Model
{
    public class Wall
    {
        public Vector3 start;
        public Vector3 end;
        public int floors;
        public GameObject gameObject;
        public float verticalScale;

        public Vector3 normal
        {
            get
            {
                Vector3 dirAux = direction;
                dirAux.y = -dirAux.y;
                return Vector3.Cross(direction, dirAux);
            }
        }

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

        public float angle
        {
            get
            {
                return Vector3.Angle(start, end);
            }
        }

        public Vector3 direction
        {
            get
            {
                return (end - start).normalized;
            }
        }

        public Wall()
        {
            start = Vector3.zero;
            end = Vector3.zero;
            floors = 1;
            verticalScale = 1f;
        }

        public Wall(Vector3 start, Vector3 end, int floors, float verticalScale)
        {
            this.start = start;
            this.end = end;
            this.floors = floors;
            this.verticalScale = verticalScale;
        }
    }

}