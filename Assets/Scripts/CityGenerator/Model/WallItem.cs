using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityGen.MenuItem
{

    public enum WallItemType { wall, window, door }

    [Serializable]
    public struct Neighbour
    {
        public WallItem item;
        public float probability;
    }

    [CreateAssetMenu(fileName = "WallItem", menuName = "Buildings/Create Wall Item")]
    public class WallItem : ScriptableObject
    {
        public GameObject prefab;
        public WallItemType type;

        [Tooltip("Define if these wall item is only available in a specific floor.\n0 means there is no capability")]
        public int floorCap;
        public List<Neighbour> neighbours;


        public Neighbour SelectNeighbour(int floor)
        {

            List<Neighbour> list = neighbours.Where(l => l.item.floorCap == 0 || l.item.floorCap == floor).ToList();

            float max = 0;
            List<Neighbour> acumultated = new List<Neighbour>();

            foreach (Neighbour n in list)
            {

                if (n.probability > 0)
                    max += n.probability;
                else
                    max += 1;

                Neighbour n2 = new Neighbour();
                n2.item = n.item;
                n2.probability = max;

                acumultated.Add(n2);
            }

            float rng = UnityEngine.Random.Range(0, max);

            int res = 0;
            for (int i = 0; i < acumultated.Count; i++)
            {
                if (rng < acumultated[i].probability)
                {
                    res = i;
                    break;
                }
            }

            return list[res];
        }

    }

}