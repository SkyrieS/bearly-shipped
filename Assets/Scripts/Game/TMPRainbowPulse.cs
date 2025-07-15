using UnityEngine;
using TMPro;

public class TMPRainbowPulse : MonoBehaviour
{
    public float rainbowSpeed = 2f;
    public float rainbowSpread = 1f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 10f;
    public bool affectChildren = false;

    private TMP_Text tmpText;
    private float baseFontSize;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        if (tmpText == null && affectChildren)
        {
            tmpText = GetComponentInChildren<TMP_Text>();
        }
        if (tmpText != null)
        {
            baseFontSize = tmpText.fontSize;
        }
    }

    void Update()
    {
        if (tmpText == null) return;
        tmpText.ForceMeshUpdate();
        var textInfo = tmpText.textInfo;
        float time = Time.time * rainbowSpeed;
        float hue = Mathf.Repeat(time + rainbowSpread, 1f);
        Color32 color = Color.HSVToRGB(hue, 1f, 1f);
        tmpText.fontSize = baseFontSize + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        tmpText.color = color;
    }
}
