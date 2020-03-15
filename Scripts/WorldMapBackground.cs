using System;
using UnityEngine;

public class WorldMapBackground : MonoBehaviour
{
    public static event Action OnWorldMapBackgroundPressed;

    private void OnMouseDown()
    {
        //if (OnWorldMapBackgroundPressed != null)
        //{
        //    OnWorldMapBackgroundPressed.Invoke();
        //}
    }
}
