using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject loadGameErrorNotification;
    public CanvasGroup loadGameErrorNotificationCanvasGroup;
    public float animateLoadGameErrorTime;

    public GameManager gameManager;
    public LevelLoader levelLoader;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        levelLoader = FindObjectOfType<LevelLoader>();

        GameManager.OnLoadGameError -= AnimateLoadGameErrorNotification;
        GameManager.OnLoadGameError += AnimateLoadGameErrorNotification;

        GameManager.OnLoadGameSuccess -= LoadLevel;
        GameManager.OnLoadGameSuccess += LoadLevel;


    }

    public void LoadGame()
    {
        if (gameManager != null)
        {
            gameManager.LoadGame();
        }
    }

    void LoadLevel()
    {
        if (levelLoader != null)
        {
            levelLoader.SetSceneIndex(1);
            levelLoader.LoadNextLevel();
        }
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

    public void StartNewGame()
    {
        if(gameManager != null)
        {
            gameManager.SetPlayerDeck();
            gameManager.ResetMapPoints();
            gameManager.gameInProgress = false;
        }
    }

    public void QuitGame()
    {
        if(gameManager != null)
        {
            gameManager.Quit();
        }
    }

    private void OnDestroy()
    {
        GameManager.OnLoadGameError -= AnimateLoadGameErrorNotification;
        GameManager.OnLoadGameSuccess -= LoadLevel;
    }
}
