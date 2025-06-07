using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnomaliRunnerGameplay : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField] protected List<AnomaliRunnerNPC> npcs = new List<AnomaliRunnerNPC>();

    [Header("Main Props")]
    [SerializeField] protected float maxRange = 1.0f;
    [SerializeField] protected Transform transStart = null;
    [SerializeField] protected Transform transEnd = null;

    [Header("Player's Props")]
    [SerializeField] protected Transform transPlayer = null;
    [SerializeField] protected SkeletonAnimation skelePlayer = null;
    [SerializeField] protected SkeletonGraphic skeleCoinAbsorb = null;

    [Header("Doll's Props")]
    [SerializeField] protected SkeletonAnimation skeleDoll = null;
    [SerializeField] protected Image uiDollFacing = null;
    protected DG.Tweening.Sequence currentUiDollTween = null;

    [Header("Watcher's Props")]
    [SerializeField] protected SkeletonAnimation skeleWatcherLeft = null;
    [SerializeField] protected SkeletonAnimation skeleWatcherRight = null;
    protected DG.Tweening.Sequence currentWatcherSeq = null;

    public enum WatcherState
    {
        Idle = 0,
        Aim = 1
    }
    protected WatcherState watcherState = WatcherState.Idle;

    [Header("Bonus Handler")]
    [SerializeField] protected AnomaliRunnerBonusHandler bonusHandler = null;
    public AnomaliRunnerBonusHandler BonusHandler
    {
        get
        {
            return bonusHandler;
        }
    }

    protected bool gameStart = false;
    protected bool isPlayerMove = false;
    protected bool firstTimeAim = false;
    protected float currentRange = 0f;
    protected float cacheCurrentRange = 0f;
    protected string cacheFinalBonus = "";
    protected double currentFinishMultiplier = 0f;
    protected DONInputSystem controls = null;
    protected Tweener currentProgressTween = null;
    protected Tweener currentCoinAbsorb = null;

    public void SetFinishData(double cacheFinishMultiplier, string cacheFinalBonus)
    {
        this.cacheFinalBonus = cacheFinalBonus;
        currentFinishMultiplier = cacheFinishMultiplier;
    }

    public float CurrentRange
    {
        get
        {
            return currentRange;
        }
        set
        {
            currentRange = value;

            if (UIGameplay.Instance != null)
            {
                UIGameplay.Instance.UpdateTextMultiplierInrunByAmount(currentRange);
                UIGameplay.Instance.UpdateTextCashoutInrunByAmount(currentRange);
            }
        }
    }

    protected void ClearForNPC()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] == null)
                continue;

            npcs[i].Clear();
        }
    }

    protected void ConstructTrackAndAvatarsForNPC()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] == null)
                continue;

            npcs[i].ConstructTrackAndAvatar(transStart.position, transEnd.position, maxRange);
        }
    }

    protected void UpdateStateOfNPC(AnomaliRunnerNPC.ActionState state)
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] == null)
                continue;

            npcs[i].SetState(state);
        }
    }

    protected void ConstructTrackAndAvatars()
    {
        CurrentRange = 0f;
        cacheCurrentRange = 0f;
        transPlayer.DOKill();
        transPlayer.position = transStart.position;
        transPlayer.DOMoveZ(transEnd.position.z, maxRange).SetEase(Ease.Linear);
        transPlayer.DOPause();
        transPlayer.DOGoto(currentRange * maxRange);

        if (bonusHandler != null) {
            bonusHandler.ConstructByTrack(maxRange, transStart.position, transEnd.position);
        }

        if (UIGameplay.Instance.DoAutoPlay)
        {
            ClientSocketIO.Instance.SendEvent("startRunning");
            AudioManager.Instance.Play("sfx", "move", true);
        }

        firstTimeAim = true;
    }

    public bool GameStart
    {
        get
        {
            return gameStart;
        }
        set
        {
            bool oldGameStartState = gameStart;
            gameStart = value;
            if (gameStart != oldGameStartState)
            {
                if (gameStart)
                {
                    ConstructTrackAndAvatars();
                    ConstructTrackAndAvatarsForNPC();
                }
            }
        }
    }

    public void Clear()
    {
        StopWarnScreen();

        CurrentRange = 0f;
        cacheCurrentRange = 0f;
        transPlayer.DOKill();
        transPlayer.position = transStart.position;
        isPlayerMove = false;
        int indexRandom = Random.Range(0, 3) + 1;
        skelePlayer.AnimationState.SetAnimation(0, "stop_variation_" + indexRandom, false);
        AudioManager.Instance.Stop("sfx", "move");

        ClearForNPC();

        if (currentWatcherSeq != null)
        {
            if (currentWatcherSeq.IsActive())
                currentWatcherSeq.Kill();
        }

        SetWatcherState(WatcherState.Idle);
    }

    protected void OnMoveStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Debug.Log("Press Down Run");
        if (!gameStart)
            return;

        ClientSocketIO.Instance.SendEvent("startRunning");
        AudioManager.Instance.Play("sfx", "move", true);
    }

    protected void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Debug.Log("Press Up Run");
        if (!gameStart)
            return;

        ClientSocketIO.Instance.SendEvent("stopRunning");
        AudioManager.Instance.Stop("sfx", "move");
    }

    public void StopWarnScreen()
    {
        if (currentUiDollTween != null)
            currentUiDollTween.Kill();

        Color newColor = Color.white;
        newColor.a = 0f;
        uiDollFacing.color = newColor;

        AudioManager.Instance.Stop("sfx", "alarm");
    }

    public void PlayWarnScreen()
    {
        if (currentUiDollTween != null)
            currentUiDollTween.Kill();

        currentUiDollTween = DOTween.Sequence();
        currentUiDollTween.OnStart(() => {
            Color newColor = Color.white;
            newColor.a = 0f;

            uiDollFacing.color = newColor;
        });
        currentUiDollTween.Append(uiDollFacing.DOFade(1f, 0.5f));
        currentUiDollTween.Append(uiDollFacing.DOFade(0f, 0.5f));
        currentUiDollTween.SetLoops(-1);
    }

    public void PlayDefeat()
    {
        skelePlayer.AnimationState.SetAnimation(0, "shot_start", false);

        GameStart = false;
        AudioManager.Instance.Play("sfx", "shot");
        AudioManager.Instance.Stop("sfx", "move");

        UpdateStateOfNPC(AnomaliRunnerNPC.ActionState.Idle);

        UIGameplay.Instance.ShowWinOrLosePanel(false);
    }

    public void PlayVictory(double endMultiplier, double endWinAmount)
    {
        GameStart = false;

        string tier = "low";
        CurrentRange = (float) endMultiplier;

        if (CurrentRange >= 15)
        {
            if (!string.IsNullOrEmpty(cacheFinalBonus))
            {
                if (cacheFinalBonus == "diamond")
                    tier = "high";
                else if (cacheFinalBonus == "coin" || cacheFinalBonus == "cash")
                    tier = "mid";
            }
        }

        skelePlayer.AnimationState.SetAnimation(0, "reward_" + tier + "_start", false);
        UIGameplay.Instance.ShowWinOrLosePanel(true, endWinAmount);

        UpdateStateOfNPC(AnomaliRunnerNPC.ActionState.Idle);

        AudioManager.Instance.Stop("sfx", "move");
    }

    protected void WatcherShootRandomNPC(int maxNumShoot)
    {
        int currentNumShoot = 0;
        if (currentWatcherSeq != null)
        {
            if (currentWatcherSeq.IsActive())
                currentWatcherSeq.Kill();
        }
        currentWatcherSeq = DOTween.Sequence();
        for (int i = 0; i < npcs.Count; i++) {
            if (npcs[i] == null)
                continue;

            if (npcs[i].CurrentState == AnomaliRunnerNPC.ActionState.Finish)
                continue;

            if (npcs[i].CurrentState == AnomaliRunnerNPC.ActionState.Dead)
                continue;

            var fetchNPC = npcs[i];
            var fetchShootIndex = currentNumShoot;
            currentWatcherSeq.AppendInterval(0.2f);
            currentWatcherSeq.AppendCallback(() => {
                fetchNPC.SetState(AnomaliRunnerNPC.ActionState.Dead);
                if (fetchShootIndex % 2 == 0)
                    skeleWatcherLeft.AnimationState.SetAnimation(0, "shoot", false);
                else
                    skeleWatcherRight.AnimationState.SetAnimation(0, "shoot", false);
                AudioManager.Instance.Play("sfx", "shot" + fetchShootIndex);
            });
            currentNumShoot++;

            if (currentNumShoot >= maxNumShoot)
                break;
        }
    }

    public void SetWatcherState(WatcherState state)
    {
        watcherState = state;

        switch (watcherState) {
            case WatcherState.Idle:
                skeleWatcherLeft.AnimationState.SetAnimation(0, "idle", true);
                skeleWatcherRight.AnimationState.SetAnimation(0, "idle", true);
                break;
            case WatcherState.Aim:
                skeleWatcherLeft.AnimationState.SetAnimation(0, "aim", true);
                skeleWatcherRight.AnimationState.SetAnimation(0, "aim", true);
                break;
        }
    }

    #region OnGameplayResponseBySocket
    public void CharacterMoveUpdateBySocket(SocketIO.AnomaliJSON.Response.RangeUpdate output)
    {
        if (!gameStart)
            return;

        var floatParsed = (float)double.Parse(output.range, CultureInfo.InvariantCulture);
        cacheCurrentRange = floatParsed;

        if (currentProgressTween != null)
            currentProgressTween.Kill();

        if (currentCoinAbsorb != null)
            currentCoinAbsorb.Kill();

        currentProgressTween = DOVirtual.Float(currentRange, floatParsed, 0.5f, (floatUpdate) => {
            transPlayer.DOGoto(floatUpdate);
            CurrentRange = floatUpdate;
            //PrintMultiplier
        }).SetEase(Ease.Linear);

        if (!isPlayerMove)
        {
            skelePlayer.AnimationState.SetAnimation(0, "run", true);
            skeleCoinAbsorb.AnimationState.SetAnimation(0, "Loop", true);

            isPlayerMove = true;
        }

        currentCoinAbsorb = skeleCoinAbsorb.transform.DOScale(Vector3.one * 1.3f, 0.3f);

        if (bonusHandler !=  null)
        {
            bonusHandler.UpdateRangeByRunner(floatParsed);
        }
    }

    public void CharacterMoveStopBySocket()
    {
        if (!gameStart)
            return;

        if (currentCoinAbsorb != null)
            currentCoinAbsorb.Kill();

        currentCoinAbsorb = skeleCoinAbsorb.transform.DOScale(Vector3.zero, 0.25f);
        skeleCoinAbsorb.AnimationState.SetAnimation(0, "Loop", false);

        isPlayerMove = false;
        int indexRandom = Random.Range(0, 3) + 1;
        skelePlayer.AnimationState.SetAnimation(0, "stop_variation_" + indexRandom, false);
    }

    public void OnDollStateUpdate(SocketIO.AnomaliJSON.Response.DollState dollState)
    {
        switch (dollState.status)
        {
            case "notFacing":
                skeleDoll.AnimationState.SetAnimation(0, "front_to_back", false);
                AudioManager.Instance.Stop("sfx", "alarm");

                StopWarnScreen();
                SetWatcherState(WatcherState.Idle);
                UpdateStateOfNPC(AnomaliRunnerNPC.ActionState.Run);
                break;
            case "facing":
                SetWatcherState(WatcherState.Aim);
                UpdateStateOfNPC(AnomaliRunnerNPC.ActionState.Idle);
                int numAttacks = Random.Range(0, 4);
                if (!firstTimeAim)
                    WatcherShootRandomNPC(numAttacks);
                else
                    firstTimeAim = false;
                break;
            case "warning":
                skeleDoll.AnimationState.SetAnimation(0, "back_to_front", false);
                AudioManager.Instance.Play("sfx", "alarm", false);

                PlayWarnScreen();
                break;
        }

        Debug.Log("Doll State: " + dollState.status);
    }

    public void OnCaughtByDoll()
    {
        PlayDefeat();
    }

    public void OnForceCaughtByDoll(SocketIO.AnomaliJSON.Response.RangeCrash crashRoot)
    {
        skeleDoll.AnimationState.SetAnimation(0, "back_to_front B", false);
        AudioManager.Instance.Play("sfx", "alarm", false);

        PlayWarnScreen();
        PlayDefeat();
    }

    public void OnChashoutSuccess(SocketIO.AnomaliJSON.Response.GameInfo infoCashout)
    {
        PlayVictory(infoCashout.data.range, infoCashout.data.winAmount);

        ToastHandler.Instance.Close();
    }
    #endregion

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new DONInputSystem();
            controls.CharacterControl.Move.started += this.OnMoveStarted;
            controls.CharacterControl.Move.canceled += this.OnMoveCanceled;
        }

        controls.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }

    public void SetMaxRange(float input)
    {
        maxRange = input;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStart = false;

        skelePlayer.AnimationState.Complete += (track) =>
        {
            if (track == null)
                return;

            if (track.Animation == null)
                return;

            switch (track.Animation.Name)
            {
                case "stop_variation_1":
                    if (!isPlayerMove)
                        skelePlayer.AnimationState.SetAnimation(0, "stop_variation_1_loop", true);
                    break;
                case "stop_variation_2":
                    if (!isPlayerMove)
                        skelePlayer.AnimationState.SetAnimation(0, "stop_variation_2_loop", true);
                    break;
                case "stop_variation_3":
                    if (!isPlayerMove)
                        skelePlayer.AnimationState.SetAnimation(0, "stop_variation_3_loop", true);
                    break;
                case "reward_low_start":
                    TrackEntry entryRewardLow = skelePlayer.AnimationState.SetAnimation(0, "reward_low_idle", true);
                    entryRewardLow.MixDuration = 0f;
                    break;
                case "reward_mid_start":
                    TrackEntry entryRewardMid = skelePlayer.AnimationState.SetAnimation(0, "reward_mid_idle", true);
                    entryRewardMid.MixDuration = 0f;
                    break;
                case "reward_high_start":
                    TrackEntry entryRewardHigh = skelePlayer.AnimationState.SetAnimation(0, "reward_high_idle", true);
                    entryRewardHigh.MixDuration = 0f;
                    break;
                case "shot_start":
                    TrackEntry entryShot = skelePlayer.AnimationState.SetAnimation(0, "shot_idle", false);
                    entryShot.MixDuration = 0f;
                    break;
            }
        };

        skeleDoll.AnimationState.Complete += (track) =>
        {
            if (track == null)
                return;

            if (track.Animation == null)
                return;

            switch (track.Animation.Name)
            {
                case "back_to_front":
                    skeleDoll.AnimationState.SetAnimation(0, "front_idle", true);
                    break;
                case "back_to_front B":
                    skeleDoll.AnimationState.SetAnimation(0, "front_idle", true);
                    break;
                case "front_to_back":
                    skeleDoll.AnimationState.SetAnimation(0, "back_idle", true);
                    break;
            }
        };

        skeleWatcherLeft.AnimationState.Complete += (track) =>
        {
            if (track == null)
                return;

            if (track.Animation == null)
                return;

            switch (track.Animation.Name)
            {
                case "shoot":
                    skeleWatcherLeft.AnimationState.SetAnimation(0, "aim", true);
                    break;
            }
        };
        skeleWatcherRight.AnimationState.Complete += (track) =>
        {
            if (track == null)
                return;

            if (track.Animation == null)
                return;

            switch (track.Animation.Name)
            {
                case "shoot":
                    skeleWatcherRight.AnimationState.SetAnimation(0, "aim", true);
                    break;
            }
        };

        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] == null)
                continue;

            npcs[i].Init();
        }
    }

    private void Update()
    {
        if (!GameStart)
            return;

        if (UIGameplay.Instance.DoAutoPlay)
        {
            if (cacheCurrentRange >= UIGameplay.Instance.GetSelectedStopMultiplier())
            {
                UIGameplay.Instance.OnClickCashout();
                GameStart = false;
            }
        }

        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] == null)
                continue;

            if (npcs[i].CurrentState != AnomaliRunnerNPC.ActionState.Run)
                continue;

            npcs[i].UpdateRange(Time.deltaTime);
        }
    }
}
