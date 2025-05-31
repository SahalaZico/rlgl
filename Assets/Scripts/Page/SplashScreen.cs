using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : Singleton<SplashScreen>
{
    [Header("UI")]
    [SerializeField] GameObject parent;

    [Header("Button")]
    [SerializeField] Button buttonStartGame;
    [SerializeField] Button buttonHowToPlay;

    private void Awake()
    {
        parent.SetActive(true);

        buttonStartGame.onClick.AddListener(OnClickButtonStartGame);
        buttonHowToPlay.onClick.AddListener(OnClickButtonHowToPlay);

        StatusHandler.Instance.statusChangedEvent.AddListener(OnStatusChanged);
    }

    #region Private Function
    void RefreshOnSplashScreen()
    {
        parent.SetActive(true);
    }
    #endregion

    #region Listener
    void OnClickButtonStartGame()
    {
        Debug.Log("button start game..");

        parent.SetActive(false);
        StatusHandler.Instance.ChangeStatus(StatusHandler.STATUS.OnBet, true);
    }

    void OnClickButtonHowToPlay()
    {
        Debug.Log("button how to play..");
    }

    private void OnStatusChanged(StatusHandler.STATUS status)
    {
        switch (status)
        {
            case StatusHandler.STATUS.OnSplashScreen:
                break;
        }    
    }
    #endregion
}