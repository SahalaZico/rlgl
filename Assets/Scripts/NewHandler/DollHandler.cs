using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using Spine;

public class DollHandler : Singleton<DollHandler>
{
    [Header("Doll")]
    [SerializeField] SkeletonAnimation spineDoll;
    [SerializeField] SkeletonAnimation spineElectrocute;
    [SerializeField] float dollInactiveDurationMin, dollInactiveDurationMax;
    [SerializeField] float dollActiveDuration;
    [SerializeField] public bool isDollActive;
    [SerializeField] Image danger;

    [Header("Tower")]
    [SerializeField] SkeletonAnimation spineTowerLeft;
    [SerializeField] SkeletonAnimation spineTowerRight;

    [Header("Event")]
    [SerializeField] public UnityEvent towerShootEvent;

    bool isPause, isDollStart, isDollColorChanged, isDollAnimated;
    float interval, time;
    InitialBet initialBet;

    private void Start()
    {
        isPause = false;
        isDollStart = false;

        spineElectrocute.gameObject.SetActive(false);

        GameplayHandler.Instance.gameStartEvent.AddListener(OnGameStartEvent);
        GameplayHandler.Instance.gameWinLoseEvent.AddListener(OnGameWinLoseEvent);
        GameplayHandler.Instance.gamePauseEvent.AddListener(OnGamePauseEvent);
        towerShootEvent.AddListener(OnTowerShootEvent);

        spineDoll.AnimationState.Complete += OnAnimationStateComplete;
        spineElectrocute.AnimationState.Complete += OnAnimationStateComplete;
        spineTowerLeft.AnimationState.Complete += OnAnimationStateComplete;
        spineTowerRight.AnimationState.Complete += OnAnimationStateComplete;

        danger.gameObject.SetActive(false);
        danger.DOFade(0, .3f).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnAnimationStateComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry == null) return;
        if (trackEntry.ToString() == string.Empty) return;

