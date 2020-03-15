using UnityEngine;
using System.Collections;
using TMPro;

public class MapGUIManager : MonoBehaviour
{
    public GameObject ventureButton;
    public GameObject battleButton;
    public GameObject backButton;
    public GameObject saveButton;
    public GameObject mapPointInfoPanel;
    public GameObject mapInfoEnemyIconsPanel;
    public GameObject saveNotificationPanel;
    public CanvasGroup saveNotificationCanvasGroup;
    public float animateSaveNotificationFadeTime;
    public GameObject loadGameErrorNotification;
    public CanvasGroup loadGameErrorNotificationCanvasGroup;
    public float animateLoadGameErrorTime;

    public TMP_Text mapPointInfoTitleText;
    public TMP_Text mapPointInfoLocationText;
    public TMP_Text mapPointInfoDescriptionText;

    // Start is called before the first frame update
    void Start()
    {
        if (ventureButton != null)
        {
            HideVentureButton();
        }
        if (backButton != null)
        {
            HideBackButton();
        }
        if (battleButton != null)
        {
            HideBattleButton();
        }
        if (mapPointInfoPanel != null)
        {
            HideMapPointInfoPanel();
        }

        GameManager.OnLoadGameError -= AnimateLoadGameErrorNotification;
        GameManager.OnLoadGameError += AnimateLoadGameErrorNotification;

        WorldMapMarker.OnWorldMapMarkerOver -= SetAndShowMapPointInfoPanel;
        WorldMapMarker.OnWorldMapMarkerOver += SetAndShowMapPointInfoPanel;
        WorldMapMarker.OnWorldMapMarkerExit -= HideMapPointInfoPanel;
        WorldMapMarker.OnWorldMapMarkerExit += HideMapPointInfoPanel;

        MapManager.CanMoveToMapMarkerEvent -= UpdateMapGUI;
        MapManager.CanMoveToMapMarkerEvent += UpdateMapGUI;
        MapManager.AnimatePlayerToMapMarkerComplete -= UpdateMapGUI;
        MapManager.AnimatePlayerToMapMarkerComplete += UpdateMapGUI;

        MapManager.OnSaveGame -= AnimateSaveGameNotification;
        MapManager.OnSaveGame += AnimateSaveGameNotification;

    }

    void UpdateMapGUI(MapPoint mapPoint, bool canMoveToMapMarker, bool isCurrentMapMarker = false)
    {
        SetMapPointInfoTitleText(mapPoint.mapPointType);

        if (mapPoint.mapPointType == MapPointType.BATTLE ||
            mapPoint.mapPointType == MapPointType.ELITEBATTLE ||
            mapPoint.mapPointType == MapPointType.MINIBOSS)
        {
            ShowMapPointInfoPanel();

            if (canMoveToMapMarker)
            {
                ShowBattleButton();
            }
            else
            {
                HideBattleButton();
            }

            ShowBackButton();
            HideVentureButton();
            HideSaveButton();
        }
        else if(mapPoint.mapPointType == MapPointType.RANDOM)
        {
            ShowMapPointInfoPanel();

            if (canMoveToMapMarker)
            {
                ShowVentureButton();
            }
            else
            {
                HideVentureButton();
            }

            ShowBackButton();
            HideBattleButton();
            HideSaveButton();

        }
        else if (mapPoint.mapPointType == MapPointType.SHOP)
        {
            ShowMapPointInfoPanel();

            if (canMoveToMapMarker)
            {
                ShowVentureButton();
            }
            else
            {
                HideVentureButton();
            }

            ShowBackButton();
            HideBattleButton();
            HideSaveButton();

        }
        else if (mapPoint.mapPointType == MapPointType.SAVE)
        {
            ShowMapPointInfoPanel();

            if (canMoveToMapMarker)
            {
                ShowVentureButton();
            }
            else
            {
                HideVentureButton();
            }

            if(isCurrentMapMarker)
            {
                ShowSaveButton();
            }
            else
            {
                HideSaveButton();
            }

            ShowBackButton();
            HideBattleButton();
        }
        else if (mapPoint.mapPointType == MapPointType.TREASURE)
        {
            ShowMapPointInfoPanel();

            if (canMoveToMapMarker)
            {
                ShowVentureButton();
            }
            else
            {
                HideVentureButton();
            }

            ShowBackButton();
            HideBattleButton();
            HideSaveButton();
        }
        else if (mapPoint.mapPointType == MapPointType.NONE ||
            mapPoint.mapPointType == MapPointType.START ||
            mapPoint.mapPointType == MapPointType.END)
        {
            HideMapPointInfoPanel();

            HideVentureButton();
            HideBattleButton();
            ShowBackButton();
            HideSaveButton();
        }
    }

    void ResetMapGUI()
    {
        HideBattleButton();
        HideBackButton();
    }

    public void HideVentureButton()
    {
        ventureButton.SetActive(false);
    }

    public void ShowVentureButton()
    {
        ventureButton.SetActive(true);
    }

    public void HideBattleButton()
    {
        battleButton.SetActive(false);
    }

    public void ShowBattleButton()
    {
        battleButton.SetActive(true);
    }

    public void HideBackButton()
    {
        backButton.SetActive(false);
    }

    public void ShowBackButton()
    {
        backButton.SetActive(true);
    }

    public void HideSaveButton()
    {
        saveButton.SetActive(false);
    }

    public void ShowSaveButton()
    {
        saveButton.SetActive(true);
    }

    public void HideMapPointInfoPanel()
    {
        mapPointInfoPanel.SetActive(false);
    }

    public void ShowMapPointInfoPanel()
    {
        mapPointInfoPanel.SetActive(true);
    }

