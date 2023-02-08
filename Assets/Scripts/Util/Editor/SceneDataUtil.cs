using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneDataUtil
{

    public static void CreateAsset(Object asset, string name)
    {
        CreateAsset(asset, "", name);
    }

    public static void CreateAsset(Object asset, string subpath, string name)
    {
        string path = GetCurrentDataPath();
        if (subpath != "") path += "/" + subpath;

        path = path.Trim().Replace("//", "/");

        CreateFolderIfNotExist(path);

        string fullPath = path + "/" + name;

        fullPath = fullPath.Trim().Replace("//", "/");

        AssetDatabase.CreateAsset(asset, fullPath);
    }

    public static void SaveAssets()
    {
        AssetDatabase.SaveAssets();
    }

    public static void ClearData()
    {
        ClearData("");
    }

    public static void ClearData(string subpath)
    {
        string path = GetCurrentDataPath();
        if (subpath != "") path += "/" + subpath;

        path = path.Trim().Replace("//", "/");

        if (AssetDatabase.IsValidFolder(path))
        {
            string[] toDelete = { path };
            foreach (var asset in AssetDatabase.FindAssets("", toDelete))
            {
                var fullPath = AssetDatabase.GUIDToAssetPath(asset);
                AssetDatabase.DeleteAsset(fullPath);
            }
        }
    }

    private static string GetCurrentDataPath()
    {
        List<string> aux = SceneManager.GetActiveScene().path.Split('/').ToList();

        aux[aux.Count - 1] = "";

        return string.Join("/", aux) + SceneManager.GetActiveScene().name + "_Data";
    }

    private static void CreateFolderIfNotExist(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            List<string> aux = path.Split('/').ToList();

            string name = aux[aux.Count - 1];
            aux.RemoveAt(aux.Count - 1);

            AssetDatabase.CreateFolder(string.Join("/", aux), name);
            Debug.Log($"Carpeta {path} creada!");
        }
    }

}
