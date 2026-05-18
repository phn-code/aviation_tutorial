using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class SubtitleBehaviour : PlayableBehaviour
{

    public string subtitleText;
    public Color subtitleColor;

    // Update this later to make the text color serializble.

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        TextMeshProUGUI text = playerData as TextMeshProUGUI;
        text.text = subtitleText;
        //text.color = new Color(0, 0, 0, info.weight); // The subtitle text is instantiated as black.

    }

}
