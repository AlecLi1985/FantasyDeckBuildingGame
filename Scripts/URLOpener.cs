using UnityEngine;
using System.Runtime.InteropServices;

public class URLOpener : MonoBehaviour
{
    public string URL;

    [DllImport("__Internal")]
    private static extern void OpenURL(string url);

    public void OpenInNewTab()
    {
        //Application.OpenURL(URL);
        OpenURL(URL);
    }
}
