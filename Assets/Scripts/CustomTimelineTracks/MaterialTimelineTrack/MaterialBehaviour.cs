using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class MaterialBehaviour : PlayableBehaviour
{

    public Material currentMaterial;
    public Texture targetTexture; // new texture.
    //public Material targetMaterial;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {

        Material mat = playerData as Material;
        //Material tMat = playerData as Material;
        Texture tex = playerData as Texture;

        // Update the material in the timeline.
        mat = currentMaterial;
        tex = targetTexture;

        //Debug.Log("tMat" + tMat.name);

        // Reassign the texture of a material! This will alter the material itself!
        mat.mainTexture = tex;

        //// Updates the material itself. COLOR (not material)
        //mat.color = tMat.color;

    }
}