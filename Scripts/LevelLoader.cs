using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;
    public int sceneIndex;

    public UnityEvent PreLoadSceneEvent;

    public void SetSceneIndex(int scene)
    {
        sceneIndex = scene;
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(sceneIndex));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        if(PreLoadSceneEvent != null)
        {
            PreLoadSceneEvent.Invoke();
        }

        SceneManager.LoadScene(levelIndex);

    }
}
