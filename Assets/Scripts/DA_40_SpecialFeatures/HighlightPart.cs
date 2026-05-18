using System.Collections;
using UnityEngine;

public class HighlightPart : MonoBehaviour
{
    public Color highlightColor = Color.red;
    public float pulseDuration = 0.3f;
    public int pulseCount = 3;

    private Renderer[] _renderers;
    private Color[] _originalColors;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    public void Activate()
    {
        StopAllCoroutines();
        StartCoroutine(HighlightRoutine());
    }

    IEnumerator HighlightRoutine()
    {
        for (int p = 0; p < pulseCount; p++)
        {
            foreach (var r in _renderers) r.material.color = highlightColor;
            yield return new WaitForSeconds(pulseDuration);

            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].material.color = _originalColors[i];
            yield return new WaitForSeconds(pulseDuration);
        }
    }
}