    public void SetAndShowMapPointInfoPanel(MapPoint mapPoint)
    {
        SetMapPointInfoTitleText(mapPoint.mapPointType);
        SetMapPointInfoLocationText(mapPoint.title);
        SetMapPointInfoDescriptionText(mapPoint.description);

        for (int i = 0; i < mapInfoEnemyIconsPanel.transform.childCount; i++)
        {
            if (i < mapPoint.enemySpawnCount)
            {
                mapInfoEnemyIconsPanel.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                mapInfoEnemyIconsPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        ShowMapPointInfoPanel();
    }

    public void SetMapPointInfoTitleText(MapPointType mapPointType)
    {

        if (mapPointType == MapPointType.BATTLE)
        {
            mapPointInfoTitleText.text = "Battle";
        }
        if (mapPointType == MapPointType.ELITEBATTLE)
        {
            mapPointInfoTitleText.text = "Battle Elite";
        }
        else if (mapPointType == MapPointType.RANDOM)
        {
            mapPointInfoTitleText.text = "Random";
        }
        else if (mapPointType == MapPointType.SAVE)
        {
            mapPointInfoTitleText.text = "Save";
        }
        else if (mapPointType == MapPointType.SHOP)
        {
            mapPointInfoTitleText.text = "Shop";
        }
        else if (mapPointType == MapPointType.MINIBOSS)
        {
            mapPointInfoTitleText.text = "Mini Boss";
        }
        else if (mapPointType == MapPointType.TREASURE)
        {
            mapPointInfoTitleText.text = "Treasure";
        }
        else if (mapPointType == MapPointType.END)
        {
            mapPointInfoTitleText.text = "Finale";
        }
        else if (mapPointType == MapPointType.NONE ||
            mapPointType == MapPointType.START ||
            mapPointType == MapPointType.END)
        {
            mapPointInfoTitleText.text = "";
        }

    }

    public void SetMapPointInfoLocationText(string infoText)
    {
        mapPointInfoLocationText.text = infoText;
    }

    public void SetMapPointInfoDescriptionText(string infoText)
    {
        mapPointInfoDescriptionText.text = infoText;
    }

    void AnimateSaveGameNotification()
    {
        saveNotificationPanel.SetActive(true);
        saveNotificationCanvasGroup.alpha = 0f;

        StopAllCoroutines();
        StartCoroutine(AnimateFadeInSaveNotificationRoutine(.25f));
    }

    IEnumerator AnimateFadeInSaveNotificationRoutine(float fadeOutDelay)
    {
        float currentTime;
        for (currentTime = 0f; currentTime < animateSaveNotificationFadeTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateSaveNotificationFadeTime) * currentTime;
            saveNotificationCanvasGroup.alpha = Mathf.Lerp(saveNotificationCanvasGroup.alpha, 1f, normalizedTime);

            yield return null;
        }

        saveNotificationCanvasGroup.alpha = 1f;

        StartCoroutine(AnimateFadeOutSaveNotificationPanelRoutine(fadeOutDelay));
    }

    IEnumerator AnimateFadeOutSaveNotificationPanelRoutine(float fadeOutDelay)
    {
        yield return new WaitForSeconds(fadeOutDelay);

        float currentTime;
        for (currentTime = 0f; currentTime < animateSaveNotificationFadeTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateSaveNotificationFadeTime) * currentTime;
            saveNotificationCanvasGroup.alpha = Mathf.Lerp(saveNotificationCanvasGroup.alpha, 0f, normalizedTime);

            yield return null;
        }

        saveNotificationCanvasGroup.alpha = 0f;
        saveNotificationPanel.SetActive(false);
    }

    void AnimateLoadGameErrorNotification()
    {
        loadGameErrorNotification.SetActive(true);
        loadGameErrorNotificationCanvasGroup.alpha = 0f;

        StopAllCoroutines();
        StartCoroutine(AnimateFadeInLoadGameErrorNotificationRoutine(.25f));
    }

    IEnumerator AnimateFadeInLoadGameErrorNotificationRoutine(float fadeOutDelay)
    {
        float currentTime;
        for (currentTime = 0f; currentTime < animateLoadGameErrorTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateLoadGameErrorTime) * currentTime;
            loadGameErrorNotificationCanvasGroup.alpha = Mathf.Lerp(loadGameErrorNotificationCanvasGroup.alpha, 1f, normalizedTime);

            yield return null;
        }

        loadGameErrorNotificationCanvasGroup.alpha = 1f;

        StartCoroutine(AnimateFadeOutLoadGameErrorNotificationRoutine(fadeOutDelay));
    }

    IEnumerator AnimateFadeOutLoadGameErrorNotificationRoutine(float fadeOutDelay)
    {
        yield return new WaitForSeconds(fadeOutDelay);

        float currentTime;
        for (currentTime = 0f; currentTime < animateLoadGameErrorTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateLoadGameErrorTime) * currentTime;
            loadGameErrorNotificationCanvasGroup.alpha = Mathf.Lerp(loadGameErrorNotificationCanvasGroup.alpha, 0f, normalizedTime);

            yield return null;
        }

        loadGameErrorNotificationCanvasGroup.alpha = 0f;
        loadGameErrorNotification.SetActive(false);
    }

    private void OnDestroy()
    {
        WorldMapMarker.OnWorldMapMarkerOver -= SetAndShowMapPointInfoPanel;
        WorldMapMarker.OnWorldMapMarkerExit -= HideMapPointInfoPanel;

        MapManager.CanMoveToMapMarkerEvent -= UpdateMapGUI;
        MapManager.AnimatePlayerToMapMarkerComplete -= UpdateMapGUI;
        MapManager.OnSaveGame -= AnimateSaveGameNotification;

        GameManager.OnLoadGameError -= AnimateLoadGameErrorNotification;

    }
}
