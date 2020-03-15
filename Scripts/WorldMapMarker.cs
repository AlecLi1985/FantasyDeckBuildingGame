using System;
using UnityEngine;


public class WorldMapMarker : MonoBehaviour
{
    public static event Action<MapPoint, WorldMapMarker> OnWorldMapMarkerPressed;
    public static event Action<MapPoint> OnWorldMapMarkerOver;
    public static event Action OnWorldMapMarkerExit;

    public MapPoint mapPoint;
    [TextArea]
    public string mapPointDescription;

    public Texture battleTexture;
    public Color battleColor;

    public Texture eliteBattleTexture;
    public Color eliteBattleColor;
    public Color eliteBattleEmissionColor;
    public float eliteBattleEmissionIntensity = 1f;

    public Texture shopTexture;
    public Color shopColor;

    public Texture randomTexture;
    public Color randomColor;

    public Texture saveTexture;
    public Color saveColor;

    public Texture miniBossTexture;
    public Color miniBossColor;

    public Texture treasureTexture;
    public Color treasureColor;
    public Color treasureEmissionColor;
    public float treasureEmissionIntensity = 1f;

    public Texture startTexture;
    public Color startColor;

    public Texture endTexture;
    public Color endColor;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {

    }

    private void OnMouseOver()
    {
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("WorldMapMarker");
       if(OnWorldMapMarkerOver != null)
        {
            OnWorldMapMarkerOver.Invoke(mapPoint);
        }
    }

    private void OnMouseDown()
    {
        if(OnWorldMapMarkerPressed != null)
        {
            OnWorldMapMarkerPressed.Invoke(mapPoint, this);
        }
    }

    private void OnMouseUp()
    {

    }

    private void OnMouseUpAsButton()
    {

    }

    private void OnMouseExit()
    {
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Default");

        if (OnWorldMapMarkerExit != null)
        {
            OnWorldMapMarkerExit.Invoke();
        }
    }

    public void SetMapMarkerMaterial()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        switch (mapPoint.mapPointType)
        {
            case MapPointType.BATTLE:
                renderer.material.SetTexture("_BaseMap", battleTexture);
                renderer.material.SetColor("_BaseColor", battleColor);
                break;
            case MapPointType.ELITEBATTLE:
                renderer.material.SetTexture("_BaseMap", eliteBattleTexture);
                renderer.material.SetColor("_BaseColor", eliteBattleColor);
                renderer.material.SetTexture("_EmissionMap", eliteBattleTexture);
                renderer.material.SetColor("_EmissionColor", eliteBattleEmissionColor * eliteBattleEmissionIntensity);
                renderer.material.EnableKeyword("_EMISSION");

                break;
            case MapPointType.SHOP:
                renderer.material.SetTexture("_BaseMap", shopTexture);
                renderer.material.SetColor("_BaseColor", shopColor);
                break;
            case MapPointType.RANDOM:
                renderer.material.SetTexture("_BaseMap", randomTexture);
                renderer.material.SetColor("_BaseColor", randomColor);
                break;
            case MapPointType.SAVE:
                renderer.material.SetTexture("_BaseMap", saveTexture);
                renderer.material.SetColor("_BaseColor", saveColor);
                break;
            case MapPointType.MINIBOSS:
                renderer.material.SetTexture("_BaseMap", miniBossTexture);
                renderer.material.SetColor("_BaseColor", miniBossColor);
                break;
            case MapPointType.TREASURE:
                renderer.material.SetTexture("_BaseMap", treasureTexture);
                renderer.material.SetColor("_BaseColor", treasureColor);
                renderer.material.SetTexture("_EmissionMap", treasureTexture);
                renderer.material.SetColor("_EmissionColor", treasureEmissionColor * treasureEmissionIntensity);
                renderer.material.EnableKeyword("_EMISSION");
                break;
            case MapPointType.START:
                renderer.material.SetTexture("_BaseMap", startTexture);
                renderer.material.SetColor("_BaseColor", startColor);
                gameObject.tag = "StartPoint";
                break;
            case MapPointType.END:
                renderer.material.SetTexture("_BaseMap", endTexture);
                renderer.material.SetColor("_BaseColor", endColor);
                gameObject.tag = "EndPoint";
                break;
        }
    }

}
