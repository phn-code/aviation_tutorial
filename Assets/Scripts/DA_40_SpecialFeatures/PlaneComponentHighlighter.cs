using System.Collections.Generic;
using UnityEngine;

/** 
Handles highlighting of specific components of the aircraft. Works by duplicating, modifying, and then assigning a new emissive material to the selected GameObjects. Typically called from DA_40.cs.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public static class PlaneHighlighter
{
    /**
    Highlights a collection of GameObjects a determined colour using emission.
    @param components The collection of GameObjects to be highlighted.
    @param glowColour The the colour that the selected GameObjects should glow.
    @return void
    */
    public static void Highlight(IEnumerable<GameObject> components, Color glowColour)
    {
        foreach (var obj in components) // Loop through each plane component passed in (parent or standalone) 
        {
            Debug.Log($"Highlighting: {obj.name}");
            if (obj == null) continue; // Skip plane components that are null (shouldn't happen, but a good failsafe)

            var renderers = obj.GetComponentsInChildren<Renderer>(true); // Get the render component of each child object AND the parent object
            foreach (var renderer in renderers) // Loop through each renderer
            {
                var mat = renderer.material; // Create a new material instance for JUST the selected component so that the original material and other components that may share that material aren't affected

                if (mat.HasProperty("_Surface")) // Checking if the material is transparent or opaque, if so we want to skip it
                {
                    int surface = (int)mat.GetFloat("_Surface");
                    if (surface == 1) continue; // Skip transparent material
                }

                mat.EnableKeyword("_EMISSION"); // Toggle emission for the material if not already enabled
                mat.SetColor("_EmissionColor", glowColour * 100f); // Set the emission colour to the passed in colour, and intensify it
            }
        }
    }

    /**
    Uh-highlights a collection of GameObjects by disabling their emission and resetting their materials' properties.
    @param components The collection of GameObjects to be reverted to a non-emissive state.
    @return void
    */
    public static void Unhighlight(IEnumerable<GameObject> components)
    {
        foreach (var obj in components) // Loop through each plane component passed in (parent or standalone)
        {
            Debug.Log($"Un-highlighting: {obj.name}");
            if (obj == null) continue; // Skip plane components that are null (shouldn't happen, but a good failsafe)

            var renderers = obj.GetComponentsInChildren<Renderer>(true); // Get the render component of each child object AND the parent object
            foreach (var renderer in renderers) // Loop through each renderer and turn off their emission and reset their properties
            {
                var mat = renderer.material;
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}