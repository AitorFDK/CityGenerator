using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CityGen.Model;
using CityGen.MenuItem;

namespace CityGen.Utils
{

    public class FillSegment : MonoBehaviour
    {

        public Mesh[] meshesTest;

        public GameObject go;
        public GameObject start;
        public GameObject end;

        public Material matWall;

        // Start is called before the first frame update
        void Start()
        {
            //FillSegmentWithBlock(go, start.transform.position, end.transform.position);
            FillSegmentWithMeshes(start.transform.position, end.transform.position, meshesTest, matWall);
        }

        public static void FillSegmentWithBlock(GameObject gameObject, Vector3 vStart, Vector3 vEnd)
        {

            float distancia = Vector3.Distance(vEnd, vStart);

            Vector3 direction = (vEnd - vStart).normalized;

            Vector3 position = vStart;

            int i = 0;

            while (distancia > Vector3.Distance(position, vStart))
            {
                GameObject.Instantiate(gameObject, position, Quaternion.identity);
                // GameObject.Instantiate(gameObject, position, Quaternion.LookRotation(direction, Vector3.up));

                position += Vector3.Scale(direction, gameObject.GetComponentsInChildren<Renderer>().First().bounds.size);

                i++;
            }

        }

        public static GameObject BuildHouse(House house, float blockSize, Quaternion rotationOffset)
        {

            GameObject go = new GameObject("House(Center:" + house.center.ToString() + ")");
            go.transform.position = house.center;
            house.gameObject = go;

            float vscale = Random.Range(0.8f, 1.2f);
            // PolygonOrientation orientation = house.Orientation();

            for (int i = 1; i <= house.vertices.Count; i++)
            {
                // GameObject wall = CreateHouseWall(house.floors, gameObjects, house.vertices.)

                Wall wall = new Wall(house.vertices[i - 1], house.vertices[i == house.vertices.Count ? 0 : i], house.floors, vscale);

                BuildWall(wall, house.houseConfig, FillBlockSelection.random, blockSize, rotationOffset);
                wall.gameObject.transform.parent = house.gameObject.transform;

                GameObject col = new GameObject("WallCollision");
                col.transform.localPosition = wall.center + (Vector3.up * house.floors * blockSize * vscale) / 2f;                                
                col.transform.localRotation = Quaternion.LookRotation(wall.direction, Vector3.up);                

                BoxCollider collider = col.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(.5f, house.floors * blockSize * vscale, wall.distance);
                
                col.transform.parent = wall.gameObject.transform;
                
                // collider.transform.rotation = Quaternion.LookRotation(wall.direction, Vector3.up);

                house.walls.Add(wall);
            }

            return go;
        }

        public static GameObject BuildWall(Wall wall, HouseItem config, FillBlockSelection goSelection, float blockSize, Quaternion rotationOffset)
        {

            int nBlocks = (int)Mathf.Round(wall.distance / blockSize);
            float sizeBloques = nBlocks * blockSize;
            float scale = wall.distance / sizeBloques;

            GameObject wallGO = new GameObject("Walls(Start:" + wall.start.ToString() + ")");
            wall.gameObject = wallGO;
            wallGO.transform.position = wall.start;

            for (int i = 0; i < wall.floors; i++)
            {

                // Vector3 position = vStart + direction * blockSize * scale + Vector3.up * blockSize * scale * i;
                Vector3 position = wall.start + wall.direction * blockSize * scale + Vector3.up * blockSize * wall.verticalScale * i;

                WallItem item = null;

                for (int j = 0; j < nBlocks; j++)
                {

                    if (item == null)
                    {
                        bool continueIterating = true;
                        while (continueIterating)
                        {
                            item = config.wallItems[Random.Range(0, config.wallItems.Count)];

                            if (item.floorCap > 0 && i + 1 != item.floorCap) // si no compleix restriccio de pis, torna a tirar
                                continue;

                            continueIterating = false;
                        }
                    }
                    else
                        item = item.SelectNeighbour(i + 1).item;

                    GameObject go = GameObject.Instantiate(item.prefab, position, Quaternion.LookRotation(wall.direction, Vector3.up));
                    go.name = "Floor:" + i + "_Block:" + j;

                    Vector3 vScale = Vector3.one;
                    vScale.x *= scale;
                    vScale.y *= wall.verticalScale;

                    // go.transform.localScale *= scale;
                    go.transform.localScale = Vector3.Scale(go.transform.localScale, vScale);
                    go.transform.localRotation *= rotationOffset;
                    go.transform.parent = wallGO.transform;

                    // position += direction * blockSize * scale;
                    position += wall.direction * blockSize * scale;
                }
            }
            return wallGO;
        }

        // public static GameObject CreateHouseWall (int nFloors, List<GameObject> gameObjects, Vector3 vStart, Vector3 vEnd, FillBlockSelection goSelection, float blockSize, Quaternion rotationOffset) {

