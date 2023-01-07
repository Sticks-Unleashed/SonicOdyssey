using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuControl : MonoBehaviour
{
    public static bool Paused;
    public Animator menuAnimator;
    int MenuIndex;
    public GameObject[] Menus;

    private void Update()
    {
        for (int i = 0; i < Menus.Length; i++)
        {
            if (i == MenuIndex)
            {
                Menus[i].SetActive(true);
            } else
            {
                Menus[i].SetActive(false);
            }
        }

        if (Input.GetKeyDown("joystick button 7") || Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Paused)
            {
                Paused = true;
                Time.timeScale = 0;
            }
        }

        menuAnimator.SetBool("Paused", Paused);

        //Lock and hide the mouse
        Cursor.visible = Paused;
        Cursor.lockState = Paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Unpause()
    {
        Paused = false;
        Time.timeScale = 1;
    }

    public void SetMenu (int index)
    {
        StartCoroutine(MenuSwitch(index));
    }

    IEnumerator MenuSwitch (int index)
    {
        menuAnimator.SetTrigger("MenuChange");
        yield return new WaitForSecondsRealtime(0.3f);
        MenuIndex = index;
    }
}
