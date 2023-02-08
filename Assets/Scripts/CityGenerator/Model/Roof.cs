
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Util.Optimization;

namespace CityGen.Model
{

    public enum RoofType
    {
        triangular, inclinated, bevel
    }

    public class Roof
    {

        public static int tilesCreated = 0;

        public House house { get; set; }
        public RoofType type { get; set; }
        public GameObject gameObject { get; set; }
        public Vector3 vertices { get; set; }
        public float roofHeight { get; set; }
        public float maxHeight { get; set; } // tamaño pared

        private bool _createAssetsInFolder;

        public Roof(House house, float maxHeight, bool createAssetsInFolder = false)
        {
            this.house = house;
            this.maxHeight = maxHeight;
            this._createAssetsInFolder = createAssetsInFolder;
        }

        public void ChooseRoofType()
        {
            if (house.walls.Count == 4)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        this.type = RoofType.triangular;
                        break;
                    case 1:
                        this.type = RoofType.inclinated;
                        break;
                }
            }
            else
            {
                this.type = RoofType.bevel;
            }

        }

        public void GenerateTriangularVertices(List<RoofTile> tiles, List<Material> tilesMaterials, Vector2 tileSize, Material triangleMat, Material vigaMat)
        {

            this.gameObject = new GameObject("TriangularRoof(" + house.center + ")");
            this.gameObject.transform.position = house.center;

            this.gameObject.transform.parent = house.gameObject.transform;


            Vector3 center = house.center;

            if (house.walls.Count != 4) return;

            List<Wall> ordered = house.walls.OrderBy(p => p.distance).ToList();

            List<Wall> corta = new List<Wall> { ordered[0], ordered.First(p => ordered[0].start != p.start && ordered[0].start != p.end && ordered[0].end != p.start && ordered[0].end != p.end) };
            // List<Wall> largo = ordered.Except(corta).ToList();

            float min = maxHeight * corta[0].verticalScale / 2;
            float max = maxHeight * corta[0].verticalScale;

            Vector3 centroCorto1 = corta[0].center;
            centroCorto1.y = corta[0].floors * max;

            Vector3 centroCorto2 = corta[1].center;
            centroCorto2.y = corta[1].floors * max;

            float distanciaViga = Vector3.Distance(centroCorto1, centroCorto2);

            // viga principal
            GameObject viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaPrincipal";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.7f, .7f, distanciaViga + 1f);
            viga.transform.position = (centroCorto1 + centroCorto2) / 2;
            viga.transform.rotation = Quaternion.LookRotation(centroCorto2 - centroCorto1, Vector3.up);
            roofHeight = Random.Range(min, max);
            viga.transform.position = new Vector3(viga.transform.position.x, viga.transform.position.y + roofHeight, viga.transform.position.z);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            // Crear vertices

            Vector3 v1 = corta[0].start;
            v1.y = centroCorto1.y;

            Vector3 v2 = corta[0].end;
            v2.y = centroCorto1.y;

            Vector3 v3 = centroCorto1;
            v3.y = viga.transform.position.y;

            Vector3 v4 = corta[1].start;
            v4.y = centroCorto2.y;

            Vector3 v5 = corta[1].end;
            v5.y = centroCorto2.y;

            Vector3 v6 = centroCorto2;
            v6.y = viga.transform.position.y;

            v3 -= new Vector3(0, 0.15f, 0);
            v6 -= new Vector3(0, 0.15f, 0);

            // subir los vertices inferiores para que no solape con la pared
            Vector3 direction = (v4 - v2).normalized;
            Vector3 cross = Vector3.Cross((v3 - v2).normalized, direction).normalized;
            v2 -= cross * 0.3f;
            v2 -= Vector3.up * .1f; // bajar la viga de madera
            v4 -= cross * 0.3f;
            v4 -= Vector3.up * .1f; // bajar la viga de madera

            direction = (v1 - v5).normalized;
            cross = Vector3.Cross((v6 - v5).normalized, direction).normalized;
            v5 -= cross * 0.3f;
            v5 -= Vector3.up * .1f; // bajar la viga de madera
            v1 -= cross * 0.3f;
            v1 -= Vector3.up * .1f; // bajar la viga de madera


            // vigas triangulo 1

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaTriangulo1-1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v2, v3));
            viga.transform.position = (v2 + v3) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v3 - v2, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaTriangulo1-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v1, v3));
            viga.transform.position = (v1 + v3) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v3 - v1, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaTriangulo1-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .45f, Vector3.Distance(v1, v2));
            viga.transform.position = (v1 + v2) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v2 - v1, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            // vigas triangulo 2

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaTriangulo2-1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v4, v6));
            viga.transform.position = (v4 + v6) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v6 - v4, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaTriangulo2-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v5, v6));
            viga.transform.position = (v5 + v6) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v6 - v5, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaTriangulo2-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .45f, Vector3.Distance(v4, v5));
            viga.transform.position = (v4 + v5) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v5 - v4, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            // vigas uniones pared

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaUnion1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .4f, Vector3.Distance(v2, v4));
            viga.transform.position = (v2 + v4) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v4 - v2, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaUnion2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .4f, Vector3.Distance(v5, v1));
            viga.transform.position = (v5 + v1) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v1 - v5, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            v3 += new Vector3(0, 0.15f, 0);
            v6 += new Vector3(0, 0.15f, 0);

            // volver a subir los puntos de las vigas de madera
            v1 += Vector3.up * .1f;
            v2 += Vector3.up * .1f;
            v4 += Vector3.up * .1f;
            v5 += Vector3.up * .1f;

            // ----------------- RENDER DE TRINAGULO TEJADO -----------------

            Mesh m = new Mesh();

            m.vertices = new Vector3[] { v1, v2, v3, v4, v5, v6 };

            m.triangles = new int[] {
            0, 1, 2,
            5, 3, 4
        };

            m.uv = BuildUVs(m.vertices);
            m.RecalculateNormals();
            m.RecalculateBounds();

            
            MeshHelper.Subdivide4(m);
            MeshHelper.Subdivide4(m);

            GameObject instanced = new GameObject("RoofTriangles_" + this.gameObject.transform.position);
            // instanced.transform.position = viga.transform.position;
            instanced.transform.parent = this.gameObject.transform;

            MeshFilter mf = instanced.AddComponent<MeshFilter>();
            mf.sharedMesh = m;

            if (_createAssetsInFolder){
#if UNITY_EDITOR
                SceneDataUtil.CreateAsset(mf.sharedMesh, "Meshes",  instanced.name + "_mesh.asset");
#endif
            }

            Renderer rend = instanced.AddComponent<MeshRenderer>();
            rend.material = triangleMat;

            // ----------------- RENDER DE VERDAD -----------------


            direction = (v4 - v2).normalized;
            cross = Vector3.Cross((v3 - v2).normalized, direction).normalized;
            v2 -= direction * 0.2f;
            v4 += direction * 0.2f;
            v2 -= cross * 0.1f;
            v4 -= cross * 0.1f;

            direction = (v1 - v5).normalized;
            cross = Vector3.Cross((v6 - v5).normalized, direction).normalized;
            v5 -= direction * 0.2f;
            v1 += direction * 0.2f;
            v5 -= cross * 0.1f;
            v1 -= cross * 0.1f;

            // colocar entre v2 i v4
            GameObject roof = BuildRoofTiles(v2, v3, v4, v6, tiles, tilesMaterials, tileSize);
            roof.transform.parent = this.gameObject.transform;

            MeshCombiner.CombineWithDiferentsMaterials(roof.transform);

            // colocar entre v5 i v1  
            roof = BuildRoofTiles(v5, v6, v1, v3, tiles, tilesMaterials, tileSize);
            roof.transform.parent = this.gameObject.transform;

            MeshCombiner.CombineWithDiferentsMaterials(roof.transform);

        }

        /*****************************************************************************************

            Colocar en una cara de 4 vertices todas tejas indicadas aleatoriamente

            Orden vertices:

                v1 ------ v3
                 \         \
                  v0 ------ v2

            Devuelve un game object que tiene todas las tejas como child

        *****************************************************************************************/
        private GameObject BuildRoofTiles(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, List<RoofTile> tiles, List<Material> tilesMaterials, Vector2 tileSize)
        {

            List<GameObject> gos = new List<GameObject>();

            Vector3 leftDir = (v1 - v0).normalized;
            float leftDist = Vector3.Distance(v1, v0);

            Vector3 rightDir = (v3 - v2).normalized;
            float rightDist = Vector3.Distance(v3, v2);

            // vertical
            int nBlocksV = (int)Mathf.Round((leftDist / tileSize.y + rightDist / tileSize.y) / 2);

            float lBlocksSizeV = nBlocksV * tileSize.y;
            float lScaleV = leftDist / lBlocksSizeV;
            float lAngle = Vector3.Angle(v1 - v0, Vector3.up);

            float rBlocksSizeV = nBlocksV * tileSize.y;
            float rScaleV = rightDist / rBlocksSizeV;
            float rAngle = Vector3.Angle(v3 - v2, Vector3.up);

            GameObject roofgo = new GameObject("Roof(" + v0 + " to " + v2 + ")");
            GameObject go;
            roofgo.transform.position = v0;
            roofgo.transform.rotation = Quaternion.LookRotation((v2 - v0), Vector3.up);

            Vector3 rot, inicio, fin, position, direction, vScale;
            float distance, blocksSize, scale;
            int nBlocks;
            Material mat;

            for (int i = 0; i <= nBlocksV; i++) // vertical
            {

                inicio = v0 + leftDir * tileSize.y * lScaleV * i;
                fin = v2 + rightDir * tileSize.y * rScaleV * i;

                position = inicio;
                direction = (fin - inicio).normalized;

                position += direction * tileSize.x;

                distance = Vector3.Distance(inicio, fin);
                nBlocks = (int)Mathf.Round(distance / tileSize.x);
                blocksSize = nBlocks * tileSize.x;
                scale = distance / blocksSize;

                for (int j = 0; j < nBlocks; j++) // horizontal
                {
                    if (i <= 1 || j == 0 || j == nBlocks - 1)
                        go = GameObject.Instantiate(tiles[Random.Range(0, tiles.Count)].tileCovered, position, Quaternion.LookRotation(direction, Vector3.up));
                    else
                        go = GameObject.Instantiate(tiles[Random.Range(0, tiles.Count)].tile, position, Quaternion.LookRotation(direction, Vector3.up));
                    go.name = "Roof Block: " + i + "-" + j;

                    vScale = Vector3.one;
                    vScale.x = scale;
                    vScale.y = Mathf.Lerp(lScaleV, rScaleV, (float)j / nBlocks);
                    go.transform.localScale = Vector3.Scale(go.transform.localScale, vScale);

                    rot = go.transform.localRotation.eulerAngles;
                    rot.x += Random.Range(1.5f, -1.5f);
                    rot.y += Random.Range(1.5f, -1.5f);
                    rot.z += Random.Range(3f, -3f);
                    rot.z -= Mathf.Lerp(lAngle, rAngle, (float)j / nBlocks);

                    go.transform.localRotation = Quaternion.Euler(rot);

                    mat = tilesMaterials[Random.Range(0, tilesMaterials.Count)];
                    foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                    {
                        r.material = mat;
                    }

                    go.transform.parent = roofgo.transform;

                    // GameObjectUtility.SetStaticEditorFlags(go, staticEditorFlags);

                    position += direction * tileSize.x * scale;

                    tilesCreated++;
                }
            }

            return roofgo;
        }

        public void GenerateInclinatedVertices(List<RoofTile> tiles, List<Material> tilesMaterials, Vector2 tileSize, Material roofMat, Material vigaMat)
        {
            if (house.walls.Count != 4) return;

            this.gameObject = new GameObject("InclinatedRoof(" + house.center + ")");
            this.gameObject.transform.position = house.center;

            List<Wall> ordered = house.walls.OrderBy(p => p.distance).ToList();

            List<Wall> corta = new List<Wall> { ordered[0], ordered.First(p => ordered[0].start != p.start && ordered[0].start != p.end && ordered[0].end != p.start && ordered[0].end != p.end) };

            float min = maxHeight * corta[0].verticalScale / 5;
            float max = maxHeight * corta[0].verticalScale;

            Vector3 centroCorto1 = corta[0].center;
            centroCorto1.y = corta[0].floors * max;

            Vector3 centroCorto2 = corta[1].center;
            centroCorto2.y = corta[1].floors * max;

            roofHeight = Random.Range(min + min, max);

            // Crear vertices

            Vector3 v1 = corta[0].start;
            v1.y = centroCorto1.y;

            Vector3 v2 = corta[0].end;
            v2.y = centroCorto1.y;

            Vector3 v3 = v2;
            v3.y = v2.y + roofHeight;

            Vector3 v4 = v1;
            v4.y = v1.y + roofHeight;

            Vector3 v5 = corta[1].start;
            v5.y = centroCorto2.y;

            Vector3 v6 = v5;
            v6.y += min;

            Vector3 v8 = corta[1].end;
            v8.y = centroCorto2.y;

            Vector3 v7 = v8;
            v7.y += min;


            v3 -= new Vector3(0, 0.15f, 0);
            v4 -= new Vector3(0, 0.15f, 0);
            v6 -= new Vector3(0, 0.15f, 0);
            v7 -= new Vector3(0, 0.15f, 0);

            // vigas cara 1

            GameObject viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara1-1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v2, v3));
            viga.transform.position = (v2 + v3) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v3 - v2, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara1-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v3, v6));
            viga.transform.position = (v3 + v6) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v6 - v3, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara1-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v5, v6));
            viga.transform.position = (v5 + v6) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v6 - v5, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara1-4";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v2, v5));
            viga.transform.position = (v2 + v5) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v5 - v2, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            // vigas cara 2

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara2-1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v5, v6));
            viga.transform.position = (v5 + v6) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v6 - v5, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara2-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v6, v7));
            viga.transform.position = (v6 + v7) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v7 - v6, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara2-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v8, v7));
            viga.transform.position = (v8 + v7) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v7 - v8, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara2-4";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v8, v5));
            viga.transform.position = (v8 + v5) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v5 - v8, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            // vigas cara 3

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara3-1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v7, v4));
            viga.transform.position = (v7 + v4) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v4 - v7, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara3-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v1, v4));
            viga.transform.position = (v1 + v4) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v4 - v1, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara3-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v8, v1));
            viga.transform.position = (v8 + v1) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v1 - v8, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            // vigas cara 4

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara4-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v3, v4));
            viga.transform.position = (v3 + v4) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v4 - v3, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaCara4-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.3f, .3f, Vector3.Distance(v2, v1));
            viga.transform.position = (v2 + v1) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v1 - v2, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());


            v3 += new Vector3(0, 0.15f, 0);
            v4 += new Vector3(0, 0.15f, 0);
            v6 += new Vector3(0, 0.15f, 0);
            v7 += new Vector3(0, 0.15f, 0);


            // -------------- RENDER PAREDES TEJADO ---------------------------

            Mesh m = new Mesh();

            m.vertices = new Vector3[] { v1, v2, v3, v4, v5, v6, v7, v8 };

            m.triangles = new int[] {
            0, 1, 2,
            0, 2, 3,
            2, 1, 4,
            4, 5, 2,
            6, 5, 7,
            5, 4, 7,
            6, 7, 0,
            0, 3, 6
        };

            m.uv = BuildUVs(m.vertices);
            m.RecalculateNormals();
            m.RecalculateBounds();

            GameObject i = new GameObject("RoofInclinated_" + ((centroCorto1 + centroCorto2) / 2));
            i.gameObject.transform.parent = this.gameObject.transform;

            MeshFilter mf = i.AddComponent<MeshFilter>();
            mf.sharedMesh = m;

            if (_createAssetsInFolder){
#if UNITY_EDITOR
                SceneDataUtil.CreateAsset(mf.sharedMesh, "Meshes",  i.name + "_mesh.asset");
#endif
            }

            Renderer rend = i.AddComponent<MeshRenderer>();
            rend.material = roofMat;


            // ---------------- RENDER CUQUI --------------------


            v6 -= (v7 - v6).normalized * 0.4f;
            v7 += (v7 - v6).normalized * 0.4f;

            v3 -= (v4 - v3).normalized * 0.4f;
            v4 += (v4 - v3).normalized * 0.4f;

            this.gameObject.transform.parent = house.gameObject.transform;

            GameObject roof = BuildRoofTiles(v6, v3, v7, v4, tiles, tilesMaterials, tileSize);
            roof.transform.parent = this.gameObject.transform;

            Util.Optimization.MeshCombiner.CombineWithDiferentsMaterials(roof);

        }

        public void GenerateBevelVertices(List<RoofTile> tiles, List<Material> tilesMaterials, Vector2 tileSize, Material vigaMat)
        {

            this.gameObject = new GameObject("BevelRoof(" + house.center + ")");
            this.gameObject.transform.position = house.center;
            this.gameObject.transform.parent = house.gameObject.transform;


            Vector3[] originalVertices = house.vertices.ToArray();


            house.DisplaceWalls(-0.4f);
            Vector3[] baseVertices = house.vertices.ToArray();

            // restarurar vertices casa
            house.vertices = originalVertices.ToList();

            // cojer pared corta
            Wall corta = house.walls.OrderBy(p => p.distance).ToList()[0];

            // randomizar la distancia que se mete para dentro
            float displace = Random.Range(corta.distance / 3, corta.distance / 2);
            // float displace = Random.Range(2.5f, 5f);

            // randomizar altura de la base elevada
            float min = maxHeight * corta.verticalScale * .75f;
            float max = maxHeight * corta.verticalScale;
            roofHeight = Random.Range(min, max) / 2;

            // obtener los vertices desplazados para dentro
            house.DisplaceWalls(displace);
            Vector3[] elevatedVertices = house.vertices.ToArray();

            // Restaurar vertices casa
            house.vertices = originalVertices.ToList();

            Vector3[] vertices = new Vector3[(baseVertices.Length * 2) + 1];
            List<Vector3> verticesSuperiores = new List<Vector3>();

            for (int i = 0; i < baseVertices.Length; i++)
            {
                vertices[2 * i] = baseVertices[i];
                vertices[2 * i].y = corta.floors * max;

                vertices[2 * i + 1] = elevatedVertices[i];
                vertices[2 * i + 1].y = corta.floors * max + roofHeight;
                verticesSuperiores.Add(vertices[2 * i + 1]);
            }

            vertices[vertices.Length - 1] = house.center; // centro tejado
            vertices[vertices.Length - 1].y = corta.floors * max + roofHeight;

            // ------- RENDERIZADO POCHO ----------------

            Mesh m = new Mesh();
            m.vertices = verticesSuperiores.ToArray();


            // Triangular base superior

            Vector2[] points = new Vector2[verticesSuperiores.Count()];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2(verticesSuperiores[i].x, verticesSuperiores[i].z);
            }

            Utility.Triangulator tr = new Utility.Triangulator(points);

            int[] indices = tr.Triangulate();

            m.triangles = indices;

            m.uv = BuildUVs(m.vertices);
            m.RecalculateNormals();
            m.RecalculateBounds();

            GameObject go = new GameObject("RoofBevel_" + house.center);
            go.transform.parent = this.gameObject.transform;


            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = m;
            Renderer rend = go.AddComponent<MeshRenderer>();
            rend.material = tilesMaterials[0];

            if (_createAssetsInFolder){
#if UNITY_EDITOR
                SceneDataUtil.CreateAsset(mf.sharedMesh, "Meshes",  go.name + "_mesh.asset");
#endif
            }


            // -------------- RENDERIZADO CHIDO -------------------


            Vector3 v0, v1, v2, v3;

            GameObject roof;
            GameObject viga;

            for (int i = 0; i < baseVertices.Length - 1; i++)
            {
                v0 = vertices[2 * i] + Vector3.up * 0.2f;
                v1 = vertices[2 * i + 1];
                v2 = vertices[2 * i + 2] + Vector3.up * 0.2f;
                v3 = vertices[2 * i + 3];

                roof = BuildRoofTiles(v0, v1, v2, v3, tiles, tilesMaterials, tileSize);
                roof.transform.parent = this.gameObject.transform;

                Util.Optimization.MeshCombiner.CombineWithDiferentsMaterials(roof);

                // construir vigas

                viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
                viga.name = "VigaBevel" + i + "-1";
                viga.transform.parent = this.gameObject.transform;
                viga.transform.localScale = new Vector3(.6f, .1f, Vector3.Distance(v0, v1) + .6f);
                viga.transform.position = (v0 + v1) / 2;
                viga.transform.position -= (v1 - v0).normalized * .35f;
                viga.transform.position += Vector3.up * .1f;
                viga.transform.rotation = Quaternion.LookRotation(v1 - v0, Vector3.up);
                viga.GetComponent<Renderer>().material = tilesMaterials[0];

                if (Application.isEditor)
                    Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
                else
                    Object.Destroy(viga.GetComponent<BoxCollider>());

                v0 = originalVertices[i];
                v0.y = corta.floors * max + .1f;
                v2 = originalVertices[i + 1];
                v2.y = corta.floors * max + .1f;

                viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
                viga.name = "VigaBevel" + i + "-2";
                viga.transform.parent = this.gameObject.transform;
                viga.transform.localScale = new Vector3(.4f, .5f, Vector3.Distance(v0, v2) + .4f);
                viga.transform.position = (v0 + v2) / 2;
                viga.transform.rotation = Quaternion.LookRotation(v2 - v0, Vector3.up);
                viga.GetComponent<Renderer>().material = vigaMat;
                if (Application.isEditor)
                    Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
                else
                    Object.Destroy(viga.GetComponent<BoxCollider>());

                viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
                viga.name = "VigaBevel" + i + "-3";
                viga.transform.parent = this.gameObject.transform;
                viga.transform.localScale = new Vector3(.2f, .2f, Vector3.Distance(v1, v3) + .3f);
                viga.transform.position = (v1 + v3) / 2;
                viga.transform.rotation = Quaternion.LookRotation(v3 - v1, Vector3.up);
                viga.GetComponent<Renderer>().material = tilesMaterials[0];
                if (Application.isEditor)
                    Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
                else
                    Object.Destroy(viga.GetComponent<BoxCollider>());
            }

            // dibujar ultima pared
            v0 = vertices[2 * (baseVertices.Length - 1)] + Vector3.up * 0.2f;
            v1 = vertices[2 * (baseVertices.Length - 1) + 1];
            v2 = vertices[0] + Vector3.up * .2f;
            v3 = vertices[1];
            roof = BuildRoofTiles(v0, v1, v2, v3, tiles, tilesMaterials, tileSize);
            roof.transform.parent = this.gameObject.transform;

            Util.Optimization.MeshCombiner.CombineWithDiferentsMaterials(roof);

            // construir vigas

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaBevelUltima-1";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.6f, .1f, Vector3.Distance(v0, v1) + .6f);
            viga.transform.position = (v0 + v1) / 2;
            viga.transform.position -= (v1 - v0).normalized * .35f;
            viga.transform.position += Vector3.up * .1f;
            viga.transform.rotation = Quaternion.LookRotation(v1 - v0, Vector3.up);
            viga.GetComponent<Renderer>().material = tilesMaterials[0];
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            v0 = originalVertices[originalVertices.Length - 1];
            v0.y = corta.floors * max + .1f;
            v2 = originalVertices[0];
            v2.y = corta.floors * max + .1f;

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaBevelUltima-2";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.4f, .5f, Vector3.Distance(v0, v2) + .4f);
            viga.transform.position = (v0 + v2) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v2 - v0, Vector3.up);
            viga.GetComponent<Renderer>().material = vigaMat;
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

            viga = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viga.name = "VigaBevelUltima-3";
            viga.transform.parent = this.gameObject.transform;
            viga.transform.localScale = new Vector3(.2f, .2f, Vector3.Distance(v1, v3) + .3f);
            viga.transform.position = (v1 + v3) / 2;
            viga.transform.rotation = Quaternion.LookRotation(v3 - v1, Vector3.up);
            viga.GetComponent<Renderer>().material = tilesMaterials[0];
            if (Application.isEditor)
                Object.DestroyImmediate(viga.GetComponent<BoxCollider>());
            else
                Object.Destroy(viga.GetComponent<BoxCollider>());

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