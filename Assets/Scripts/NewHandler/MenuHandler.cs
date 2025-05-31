using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;

public class MenuHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RectTransform uiContent;

    [Header("Button")]
    [SerializeField] Button buttonMenu;
    [SerializeField] Button buttonTutorial;
    [SerializeField] Button buttonHistory;
    [SerializeField] Button buttonAudio;
    [SerializeField] Button buttonBack;

    float showYAnchoredPosition;
    float hideYAnchoredPosition;
    bool isTweenActive = false;

    private void Awake()
    {
        showYAnchoredPosition = 100f;
        hideYAnchoredPosition = 650f;
        uiContent.anchoredPosition = new Vector2(0, hideYAnchoredPosition);

        buttonMenu.onClick.AddListener(OnClickButtonMenu);
        buttonTutorial.onClick.AddListener(OnClickButtonTutorial);
        buttonHistory.onClick.AddListener(OnClickButtonHistory);
        buttonAudio.onClick.AddListener(OnClickButtonAudio);
        buttonBack.onClick.AddListener(OnClickButtonBack);
    }

    #region Listener
    private void OnClickButtonMenu()
    {
        Debug.Log("button menu clicked..");

        if (isTweenActive) return;

        isTweenActive = true;

        float targetYPosition = (uiContent.localPosition.y > showYAnchoredPosition)
            ? showYAnchoredPosition : hideYAnchoredPosition;

        uiContent.DOAnchorPos(new Vector2(0, targetYPosition), .3f).OnComplete(() => isTweenActive = false);
    }

    private void OnClickButtonTutorial()
    {
        Debug.Log("button menu tutorial..");
        HowToPlayHandler.Instance.Show();
    }

    private void OnClickButtonHistory()
    {
        Debug.Log("button menu history..");
        //HistoryHandler.Instance.Show();
    }

    private void OnClickButtonAudio()
    {
        Debug.Log("button menu audio..");
        SoundHandler.Instance.Show();
    }

    private void OnClickButtonBack()
    {
        //if (StatusHandler.Instance.status != StatusHandler.STATUS.OnUpgrade) return;
        //if (AutoPlayHandler.Instance.isAutoPlay) return;

        Debug.Log("button menu back..");
        //StatusHandler.Instance.ChangeStatus(StatusHandler.STATUS.OnSplashScreen, true);
    }
    #endregion
}