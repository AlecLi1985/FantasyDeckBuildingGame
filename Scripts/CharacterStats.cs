using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStats : MonoBehaviour
{
    public TMP_Text characterNameText;
    public TMP_Text characterHealthText;
    public Slider characterHealthSlider;
    public TMP_Text characterBlockText;
    public GameObject buffTextParent;
    public GameObject buffTextObject;

    public Transform characterObjectTransform;
    public Vector3 characterStatsPositionOffset = Vector3.zero;

    public RectTransform characterStatsRectTransform;
    public bool isPermenantlyDisabled = false;
}
