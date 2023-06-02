using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using CityGen.Utils;
using CityGen.Model;
using CityGen.MenuItem;
using Util.Optimization;
using static CityGen.Utils.FillSegment;

namespace CityGen
{

    public class JsonToCity : MonoBehaviour
    {
        [Header("Ciudad")]
        public TextAsset jsonFile;
        public bool renderizarParcela;
        public bool relativeToThis;
        public LayerMask groundLayerMask;
        public bool generateOnRun = true;
        public bool createMeshesOnData;

        [Header("Options")]
        public bool generateBuildings = true;
        public bool generateWall = true;

        [Header("Muralla")]
        public float wallHeight = 7.0f;
        public bool fillWithBlocks = false;
        [Header("Solo si se hace con meshes")]
        public Material wallMaterial;
        public Mesh[] wallMeshes;
        [Header("Solo si se hace con prefabs")]
        public FillBlockSelection wallObjectSelection;
        public List<GameObject> wallObjects;
        public GameObject towerGo;
        public GameObject doorGo;
        public List<Material> doorMaterials;

        [Header("Casas")]
        public float paredSize;
        public float minParedSize;
        public int maxPisos;
        public int seed = 0;
        public HouseItem[] houseConfig;
        public bool delete3WallsBuildings;
        public bool deleteSmallBuildings;
        public float minBuildSize;

        [Header("Roofs")]
        public List<RoofTile> roofTiles;
        public List<MaterialStruct> roofMaterials;
        public Material triangleMaterialMadera;
        public Material triangleMaterialPiedra;
        public Material vigaMaterial;

        [System.Serializable]
        public struct MaterialStruct
        {
            public List<Material> materials;
        }

        private float minArea = 100000000;

        private List<House> houseList = new List<House>();

        private List<Vector3> roadVertList = new List<Vector3>();
        private List<List<Vector3>> wallVertList = new List<List<Vector3>>();
        private List<TownWall> wallList = new List<TownWall>();
        private List<TownWall> wallDoorList = new List<TownWall>();

        // Start is called before the first frame update
        void Start()
        {
            if (generateOnRun) Generar();
        }

        public void Minify()
        {
            Transform t = transform;

            int l = t.childCount;
            for (int i = 0; i < l; i++)
            {
                SetParent(t, t.GetChild(i));
            }
        }

        private void SetParent(Transform parent, Transform child)
        {
            child.parent = parent;

            if (child.gameObject.GetComponent<LODGroup>() != null) return;

            // if (child.childCount == 0) return;

            for (int i = 0; i < child.childCount; i++)
            {
                SetParent(parent, child.GetChild(i));
            }
        }

        public void Clear()
        {
            while (transform.childCount > 0)
            {
                if (Application.isEditor)
                    DestroyImmediate(transform.GetChild(0).gameObject);
                else
                    Destroy(transform.GetChild(0));
            }

            houseList = new List<House>();
            roadVertList = new List<Vector3>();
            wallVertList = new List<List<Vector3>>();
            wallList = new List<TownWall>();
            wallDoorList = new List<TownWall>();

        }

        public void Generar()
        {
            if (this.transform.position.y != 0)
            {
                Transform t = this.transform;
                t.position = new Vector3(t.position.x, 0, t.position.z);
            }

            Random.InitState(seed);
            StartCoroutine(GenerateCity());

        }


