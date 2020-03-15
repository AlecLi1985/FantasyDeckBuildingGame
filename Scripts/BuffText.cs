using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class BuffText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<Transform> OnEnterBuffText;
    public static event Action OnExitBuffText;

    public BuffInfo buffInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(OnEnterBuffText != null)
        {
            OnEnterBuffText.Invoke(transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnExitBuffText != null)
        {
            OnExitBuffText.Invoke();
        }
    }
}
