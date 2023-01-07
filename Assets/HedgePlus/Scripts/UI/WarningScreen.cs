using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class WarningScreen : MonoBehaviour
{
    StageProgress _stg;
    LoadingScreenBehavior loadingScreen;
    public PauseMenuControl menu;
    public enum WarnScreenType { Restart, Menu, Quit }
    [HideInInspector] WarnScreenType confirmBehavior;
    bool IsRestart;
    int LevelIndex;
    private void Start()
    {
        _stg = StageProgress.instance;
        loadingScreen = FindObjectOfType<LoadingScreenBehavior>();

        if (loadingScreen != null)
        {
            LevelIndex = ThisLevelsIndex();
        }
    }

    public void ConfirmBehavior()
    {
        /*
        if (IsRestart)
        {
            if (loadingScreen != null)
            {
                menu.Unpause();
                loadingScreen.LoadLevel(LevelIndex);
            } else
            {
                Debug.LogError("Cannot restart the level, no loading screen object found");
            }
        } else
        {
            StartCoroutine("ExitToTitle");
        }
        */
        switch (confirmBehavior)
        {
            case WarnScreenType.Restart:
                if (loadingScreen != null)
                {
                    menu.Unpause();
                    loadingScreen.LoadLevel(LevelIndex);
                }
                else
                {
                    Debug.LogError("Cannot restart the level, no loading screen object found");
                }
                break;
            case WarnScreenType.Menu:
                StartCoroutine("ExitToTitle");
                break;
            case WarnScreenType.Quit:
                Application.Quit();
                break;
        }
    }

    public void SetWarningType (int index)
    {
        confirmBehavior = (WarnScreenType)index;
    }

    IEnumerator ExitToTitle()
    {
        menu.Unpause();
        _stg.fadeAnim.SetBool("Fade", true);
        SceneManager.LoadSceneAsync("TitleScene");
        yield return null;
    }

    int ThisLevelsIndex ()
    {
        int index = 0;
        int ThisScene = SceneManager.GetActiveScene().buildIndex;
        for (int i = 0; i < loadingScreen.Levels.Length; i++)
        {
            int _i = loadingScreen.Levels[i].SceneIndex;
            if (ThisScene == _i)
            {
                index = i;
            }
        }

        return index;
    }
}
