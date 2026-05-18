using UnityEngine;
using UnityEngine.Playables;


public class SubtitleClip : PlayableAsset
{
    public Color subtitleColor;
    public string subtitleText;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SubtitleBehaviour>.Create(graph);

        SubtitleBehaviour subtitleBehaviour = playable.GetBehaviour();
        subtitleBehaviour.subtitleText = subtitleText;

        // GET THE PLAYER DATA SO I CAN GET THE TEXT.COLOR!
        //subtitleBehaviour.text.color =
        subtitleBehaviour.subtitleColor = subtitleColor;


        return playable;

    }
}