        IEnumerator GenerateCity()
        {
            //FeatureBase fb = GetJson();
            JSONNode jn = GetJson();
            Quaternion thisRotation = this.gameObject.transform.rotation;
            // this.gameObject.transform.rotation = Quaternion.identity;

            MeshCombiner.SetCreateAssetsInData(createMeshesOnData);

            Matrix4x4 trs = Matrix4x4.TRS(this.gameObject.transform.position, thisRotation, this.transform.localScale);

            foreach (JSONNode n in jn["features"])
            {
                switch (n["id"].Value)
                {
                    case "buildings":

                        if (!generateBuildings) continue;

                        JSONNode coords = n["coordinates"];
                        int houseNumber = 0;

                        Vector3 thisPosition = gameObject.transform.position;

                        foreach (JSONArray blocks in coords.AsArray)
                        {
                            foreach (JSONArray builds in blocks.AsArray)
                            {
                                House house = new House(houseConfig[Random.Range(0, houseConfig.Length)]);

                                // ---------------- PARSE JSON ----------------------- //
                                List<Vector3> verts = new List<Vector3>();
                                foreach (JSONArray vert in builds.AsArray)
                                {
                                    JSONArray a = vert.AsArray;

                                    List<JSONNode> aa = a.Children.ToList();
                                    aa[0].Value = aa[0].Value.Replace('.', ',');
                                    aa[1].Value = aa[1].Value.Replace('.', ',');

                                    Vector3 v3 = trs.MultiplyPoint(new Vector3(aa[0].AsFloat, 0, aa[1].AsFloat));

                                    verts.Add(v3);
                                    house.vertices.Add(v3);
                                }

                                // ------------------ BUILD HOUSE ------------------------//
                                house.floors = Random.Range(1, maxPisos + 1); // +1 bcs is exclusive

                                Vector3 center = FindCenter(house.vertices);
                                house.center = center; // add center to house

                                house.DisplaceWalls(Random.Range(0.3f, 1.2f)); // give variation to the walls position relative to center

                                house.RemoveSmallWalls(minParedSize); // Remove the smaller walls
                                house.CollapseSameWalls(8f);

                                if (delete3WallsBuildings && house.vertices.Count <= 3)
                                    continue;

                                if (deleteSmallBuildings && house.area < minBuildSize)
                                {
                                    Debug.Log("House with area " + house.area + " deleted");
                                    continue;
                                }

                                if (house.area < minArea)
                                {
                                    minArea = house.area;
                                }

                                FillSegment.BuildHouse(house, paredSize, Quaternion.Euler(0, -90, 0));
                                house.gameObject.transform.parent = gameObject.transform;

                                yield return new WaitForSecondsRealtime(.01f);

                                Roof r = new Roof(house, paredSize, createMeshesOnData);
                                r.ChooseRoofType();

                                Material matTriangulo = house.houseConfig.type == 1 ? triangleMaterialMadera : triangleMaterialPiedra;
                                switch (r.type)
                                {
                                    case RoofType.triangular:
                                        r.GenerateTriangularVertices(roofTiles.ToList(), roofMaterials[Random.Range(0, roofMaterials.Count)].materials, new Vector2(.55f, .55f), matTriangulo, vigaMaterial);
                                        break;
                                    case RoofType.inclinated:
                                        r.GenerateInclinatedVertices(roofTiles.ToList(), roofMaterials[Random.Range(0, roofMaterials.Count)].materials, new Vector2(.55f, .55f), matTriangulo, vigaMaterial);
                                        break;
                                    case RoofType.bevel:
                                        r.GenerateBevelVertices(roofTiles.ToList(), roofMaterials[Random.Range(0, roofMaterials.Count)].materials, new Vector2(.55f, .55f), vigaMaterial);
                                        break;
                                }

                                houseList.Add(house);

                                // --------------- RENDER PARCELA ------------------ //
                                if (renderizarParcela)
                                {
                                    GameObject i = new GameObject("Parcela_" + house + "_" + center.ToString());
                                    if (relativeToThis)
                                        i.transform.position = center + thisPosition;
                                    else
                                        i.transform.position = center;

                                    i.transform.parent = gameObject.transform;
                                    i.transform.Translate(Vector3.up / 10);

                                    MeshFilter mf = i.AddComponent<MeshFilter>();

                                    Mesh m = CreateMesh(verts);

                                    mf.mesh = m;

                                    Renderer rend = i.AddComponent<MeshRenderer>();
                                    rend.material.shader = Shader.Find("HDRP/Lit");
                                    Color c = new Color(UnityEngine.Random.Range(0.8f, 0.9f), UnityEngine.Random.Range(0.8f, 0.9f), UnityEngine.Random.Range(0.8f, 0.9f), 1f);
                                    rend.material.SetColor("_BaseColor", c);
                                }

                                // contador casas creadas
                                houseNumber++;
                            }
                        }

                        Debug.Log(houseNumber + " houses created");
                        Debug.Log(Roof.tilesCreated + " tiles created");
                        Debug.Log("house with min area: " + minArea);

                        break;

                    case "walls":

                        if (!generateWall) continue;

                        JSONNode walls = n["geometries"];

                        thisPosition = gameObject.transform.position;
                        int wi = 0;
                        foreach (JSONNode test in walls.AsArray)
                        {
                            wallVertList.Add(new List<Vector3>());

                            JSONNode coordsB = test["coordinates"];
                            float width = test["width"];

                            foreach (JSONArray builds in coordsB.AsArray)
                            {
                                foreach (JSONArray vert in builds.AsArray)
                                {
                                    JSONArray a = vert.AsArray;

                                    List<JSONNode> aa = a.Children.ToList();
                                    aa[0].Value = aa[0].Value.Replace('.', ',');
                                    aa[1].Value = aa[1].Value.Replace('.', ',');

                                    Vector3 v3 = new Vector3(aa[0].AsFloat, 0, aa[1].AsFloat);

                                    if (relativeToThis)
                                    {
                                        v3 = trs.MultiplyPoint(v3);
                                    }

                                    RaycastHit hit;
                                    if (Physics.Raycast(v3 + Vector3.up * 1000, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
                                    {
                                        v3 = hit.point;
                                    }

                                    wallVertList[wi].Add(v3);
                                }

                            }

                            wi++;
                        }
                        break;


                    case "roads":

                        JSONNode roads = n["geometries"];

                        List<Vector3> roadPoints = new List<Vector3>();

                        foreach (JSONNode r in roads.AsArray)
                        {
                            JSONNode coordsR = r["coordinates"];

                            foreach (JSONArray roadxy in coordsR.AsArray)
                            {
                                roadxy[0].Value = roadxy[0].Value.Replace('.', ',');
                                roadxy[1].Value = roadxy[1].Value.Replace('.', ',');

                                Vector3 v3 = new Vector3(roadxy[0].AsFloat, 0, roadxy[1].AsFloat);

                                if (relativeToThis)
                                {
                                    v3 = trs.MultiplyPoint(v3);
                                }

                                RaycastHit hit;
                                if (Physics.Raycast(v3 + Vector3.up * 1000, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
                                {
                                    v3 = hit.point;
                                }

                                roadVertList.Add(v3);


                            }

                        }
                        break;
                }
            }

            yield return new WaitForSecondsRealtime(.01f);

            AddDoorsToWall();

            DibujarMurallas();

            DibujarPuertasMurallas();

            AdpatarCasasATerreno();
#if UNITY_EDITOR
            if (createMeshesOnData) SceneDataUtil.SaveAssets();
#endif
        }

        void AddDoorsToWall()
        {

            for (int i = 0; i < wallVertList.Count; i++)
            {
                for (int j = 0; j < wallVertList[i].Count; j++)
                {

                    if (roadVertList.Any(p => p == wallVertList[i][j]))
                    {

                        int iPrev = j > 0 ? j - 1 : wallVertList[i].Count - 1;
                        int iNext = j < wallVertList[i].Count - 1 ? j + 1 : 0;

                        Vector3 prevDir = (wallVertList[i][iPrev] - wallVertList[i][j]).normalized;
                        Vector3 nextDir = (wallVertList[i][iNext] - wallVertList[i][j]).normalized;

                        wallVertList[i].Insert(j, wallVertList[i][j]);

                        wallVertList[i][j] += prevDir * 18;
                        wallVertList[i][j + 1] += nextDir * 18;

                        TownWall obj = new TownWall();
                        obj.start = wallVertList[i][j];
                        obj.end = wallVertList[i][j + 1];
                        wallDoorList.Add(obj);
                    }
                }
            }
        }

        void DibujarMurallas()
        {
            Vector3 anterior = Vector3.zero;
            TownWall tw;
            GameObject wall;
            int iName = 0;
            int jName = 0;

            HashSet<Vector3> towersDrawn = new HashSet<Vector3>();

            /// DRAW WALLS

            for (int wi = 0; wi < wallVertList.Count; wi++)
            {
                anterior = Vector3.zero;

                foreach (Vector3 vw in wallVertList[wi])
                {
                    if (anterior != Vector3.zero)
                    {
                        tw = new TownWall();
                        tw.start = anterior;
                        tw.end = vw;

                        if (!wallList.Any(p => (p.end == tw.end && p.start == tw.start) || (p.end == tw.start && p.start == tw.end)) &&
                            !wallDoorList.Any(p => (p.end == tw.end && p.start == tw.start) || (p.end == tw.start && p.start == tw.end)))
                        {

                            if (!fillWithBlocks)
                                wall = FillSegment.FillSegmentWithMeshes(anterior, vw, wallMeshes, wallMaterial);
                            else
                                wall = FillSegment.FillSegmentWithBlock(wallObjects, anterior, vw, wallObjectSelection, 1.3f, Quaternion.identity);

                            wall.name = "Muralla-" + (iName++);
                            wall.transform.parent = this.gameObject.transform;

                            if (!fillWithBlocks && createMeshesOnData)
                            {
#if UNITY_EDITOR
                                SceneDataUtil.CreateAsset(wall.GetComponent<MeshFilter>().sharedMesh, "Meshes", wall.name + "_mesh.asset");
#endif
                            }

                            GameObject wc = GetWallCollider(tw);
                            wc.transform.parent = wall.transform;

                            tw.gameObject = wall;

                            wallList.Add(tw);
                        }
                    }
                    anterior = vw;
                }

                tw = new TownWall();
                tw.start = anterior;
                tw.end = wallVertList[wi][0];

                if (!wallList.Any(p => (p.end == tw.end && p.start == tw.start) || (p.end == tw.start && p.start == tw.end)))
                {

                    if (!fillWithBlocks)
                        wall = FillSegment.FillSegmentWithMeshes(anterior, wallVertList[wi][0], wallMeshes, wallMaterial);
                    else
                        wall = FillSegment.FillSegmentWithBlock(wallObjects, anterior, wallVertList[wi][0], wallObjectSelection, 1.3f, Quaternion.identity);

                    wall.name = "Muralla-" + (iName++);
                    wall.transform.parent = this.gameObject.transform;


                    if (!fillWithBlocks && createMeshesOnData)
                    {
#if UNITY_EDITOR
                        SceneDataUtil.CreateAsset(wall.GetComponent<MeshFilter>().sharedMesh, "Meshes", wall.name + "_mesh.asset");
#endif
                    }


                    GameObject wc = GetWallCollider(tw);
                    wc.transform.parent = wall.transform;

                    tw.gameObject = wall;

                    wallList.Add(tw);
                }

                /// DRAW WALL TOWERS
                if (towerGo != null)
                {
                    foreach (Vector3 vw in wallVertList[wi])
                    {
                        if (!towersDrawn.Contains(vw))
                        {
                            GameObject go = GameObject.Instantiate(towerGo, vw, new Quaternion());
                            go.name = "WallTower-" + (jName++);
                            go.transform.parent = this.gameObject.transform;

                            towersDrawn.Add(vw);
                        }
                    }
                }
            }
        }

        void DibujarPuertasMurallas()
        {

            HashSet<Vector3> doorsDrawn = new HashSet<Vector3>();
            for (int i = 0; i < wallDoorList.Count; i++)
            {
                if (!doorsDrawn.Contains(wallDoorList[i].center))
                {
                    GameObject go = GameObject.Instantiate(doorGo, wallDoorList[i].center, new Quaternion());
                    go.name = "WallDoor-" + i;

                    // ¡ALERTA! ¡ALERTA! ESTO ESTA HARDCODEADO.
                    if (!fillWithBlocks)
                    {
                        Renderer mr = go.transform.GetChild(2).GetChild(0).GetComponent<Renderer>();
                        Material[] materials = new Material[mr.sharedMaterials.Length];
                        for (int j = 0; j < materials.Length; j++)
                        {
                            materials[j] = doorMaterials[Random.Range(0, doorMaterials.Count)];
                        }
                        mr.sharedMaterials = materials;
                    }
                    // fin del hardcodeo

                    Vector3 direction = (wallDoorList[i].end - wallDoorList[i].start) / 2;

                    go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

                    Vector3 euler = go.transform.rotation.eulerAngles;
                    euler.x = 0;

                    go.transform.rotation = Quaternion.Euler(euler);

                    go.transform.parent = this.gameObject.transform;

                    wallDoorList[i].gameObject = go;

                    doorsDrawn.Add(wallDoorList[i].center);
                }
            }
        }

        GameObject GetWallCollider(TownWall tw)
        {
            GameObject wallCollision = new GameObject("MurallaCollision");
            wallCollision.transform.position = (tw.start + tw.end) / 2 + Vector3.up * wallMeshes[0].bounds.size.y / 2;
            BoxCollider bc = wallCollision.gameObject.AddComponent<BoxCollider>();
            Vector3 size = bc.size;
            size.y = wallMeshes[0].bounds.size.y;
            size.x = wallMeshes[0].bounds.size.x;
            size.z = tw.distance;
            bc.size = size;
            bc.transform.rotation = Quaternion.LookRotation((tw.end - tw.start).normalized, Vector3.up);
            return wallCollision;
        }

        void AdpatarCasasATerreno()
        {

            RaycastHit hit;

            foreach (House h in houseList)
            {

                Vector3 lowerPoint = Vector3.up * 1000;

                int cc = h.gameObject.transform.childCount;

                Transform ht = h.gameObject.transform;

                for (int i = 0; i < cc; i++)
                // foreach (Vector3 v in h.vertices)
                {

                    Transform child = ht.GetChild(i);

                    if (child.parent != ht) continue;



                    if (Physics.Raycast(child.position + Vector3.up * 1000, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
                    {
                        if (hit.point.y < lowerPoint.y)
                            lowerPoint = hit.point;
                    }

                }

                Vector3 vp = h.gameObject.transform.position;
                vp.y = lowerPoint.y;
                h.gameObject.transform.position = vp;
            }

        }

        // Update is called once per frame
        void Update()
        {

        }


        JSONNode GetJson()
        {
            return JSON.Parse(jsonFile.text);
            //   return JsonUtility.FromJson<FeatureBase>(jsonFile.text);
        }

        private Mesh CreateMesh(List<Vector3> points)
        {
            return CreateMesh(points, FindCenter(points));
        }

        private Mesh CreateMesh(List<Vector3> points, Vector3 center)
        {
            int[] tris = new int[points.Count * 3]; // Every 3 ints represents a triangle
            /* 4 points in the list for the square made of two triangles:
            0 *--* 1
              | /|
              |/ |
            3 *--* 2
            */

            //Vector3 center = FindCenter(points);

            Vector3[] vertices = new Vector3[points.Count + 1];
            vertices[0] = Vector3.zero;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 v = points[i];
                // v.z = 0.0f;
                vertices[i + 1] = v - center;
            }


            for (int i = 0; i < points.Count - 1; i++)
            {
                tris[i * 3] = i + 2;
                tris[i * 3 + 1] = 0;
                tris[i * 3 + 2] = i + 1;
            }

            tris[(points.Count - 1) * 3] = 1;
            tris[(points.Count - 1) * 3 + 1] = 0;
            tris[(points.Count - 1) * 3 + 2] = points.Count;

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = BuildUVs(vertices);

            mesh.triangles = tris.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Vector3 FindCenter(List<Vector3> points)
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 v3 in points)
            {
                center += v3;
            }
            return center / points.Count;
        }


        Vector2[] BuildUVs(Vector3[] vertices)
        {

            float xMin = Mathf.Infinity;
            float yMin = Mathf.Infinity;
            float xMax = -Mathf.Infinity;
            float yMax = -Mathf.Infinity;

            foreach (Vector3 v3 in vertices)
            {
                if (v3.x < xMin)
                    xMin = v3.x;
                if (v3.y < yMin)
                    yMin = v3.y;
                if (v3.x > xMax)
                    xMax = v3.x;
                if (v3.y > yMax)
                    yMax = v3.y;
            }

            float xRange = xMax - xMin;
            float yRange = yMax - yMin;

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i].x = (vertices[i].x - xMin) / xRange;
                uvs[i].y = (vertices[i].y - yMin) / yRange;

            }
            return uvs;
        }

    }

}