        //     float distancia = Vector3.Distance(vEnd, vStart);
        //     float angle = Vector3.Angle(vStart, vEnd);
        //     Vector3 direction = (vEnd - vStart).normalized;

        //     int nBlocks = (int)Mathf.Round(distancia / blockSize);
        //     float sizeBloques = nBlocks * blockSize;
        //     float scale = distancia / sizeBloques;

        //     GameObject wall = new GameObject("Walls_"+vStart.ToString());  

        //     wall.transform.position = vStart;

        //     for (int i = 0; i < nFloors; i++){

        //         // Vector3 position = vStart + direction * blockSize * scale + Vector3.up * blockSize * scale * i;
        //         Vector3 position = vStart + direction * blockSize * scale + Vector3.up * blockSize * i;

        //         for (int j = 0; j < nBlocks; j++) {

        //             GameObject gameObject = null;

        //             switch (goSelection){

        //                 case FillBlockSelection.sequencial:
        //                     gameObject = gameObjects[ i % gameObjects.Count ];
        //                     break;

        //                 case FillBlockSelection.random:
        //                     gameObject = gameObjects[ Random.Range(0,gameObjects.Count) ];
        //                     break;
        //             }


        //             GameObject go = GameObject.Instantiate(gameObject, position, Quaternion.LookRotation(direction, Vector3.up));
        //             go.name="Floor:"+i+"_Block:"+j;

        //             Vector3 vScale = Vector3.one;
        //             vScale.x *= scale;

        //             // go.transform.localScale *= scale;
        //             go.transform.localScale = Vector3.Scale(go.transform.localScale, vScale);
        //             go.transform.localRotation *= rotationOffset;
        //             go.transform.parent = wall.transform;

        //             // position += direction * blockSize * scale;
        //             position += direction * blockSize * scale;
        //         }            
        //     }
        //     return wall;
        // }

        public enum FillBlockSelection { sequencial, random };
        public static GameObject FillSegmentWithBlock(List<GameObject> gameObjects, Vector3 vStart, Vector3 vEnd, FillBlockSelection goSelection, float blockSize, Quaternion rotationOffset)
        {

            float distancia = Vector3.Distance(vEnd, vStart);

            float angle = Vector3.Angle(vStart, vEnd);

            Vector3 direction = (vEnd - vStart).normalized;


            int bloques = (int)Mathf.Round(distancia / blockSize);
            float sizeBloques = bloques * blockSize;
            float scale = distancia / sizeBloques;

            Vector3 position = vStart + direction * blockSize * scale;

            Debug.Log(distancia + " / " + blockSize + " = " + bloques + " ---- " + scale);

            GameObject res = new GameObject();
            res.transform.position = position;

            // while (distancia > Vector3.Distance(position, vStart)) {
            for (int i = 0; i < bloques; i++)
            {

                GameObject gameObject = null;

                switch (goSelection)
                {

                    case FillBlockSelection.sequencial:
                        gameObject = gameObjects[i % gameObjects.Count];
                        break;

                    case FillBlockSelection.random:
                        gameObject = gameObjects[Random.Range(0, gameObjects.Count)];
                        break;
                }


                GameObject go = GameObject.Instantiate(gameObject, position, Quaternion.identity); // Quaternion.LookRotation(direction, Vector3.up)
                go.transform.localScale *= scale;
                go.transform.localRotation *= rotationOffset;
                go.transform.parent = res.transform;

                // position += Vector3.Scale(direction, gameObject.GetComponent<Renderer>().bounds.size);
                position += direction * blockSize * scale;
                // i++;
            }
            return res;
        }


        public enum FillMeshSelection { sequencial };
        public static GameObject FillSegmentWithMeshes(Vector3 vStart, Vector3 vEnd, Mesh[] meshes, Material material, FillMeshSelection meshSelection = FillMeshSelection.sequencial)
        {

            float distancia = Vector3.Distance(vEnd, vStart);
            Vector3 direction = (vEnd - vStart).normalized;

            List<CombineInstance> combine = new List<CombineInstance>();

            float recorregut = 0;
            int i = 0;

            Quaternion q = Quaternion.LookRotation(direction, Vector3.up);

            while (distancia > recorregut)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = meshes[i];
                ci.transform = Matrix4x4.Translate(recorregut * direction) * Matrix4x4.Rotate(q);
                combine.Add(ci);

                recorregut += Vector3.Scale(Vector3.forward, ci.mesh.bounds.size).magnitude;

                switch (meshSelection)
                {
                    case FillMeshSelection.sequencial:
                        i++;
                        if (i >= meshes.Length) i = 0;
                        break;
                }
            }

            GameObject go = new GameObject();
            go.transform.position = vStart;
            go.transform.rotation = new Quaternion();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();
            mf.sharedMesh.CombineMeshes(combine.ToArray());
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            return go;
        }


    }

}