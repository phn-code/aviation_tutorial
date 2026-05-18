using UnityEngine;
using UnityEditor;
using System.IO;

public class GhostMeshGenerator
{
    [MenuItem("Tools/Generate mesh from selected meshes")]
    static void GenerateShostMesh()
    {
        GameObject target = Selection.activeGameObject;
        MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh ghostMesh = new Mesh();
        ghostMesh.name = target.name + "_GhostMesh";
        ghostMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        ghostMesh.CombineMeshes(combine, true, true);

        string path = "Assets/GhostMeshes";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string assetPath = path + "/" + ghostMesh.name + ".asset";
        AssetDatabase.CreateAsset(ghostMesh, assetPath);
        AssetDatabase.SaveAssets();
    }
}
