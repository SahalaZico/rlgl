using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplashscreenHandler : Singleton<SplashscreenHandler>
{
    [Header("Component")]
    [SerializeField] GameObject uiParent;
    [SerializeField] GameObject uiGame;
    [SerializeField] Button buttonStart;
    [SerializeField] Button buttonHowToPlay;
    [SerializeField] SkeletonGraphic spineBackground;

    [SerializeField] protected TMP_Text stateIdle = null;

    public void SetAvailable(bool state)
    {
        if (state)
        {
            stateIdle.gameObject.SetActive(false);
            buttonStart.gameObject.SetActive(true);
            buttonHowToPlay.gameObject.SetActive(true);
        } else
        {
            stateIdle.gameObject.SetActive(true);
            buttonStart.gameObject.SetActive(false);
            buttonHowToPlay.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        uiParent.SetActive(true);

        buttonStart.onClick.AddListener(OnButtonStart);
        buttonHowToPlay.onClick.AddListener(OnButtonHowToPlay);

        spineBackground.AnimationState.Complete += OnAnimationStateComplete;

        StartCoroutine(DelayFunction(.1f, () => uiGame.SetActive(false)));
    }

    private void OnAnimationStateComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry == null) return;
        if (trackEntry.ToString() == string.Empty) return;

        switch (trackEntry.ToString())
        {
            case "start":
                buttonStart.interactable = true;

                uiParent.SetActive(false);
                uiGame.SetActive(true);
                GameplayHandler.Instance.GameReset();

                spineBackground.AnimationState.SetAnimation(0, "idle", true);
                break;
        }
    }

    #region Listener
    private IEnumerator DelayFunction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private void OnButtonStart()
    {
        buttonStart.interactable = false;
        spineBackground.AnimationState.SetAnimation(0, "start", false);
    }

    private void OnButtonHowToPlay()
    {
        HowToPlayHandler.Instance.Show();
    }
    #endregion
}