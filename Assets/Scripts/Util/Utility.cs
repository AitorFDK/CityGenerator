using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Utility
{

    /// <summary>
    /// Mis cojones he hecho yo esto, lo he sacado de http://wiki.unity3d.com/index.php?title=Triangulator&_ga=2.225588058.1773793490.1600612646-310516500.1600088647 
    /// </summary>
    public class Triangulator
    {
        private List<Vector2> m_points = new List<Vector2>();

        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }

    public static class Utility
    {
        public static Vector3 Vector3fromVector2(Vector2 v, float z = 0)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Lo primero de todo, buenos dias
        /// Lo segundo, link: https://www.h3xed.com/programming/automatically-create-polygon-collider-2d-from-2d-mesh-in-unity 
        /// </summary>
        public static void AddPolygonColliderFrom2DMesh(GameObject go, Mesh m)
        {
            // Get triangles and vertices from mesh
            int[] triangles = m.triangles;
            Vector3[] vertices = m.vertices;

            // Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
            Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int e = 0; e < 3; e++)
                {
                    int vert1 = triangles[i + e];
                    int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
                    string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
                    if (edges.ContainsKey(edge))
                    {
                        edges.Remove(edge);
                    }
                    else
                    {
                        edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
                    }
                }
            }

            // Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> edge in edges.Values)
            {
                if (lookup.ContainsKey(edge.Key) == false)
                {
                    lookup.Add(edge.Key, edge.Value);
                }
            }

            // Create empty polygon collider
            PolygonCollider2D polygonCollider = go.AddComponent<PolygonCollider2D>();
            polygonCollider.pathCount = 0;

            // Loop through edge vertices in order
            int startVert = 0;
            int nextVert = startVert;
            int highestVert = startVert;
            List<Vector2> colliderPath = new List<Vector2>();
            while (true)
            {

                // Add vertex to collider path
                colliderPath.Add(vertices[nextVert]);

                // Get next vertex
                nextVert = lookup[nextVert];

                // Store highest vertex (to know what shape to move to next)
                if (nextVert > highestVert)
                {
                    highestVert = nextVert;
                }

                // Shape complete
                if (nextVert == startVert)
                {

                    // Add path to polygon collider
                    polygonCollider.pathCount++;
                    polygonCollider.SetPath(polygonCollider.pathCount - 1, colliderPath.ToArray());
                    colliderPath.Clear();

                    // Go to next shape if one exists
                    if (lookup.ContainsKey(highestVert + 1))
                    {

                        // Set starting and next vertices
                        startVert = highestVert + 1;
                        nextVert = startVert;

                        // Continue to next loop
                        continue;
                    }

                    // No more verts
                    break;
                }
            }

            return;
        }

        /// <summary>
        /// Constructor: takes a mesh and fills out vertex and triangle array of mesh
        /// Zelda: http://www.riccardostecca.net/1/save-and-load-mesh-data-in-unity/ 
        /// </summary>
        public static void SerializableMeshInfo(Mesh m, out float[] vertices, out int[] triangles)
        {
            vertices = new float[m.vertexCount * 3]; // initialize vertices array.
            for (int i = 0; i < m.vertexCount; i++) // Serialization: Vector3's values are stored sequentially.
            {
                vertices[i * 3] = m.vertices[i].x;
                vertices[i * 3 + 1] = m.vertices[i].y;
                vertices[i * 3 + 2] = m.vertices[i].z;
            }
            triangles = new int[m.triangles.Length]; // initialize triangles array
            for (int i = 0; i < m.triangles.Length; i++) // Mesh's triangles is an array that stores the indices, sequentially, of the vertices that form one face
            {
                triangles[i] = m.triangles[i];
            }
        }

        /// <summary>
        /// GetMesh gets a Mesh object from currently set data in this SerializableMeshInfo object.
        /// Sequential values are deserialized to Mesh original data types like Vector3 for vertices.
        /// Take this sword, is dangerous outside: http://www.riccardostecca.net/1/save-and-load-mesh-data-in-unity/ 
        /// </summary>
        /// <returns>The mesh created</returns>
        public static Mesh GetMesh(float[] vertices, int[] triangles)
        {
            Mesh m = new Mesh();
            List<Vector3> verticesList = new List<Vector3>();
            for (int i = 0; i < vertices.Length / 3; i++)
            {
                verticesList.Add(new Vector3(
                        vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]
                    ));
            }
            m.SetVertices(verticesList);
            m.triangles = triangles;

            m.RecalculateNormals();

            return m;
        }

        /// <summary>
        /// Get component in children adaptation to take in account inactive objects
        /// by: Me (por una vez en todo este fichero)
        /// </summary>
        /// <param name="root"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponentInAllChildren<T>(GameObject root)
        {
            foreach (Transform t in root.transform)
            {
                T aux = t.GetComponent<T>();

                if (aux != null)
                {
                    return aux;
                }
            }

            return default(T);
        }
    }
}