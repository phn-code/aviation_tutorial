using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

public class MaterialClip : PlayableAsset
{

    // MaterialClip is used in timeline and can edit the materials.
    public Material currentMaterial;
    public Texture targetTexture;
    //public Material targetMaterial;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {

        // update this to material behaviour:
        var playable = ScriptPlayable<MaterialBehaviour>.Create(graph);
        MaterialBehaviour materialBehaviour = playable.GetBehaviour();


        // Assign MaterialBehaviour variables.
        materialBehaviour.currentMaterial = currentMaterial;
        materialBehaviour.targetTexture = targetTexture;
        //materialBehaviour.targetMaterial = targetMaterial;

        return playable; // return vars to playable.

    }
}