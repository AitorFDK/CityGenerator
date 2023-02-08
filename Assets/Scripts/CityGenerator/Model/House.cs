using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GFG;
using CityGen.MenuItem;

namespace CityGen.Model
{

    public enum PolygonOrientation { left, right };


    public class House
    {
        public Vector3 center;

        private List<Vector3> _vertices;
        public List<Vector3> vertices
        {
            get { return _vertices; }
            set
            {
                _vertices = value;
                _area = -1f;
            }
        }

        public int floors;
        public List<Wall> walls;
        public GameObject gameObject;
        public Roof roof;
        public HouseItem houseConfig { get; private set; }

        private float _area;

        public float area
        {
            get
            {
                if (_area > -1f) return _area;
                CalculateArea();
                return _area;
            }
        }

        public House(HouseItem houseConfig)
        {
            center = Vector3.zero;
            vertices = new List<Vector3>();
            walls = new List<Wall>();
            gameObject = null;
            this.houseConfig = houseConfig;
        }

        public void DisplaceWalls(float value)
        {
            // ------ legacy -------- //

            // for (int i = 0; i < vertices.Count; i++)
            // {
            //     Vector3 dir = center - vertices[i];
            //     vertices[i] += dir.normalized * value;
            // }

            // ------- bien hecho (espero) ------- //

            Point[] polygon = new Point[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                polygon[i] = new Point(vertices[i].x, vertices[i].z);
            }

            Vector3[] nuevos = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 ant = vertices[i == 0 ? vertices.Count - 1 : i - 1];
                Vector3 next = vertices[i == vertices.Count - 1 ? 0 : i + 1];

                Vector3 v1 = (ant - current).normalized;
                Vector3 v2 = (next - current).normalized;

                float angle = Vector3.SignedAngle(v1, v2, Vector3.up) / 2;
                Vector3 vCenter = Quaternion.AngleAxis(angle, Vector3.up) * v1;

                Vector3 point = current + vCenter * 3;

                if (!GFG.isInside(polygon, polygon.Length, new Point(point.x, point.z)))
                {
                    vCenter = -vCenter;
                }

                nuevos[i] = current + vCenter * value;
            }

            vertices = new List<Vector3>(nuevos);
        }


        public PolygonOrientation Orientation()
        {
            if (vertices == null || vertices.Count < 3) throw new Exception("Minimum of 3 vertices needed to calculate the orientation");

            Vector3 a = vertices[2] - vertices[1];
            Vector3 b = vertices[0] - vertices[1];

            Vector3 n = Vector3.Cross(a, b);

            return n.y >= 0 ? PolygonOrientation.right : PolygonOrientation.left;
        }

        public void RemoveSmallWalls(float minimumDistance)
        {
            int i = 1;
            while (i <= vertices.Count)
            {
                int ip = i < vertices.Count ? i : 0;

                float dist = Vector3.Distance(vertices[i - 1], vertices[ip]);
                if (dist <= minimumDistance)
                {
                    Debug.LogFormat("Vector de distancia {0} entre {1} y {2} eliminado", dist, vertices[i - 1].ToString(), vertices[ip].ToString());
                    vertices.RemoveAt(ip);
                }
                else
                    i++;
            }
        }

        // Convertir n paredes que van en la misma direccion en 1
        public void CollapseSameWalls(float deltaAngle = 5f)
        {

            List<Vector3> nuevos = new List<Vector3>();

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 ant = vertices[i == 0 ? vertices.Count - 1 : i - 1];
                Vector3 next = vertices[i == vertices.Count - 1 ? 0 : i + 1];

                Vector3 v1 = (ant - current).normalized;
                Vector3 v2 = (next - current).normalized;

                float angle = Mathf.Abs(Vector3.SignedAngle(v1, v2, Vector3.up));

                if (!(angle < 180 + deltaAngle && angle > 180 - deltaAngle))
                    nuevos.Add(current);
            }

            vertices = nuevos;

        }

        // fuente codigo: https://answers.unity.com/questions/684909/how-to-calculate-the-surface-area-of-a-irregular-p.html
        private void CalculateArea()
        {
            float temp = 0;
            int i = 0;
            for (; i < vertices.Count; i++)
            {
                if (i != vertices.Count - 1)
                {
                    float mulA = vertices[i].x * vertices[i + 1].z;
                    float mulB = vertices[i + 1].x * vertices[i].z;
                    temp = temp + (mulA - mulB);
                }
                else
                {
                    float mulA = vertices[i].x * vertices[0].z;
                    float mulB = vertices[0].x * vertices[i].z;
                    temp = temp + (mulA - mulB);
                }
            }
            temp *= 0.5f;
            _area = Mathf.Abs(temp);
        }

    }
}