using UnityEngine;
using UnityEditor;

public class AutoAssignTextures : EditorWindow
{
    [MenuItem("Tools/Auto Assign Textures")]
    static void AssignTextures()
    {
        int assigned = 0;
        int missed = 0;

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", 
            new[] { "Assets/Models/DA40" });
        
        foreach (string guid in materialGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            
            string texName = mat.name.Replace("DefaultWhite_", "")
                         .Replace("DefaultWhite.", "")
                         .Replace("DefaultWhite", "")
                         .Replace(".png", "")
                         .Replace(".PNG", "");

            string[] texGuids = AssetDatabase.FindAssets(texName + " t:Texture", 
                new[] { "Assets/Models/DA40/Textures" });
            
            if (texGuids.Length > 0)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(texGuids[0]);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                mat.SetTexture("_BaseMap", tex);
                mat.SetTexture("_MainTex", tex);
                EditorUtility.SetDirty(mat);
                Debug.Log($"✅ Assigned: {mat.name} → {tex.name}");
                assigned++;
            }
            else
            {
                Debug.LogWarning($"❌ No texture found for: {mat.name} (searched: {texName})");
                missed++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Done! Assigned: {assigned}, Missed: {missed}");
    }
}