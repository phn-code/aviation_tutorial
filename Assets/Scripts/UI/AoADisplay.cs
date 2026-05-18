using UnityEngine;
using TMPro;

/** 
A dynamic Angle of Attack meter that is displayed on the UI, changing colour dynamically based on how close to critical angle it is.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class AoADisplay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI AoAMeter; /**< The Angle of Attack meter GameObject. */
    public GameObject aircraft; /**< Reference to the DA-40 aircraft GameObject. */

    void Start()
    {

    }

    void Update()
    {
        float aoa = aircraft.transform.eulerAngles.z; // Get the Angle of Attack

        float t = Mathf.InverseLerp(0f, 15f, aoa);

        Color textColour = Color.Lerp(Color.green, Color.red, t);

        AoAMeter.color = textColour;
        AoAMeter.text = aoa.ToString("F0") + "�";
    }
}