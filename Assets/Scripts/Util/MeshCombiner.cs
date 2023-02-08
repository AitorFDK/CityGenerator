using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Util.Optimization
{
    public static class MeshCombiner
    {


        private static bool _createAssetsInFolder = false;

        // Indicar si es volen guardar les meshes a la carpeta de data de la escena
        public static void SetCreateAssetsInData(bool val){
            _createAssetsInFolder = val;
        }


        /// <summary>
        /// Gets all the meshes of the children of current parent. The children must have the same material and
        /// the combined mesh can't exceed 65535 vertex.
        /// </summary>
        /// <param name="parentToCombine"> Transform that will be the recipient of the new mesh</param>
        public static void Combine(Transform parentToCombine)
        {
            //Aixo ha de anar aqui si o si perque la idea es que el pare ha de tenir una mesh com a component
            MeshFilter meshF = parentToCombine.GetComponent<MeshFilter>();
            if (meshF == null)
            {
                meshF = parentToCombine.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            }

            List<MeshFilter> ChildrenRemovePArent = new List<MeshFilter>(parentToCombine.GetComponentsInChildren<MeshFilter>());
            ChildrenRemovePArent.RemoveAt(0);
            MeshFilter[] meshFiltersChildren = ChildrenRemovePArent.ToArray();
            CombineInstance[] combineMesh = new CombineInstance[meshFiltersChildren.Length];


            if (parentToCombine.GetComponent<MeshRenderer>() == null)
            {
                MeshRenderer mr = parentToCombine.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                mr.sharedMaterial = meshFiltersChildren[0].transform.GetComponent<MeshRenderer>().sharedMaterial;
            }            

            for (int i = 0; i < meshFiltersChildren.Length; i++)
            {
                combineMesh[i].mesh = meshFiltersChildren[i].sharedMesh;
                combineMesh[i].transform = meshFiltersChildren[i].transform.localToWorldMatrix;
                //GameObject.Destroy(meshFiltersChildren[i].gameObject, 1f);
                meshFiltersChildren[i].gameObject.SetActive(false);
                if (Application.isEditor)
                    GameObject.DestroyImmediate(meshFiltersChildren[i].gameObject);
                else
                    GameObject.Destroy(meshFiltersChildren[i].gameObject);
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combineMesh);
            meshF.sharedMesh = mesh;

            // meshF.sharedMesh = new Mesh();
            // meshF.sharedMesh.CombineMeshes(combineMesh);

            // Guardem el mesh en una carpeta
            if (_createAssetsInFolder){
                #if UNITY_EDITOR
                SceneDataUtil.CreateAsset(meshF.sharedMesh, "Meshes",  parentToCombine.parent.name+ "_"+ parentToCombine.name + "_mesh.asset");
                #endif
            }


            //Realment aixo es un assert
            parentToCombine.gameObject.SetActive(true);

            //Aquesta part es necessaria per a que no es descoloquin les coses
            parentToCombine.localScale = Vector3.one;
            parentToCombine.rotation = Quaternion.identity;
            parentToCombine.position = Vector3.zero;
            
        }

        /// <summary>
        /// Gets all the meshes of the children of current parent. The children must have the same material and
        /// the combined mesh can't exceed 65535 vertex. 
        /// </summary>
        /// <param name="parentToCombine"> GameObject that will be the recipient of the new mesh</param>
        public static void Combine(GameObject parentToCombine)
        {
            Combine(parentToCombine.transform);
        }

        /// <summary>
        /// Gets all the diferents materials of the children and sorts them in diferent children.
        /// Each one of the children will recive a call of the MeshCombiner.Combine(Transform) function.
        /// All the elements can only have one material per each. And all the diferents materials must
        /// have a diferent name.
        /// </summary>
        /// <param name="parentToCombine"> Trasform tha will have the children that will combine the diferent materials</param>
        public static void CombineWithDiferentsMaterials(Transform parentToCombine)
        {
            //Remove components del pare
            if (parentToCombine.GetComponent<MeshRenderer>() != null)
            {
                if (Application.isEditor)
                    Object.DestroyImmediate(parentToCombine.GetComponent<MeshRenderer>());
                else
                    Object.Destroy(parentToCombine.GetComponent<MeshRenderer>());
            }
            if (parentToCombine.GetComponent<MeshFilter>() != null)
            {
                if (Application.isEditor)
                    Object.DestroyImmediate(parentToCombine.GetComponent<MeshFilter>());
                else
                    Object.Destroy(parentToCombine.GetComponent<MeshFilter>());
            }

            Dictionary<string, List<GameObject>> RepartimentDeFills = new Dictionary<string, List<GameObject>>();
            MeshRenderer[] ogChildren = parentToCombine.GetComponentsInChildren<MeshRenderer>();

            //Repartir els fills per material
            for (int i = 0; i < ogChildren.Length; i++)
            {
                if (RepartimentDeFills.ContainsKey(ogChildren[i].sharedMaterial.name))
                {
                    RepartimentDeFills[ogChildren[i].sharedMaterial.name].Add(ogChildren[i].gameObject);
                }
                else
                {
                    RepartimentDeFills.Add(ogChildren[i].sharedMaterial.name, new List<GameObject>());
                    RepartimentDeFills[ogChildren[i].sharedMaterial.name].Add(ogChildren[i].gameObject);
                }
            }

            foreach (KeyValuePair<string, List<GameObject>> lg in RepartimentDeFills)
            {
                GameObject nouFill = new GameObject(lg.Key);

                foreach (GameObject element in lg.Value)
                {
                    element.transform.parent = nouFill.transform;
                }

                nouFill.transform.parent = parentToCombine;

                Combine(nouFill);
            }

        }

        /// <summary>
        /// Gets all the diferents materials of the children and sorts them in diferent children.
        /// Each one of the children will recive a call of the MeshCombiner.Combine(GameObject) function.
        /// All the elements can only have one material per each.And all the diferents materials must
        /// have a diferent name.
        /// </summary>
        /// <param name="parentToCombine"> GameObject tha will have the children that will combine the diferent materials</param>
        public static void CombineWithDiferentsMaterials(GameObject parentToCombine)
        {
            CombineWithDiferentsMaterials(parentToCombine.transform);
        }

        /// <summary>
        /// This function causes the geometry and vertices of the mesh to be reordered internally 
        /// in an attempt to improve vertex cache utilisation on the graphics hardware and thus rendering performance. 
        /// This operation can take a few seconds or more for complex meshes and should only be used where the ordering of 
        /// the geometry and vertices is not significant as both will change. You should only use this function on meshes you generate procedurally in code,
        /// for regular mesh assets it is called automatically by the import pipeline when 'Optimize Mesh' is enabled in the mesh importer settings.
        /// </summary>
        /// <param name="meshToOptimaze"> The Transform that contains the mesh that will be optimazed</param>
        public static void OptimazeMesh(Transform meshToOptimaze)
        {
            MeshFilter meshToGet = meshToOptimaze.GetComponent<MeshFilter>();

            if (meshToGet == null)
            {
                Debug.LogError("There is no MeshFilter component");
                return;
            }

            Mesh mesh = meshToGet.mesh;

            if (mesh == null)
            {
                Debug.LogError("There is no mesh or is null in the mesh renderer");
                return;
            }

            mesh.Optimize();
        }

        /// <summary>
        /// This function causes the geometry and vertices of the mesh to be reordered internally 
        /// in an attempt to improve vertex cache utilisation on the graphics hardware and thus rendering performance. 
        /// This operation can take a few seconds or more for complex meshes and should only be used where the ordering of 
        /// the geometry and vertices is not significant as both will change. You should only use this function on meshes you generate procedurally in code,
        /// for regular mesh assets it is called automatically by the import pipeline when 'Optimize Mesh' is enabled in the mesh importer settings.
        /// </summary>
        /// <param name="meshToOptimaze"> The Transform that contains the mesh that will be optimazed</param>
        public static void OptimazeMesh(GameObject meshToOptimaze)
        {
            OptimazeMesh(meshToOptimaze.transform);
        }

    }
}