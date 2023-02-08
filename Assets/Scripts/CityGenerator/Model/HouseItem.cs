using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.MenuItem
{
    [CreateAssetMenu(fileName = "HouseItem", menuName = "Buildings/Create House Item")]
    public class HouseItem : ScriptableObject
    {
        public List<WallItem> wallItems;
        public int type;
    }
}