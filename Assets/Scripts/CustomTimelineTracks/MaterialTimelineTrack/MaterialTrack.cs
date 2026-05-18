using TMPro;
using UnityEngine;
using UnityEngine.Timeline;




[TrackBindingType(typeof(SkinnedMeshRenderer))] // Put the SkinnedMeshRenderer of the plane in here! // can change the type to just MeshRenderer if you want.
[TrackClipType(typeof(MaterialClip))] // use materialClip script.

// this is the track to change material of an object.
// use trackasset type.
public class MaterialTrack : TrackAsset
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}