        switch (trackEntry.ToString())
        {
            case "back_to_front":
                spineDoll.AnimationState.SetAnimation(0, "front_idle", true);
                break;
            case "back_to_front B":
                spineDoll.AnimationState.SetAnimation(0, "front_idle", true);
                break;
            case "front_to_back":
                spineDoll.AnimationState.SetAnimation(0, "back_idle", true);
                break;
            case "switch":
                spineTowerLeft.AnimationState.SetAnimation(0, "aim", true);
                spineTowerRight.AnimationState.SetAnimation(0, "aim", true);
                break;
            case "shoot":
                spineTowerLeft.AnimationState.SetAnimation(0, (isDollActive) ? "aim" : "idle", true);
                spineTowerRight.AnimationState.SetAnimation(0, (isDollActive) ? "aim" : "idle", true);
                break;
            case "explode":
                spineElectrocute.AnimationState.SetAnimation(0, "idle", true);
                break;
        }
    }

    public void SetStateDollInstantKill(SocketIO.AnomaliJSON.Response.RangeCrash state)
    {
        if (state == null)
            return;

        isDollActive = true;
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.alarmdoll, true);
        spineDoll.AnimationState.SetAnimation(0, "back_to_front B", false);
    }

    public void SetStateDoll(SocketIO.AnomaliJSON.Response.DollState state)
    {
        if (state == null)
            return;

        switch (state.status)
        {
            case "facing":
                isDollActive = true;
                spineTowerLeft.AnimationState.SetAnimation(0, "switch", false);
                spineTowerRight.AnimationState.SetAnimation(0, "switch", false);
                break;
            case "notFacing":
                isDollActive = false;
                spineDoll.AnimationState.SetAnimation(0, "front_to_back", false);
                spineTowerLeft.AnimationState.SetAnimation(0, "idle", true);
                spineTowerRight.AnimationState.SetAnimation(0, "idle", true);
                break;
            case "warning":
                SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.alarmdoll, true);
                spineDoll.AnimationState.SetAnimation(0, "back_to_front", false);
                break;
        }

        GameplayHandler.Instance.dollTurnAroundEvent?.Invoke(isDollActive);
    }

    private void Update()
    {
        //if (isDollStart && !isPause)
        //{
        //    if (GameplayHandler.Instance.currentMultiplier > initialBet.multiplier)
        //    {
        //        isDollStart = false;
        //        isDollActive = true;
        //        danger.gameObject.SetActive(true);

        //        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.alarmdoll, true);

        //        spineDoll.AnimationState.SetAnimation(0, "back_to_front B", false);
        //        spineTowerLeft.AnimationState.SetAnimation(0, "switch", false);
        //        spineTowerRight.AnimationState.SetAnimation(0, "switch", false);

        //        GameplayHandler.Instance.dollTurnAroundEvent?.Invoke(isDollActive);
        //        GameplayHandler.Instance.ShakeCamera();
        //        StartCoroutine(DelayFunction(.5f, () => { GameplayHandler.Instance.IsLose(true); }));

        //        return;
        //    }

        //    time += Time.deltaTime;

        //    // doll color red
        //    if (!isDollColorChanged && (time >= (interval - .6f)))
        //    {
        //        isDollColorChanged = true;

        //        spineDoll.Skeleton.SetColor(Color.red);
        //        StartCoroutine(DelayFunction(.6f, () => spineDoll.Skeleton.SetColor(Color.white)));
        //    }

        //    // spine doll start animated
        //    if (!isDollAnimated && (time >= (interval - 1.5f)))
        //    {
        //        isDollAnimated = true;

        //        if (!isDollActive)
        //            SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.alarmdoll, true);

        //        spineDoll.AnimationState.SetAnimation(0, (isDollActive) ? "front_to_back" : "back_to_front", false);

        //        if (!isDollActive)
        //        {
        //            spineTowerLeft.AnimationState.SetAnimation(0, "switch", false);
        //            spineTowerRight.AnimationState.SetAnimation(0, "switch", false);
        //        }
        //    }

        //    // refresh new interval
        //    if (time >= interval)
        //    {
        //        time = 0;

        //        TurnAround();
        //        RefreshNewInterval();
        //    }
        //}
    }

    #region Public Function
    public void ElectrocuteActive()
    {
        isDollStart = false;

        if (isDollActive)
        {
            isDollActive = false;
            danger.gameObject.SetActive(false);

            GameplayHandler.Instance.dollTurnAroundEvent?.Invoke(isDollActive);
        }

        spineElectrocute.gameObject.SetActive(true);
        spineElectrocute.AnimationState.SetAnimation(0, "explode", false);
        GameplayHandler.Instance.ShakeCamera();

        if (spineDoll.AnimationName == "back_to_front" || spineDoll.AnimationName == "front_idle")
        {
            spineDoll.AnimationState.SetAnimation(0, "back_idle", true);
            spineDoll.Skeleton.SetColor(Color.white);
        }

        isDollColorChanged = false;
        isDollAnimated = false;
        interval += 10f;

        StartCoroutine(DelayFunction(10f, () => {
            if (GameplayHandler.Instance.isGameStart)
            {
                isDollStart = true;
                spineElectrocute.gameObject.SetActive(false);
            }
        }));
    }
    #endregion

    #region Private Function
    private IEnumerator DelayFunction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private void Reset()
    {
        danger.gameObject.SetActive(false);
        spineElectrocute.gameObject.SetActive(false);
    }

    private void DollStart()
    {
        initialBet = APIHandler.Instance.initialBet;

        isDollStart = true;
        isDollActive = false;
        danger.gameObject.SetActive(false);

        spineDoll.Skeleton.SetColor(Color.white);
        spineDoll.AnimationState.SetAnimation(0, "back_idle", true);

        RefreshNewInterval();
    }

    private void TurnAround()
    {
        isDollActive = !isDollActive;
        danger.gameObject.SetActive(isDollActive);

        if (!isDollActive)
            SoundHandler.Instance.DeleteIndipendentSfx(SoundHandler.SFX_NAME.alarmdoll);

        GameplayHandler.Instance.dollTurnAroundEvent?.Invoke(isDollActive);

        if (isDollActive)
            GameplayHandler.Instance.ShakeCamera();
        else
        {
            spineTowerLeft.AnimationState.SetAnimation(0, "idle", true);
            spineTowerRight.AnimationState.SetAnimation(0, "idle", true);
        }

        StartCoroutine(DelayFunction(.3f, () => { GameplayHandler.Instance.IsLose(); }));
    }

    private void RefreshNewInterval()
    {
        isDollColorChanged = false;
        isDollAnimated = false;

        time = 0f;
        interval = isDollActive ? dollActiveDuration : Random.Range(dollInactiveDurationMin, dollInactiveDurationMax);
    }
    #endregion

    #region Listener
    private void OnGameStartEvent(bool isGameStart)
    {
        if (isGameStart)
            DollStart();
        else
            Reset();
    }

    private void OnGameWinLoseEvent(bool isWin)
    {
        isDollStart = false;
    }

    private void OnGamePauseEvent(bool isGamePaused)
    {
        isPause = isGamePaused;

        spineDoll.timeScale = isPause ? 0 : 1;
    }

    private void OnTowerShootEvent()
    {
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.charactershot, true);

        spineTowerLeft.AnimationState.SetAnimation(0, "shoot", true);
        spineTowerRight.AnimationState.SetAnimation(0, "shoot", true);
    }
    #endregion
}