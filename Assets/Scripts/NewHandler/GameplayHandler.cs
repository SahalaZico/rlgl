using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using TMPro;
using Spine.Unity;
using UnityEngine.UI;
using System.Globalization;
using SocketIO.AnomaliJSON.Response.DropItem;
using Newtonsoft.Json;
using UnityEngine.TextCore.Text;

public class GameplayHandler : Singleton<GameplayHandler>
{
    [Header("Game Variable")]
    [SerializeField] public float movementDuration;
    [SerializeField] public float gameDuration;

    [Header("Character")]
    [SerializeField] SkeletonAnimation spineCharacter;

    [Header("Track")]
    [SerializeField] Transform startPosition;
    [SerializeField] Transform endPosition;
    [SerializeField] List<Transform> listTrack;
    [SerializeField] TMP_Text textMultiplier;
    [SerializeField] TMP_Text textCashout;
    [SerializeField] TMP_Text textTimer;

    [Header("Button Etc")]
    [SerializeField] Button buttonRun;
    [SerializeField] SkeletonGraphic spineDynamite;
    [SerializeField] GameObject autoCashOut;

    [Header("Event")]
    [SerializeField] public UnityEvent<bool> gamePauseEvent;
    [SerializeField] public UnityEvent<bool> gameStartEvent;
    [SerializeField] public UnityEvent<bool> dollTurnAroundEvent;
    [SerializeField] public UnityEvent<bool> gameWinLoseEvent;
    [SerializeField] public UnityEvent<Vector3> bonusShowEvent;
    [SerializeField] public UnityEvent<InitialBetBonus> bonusEvent;
    [SerializeField] public UnityEvent<bool> bonusStartEvent;

    public bool isGameStart;
    public int currentTrack;
    public long inputBet = 0;
    public double inputAutoCashout = 1.0f;
    public bool isAutoCashout = false;
    public int roundAutoCashout = 0;
    public float timer;
    public float currentMultiplier;
    public double currentFinishMultiplier;
    public float currentRewardCash;
    public bool isCharacterMove = false;
    public bool isRun = false;

    bool isPause, isBonusShow, isDebounce, isUnder10Sec, doCashout;
    float distanceReached, endPositionZ, maximumMultiplier;

    List<InitialBetBonus> betBonus;

    float currentProgress = 0f;

    bool sendingBet = false;

    List<string> cacheBonus = new List<string>();
    List<double> cacheBonusPosition = new List<double>();
    List<double> cacheBonusValue = new List<double>();

    string cacheFinalBonus = "";

    public bool GetSendingBet()
    {
        return sendingBet;
    }

    public void SetSendingBet(bool state)
    {
        sendingBet = state;
    }

    public void AddBonusByRule(string catchItem, double catchPosition, object value)
    {
        if (value == null)
            return;

        if (value is string)
        {
            var fetchValue = value.ToString();
            if (string.IsNullOrEmpty(fetchValue))
            {
                return;
            } else
            {
                if (fetchValue == "empty")
                    return;
            }
        }

        if (!string.IsNullOrEmpty(catchItem))
        {
            if (catchItem == "empty")
                return;

            if (catchItem == "timer")
                catchItem = "add_time";
            if (catchItem == "coin")
                catchItem = "money";
            if (catchItem == "cash")
                catchItem = "money";
            cacheBonus.Add(catchItem);
            cacheBonusPosition.Add(catchPosition);

            if (value is double)
            {
                cacheBonusValue.Add((double) value);
            } else
            {
                cacheBonusValue.Add(0.0);
            }
        }
    }

    public void OnBetFailedBySocket(SocketIO.AnomaliJSON.Response.GameInfo output)
    {
        sendingBet = false;
    }

    public void OnBetConfirmedBySocket(SocketIO.AnomaliJSON.Response.GameInfo output)
    {
        try
        {
            isBonusShow = false;

            cacheBonus = new List<string>();
            cacheBonusPosition = new List<double>();
            cacheBonusValue = new List<double>();

            var drops = output.data.drops;

            if (drops.ContainsKey("2"))
            {
                string catchItem = drops["2"].item;
                double catchPosition = drops["2"].range;
                object value = drops["2"].value;
                AddBonusByRule(catchItem, catchPosition, value);
            }
            if (drops.ContainsKey("3"))
            {
                string catchItem = drops["3"].item;
                double catchPosition = drops["3"].range;
                object value = drops["3"].value;
                AddBonusByRule(catchItem, catchPosition, value);
            }
            if (drops.ContainsKey("4"))
            {
                string catchItem = drops["4"].item;
                double catchPosition = drops["4"].range;
                object value = drops["4"].value;
                AddBonusByRule(catchItem, catchPosition, value);
            }

            if (drops.ContainsKey("5"))
            {
                cacheFinalBonus = drops["5"].item;
                if (drops["5"].value != null)
                {
                    if (drops["5"].value is double parsedValue)
                        currentFinishMultiplier = parsedValue;
                    else if (drops["5"].value is int parsedInt)
                        currentFinishMultiplier = parsedInt;
                    else
                        currentFinishMultiplier = 0.0;
                }
                else
                {
                    currentFinishMultiplier = 0.0;
                }
            }

            InitialBet initialBet = APIHandler.Instance.initialBet;
            List<InitialBetBonus> initialBetBonus = new List<InitialBetBonus>();
            //string[] bonusType = new string[] { "add_time", "electrocute", "bomb", "speed_up" };
            for (int i = 0; i < cacheBonus.Count; i++)
            {
                string type = cacheBonus[i];
                initialBetBonus.Add(new InitialBetBonus() { type = type, show_at_multiplier = (float)cacheBonusPosition[i], value = (float)cacheBonusValue[i] });
            }
            initialBet.bet_bonus = initialBetBonus.ToArray();
            betBonus = new List<InitialBetBonus>(initialBet.bet_bonus);

        } catch (System.Exception ex)
        {
            Debug.Log("Can not passed bonus. Reason: " + ex.Message);
        }

        currentMultiplier = 0;
        currentRewardCash = 0;

        GameStart();

        sendingBet = false;

        string jsonResponse = JsonConvert.SerializeObject(output);
        Debug.Log("Bet Confirmed: " + jsonResponse);
    }

    public void RestrictToRunBySocket(SocketIO.AnomaliJSON.Response.GameInfo output)
    {
        ToastHandler.Instance.Show(output.message, 1f);
    }

    public void SetTrakConfig(SocketIO.AnomaliJSON.Response.RangeData.Root config)
    {
        movementDuration = (float) config.baseMaxRange;
        maximumMultiplier = movementDuration;
    }

    // Start is called before the first frame update
    void Start()
    {
        ButtonRun.Instance.buttonDownEvent.AddListener(OnButtonDownEvent);
        ButtonRun.Instance.buttonUpEvent.AddListener(OnButtonUpEvent);

        spineCharacter.AnimationState.Complete += OnAnimationStateComplete;
        spineDynamite.AnimationState.Complete += OnAnimationStateComplete;

        gamePauseEvent.AddListener(OnGamePauseEvent);
        dollTurnAroundEvent.AddListener(OnDollTurnAroundEvent);
    }

    private void OnDollTurnAroundEvent(bool isActive)
    {
        if (!isAutoCashout)
            return;

        if (isActive)
            OnButtonUpEvent();
        else
            OnButtonDownEvent();
    }
    
    private void Update()
    {
        if (isGameStart && !isPause)
        {
            if (isCharacterMove)
            {
                //PrintMultiplier();

                //if (isAutoCashout)
                //{
                //    if (currentMultiplier >= inputAutoCashout)
                //    {
                //        OnButtonUpEvent();
                //        //Win();

                //        return;
                //    }
                //}

                CheckBonus();
            }
            
            CheckCurrentTrack();
            //CheckLoseCondition();

            //if (!DollHandler.Instance.isDollActive)
            //    PrintTimer();
        }
    }

    private void PrintMultiplier()
    {
        distanceReached = endPositionZ - (endPositionZ - spineCharacter.transform.position.z);
        //float percentage = (distanceReached / endPositionZ);
        currentMultiplier = currentProgress;
        textMultiplier.text = currentMultiplier.ToString("F2") + "x";
        textMultiplier.transform.GetChild(0).GetComponent<TMP_Text>().text = textMultiplier.text;

        long amountToCollect = (long) (inputBet * currentMultiplier);
        var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };
        string result = amountToCollect.ToString("#,##0", nfi);
        textCashout.text = result;
    }

    private void CheckBonus()
    {
        if (betBonus == null || betBonus.Count <= 0)
            return;

        if (!isBonusShow)
        {
            float showBonusMultiplier = betBonus[0].show_at_multiplier - 1.0f;
            if (showBonusMultiplier <= 1.0f)
                showBonusMultiplier = 1.0f;

            if (currentMultiplier >= showBonusMultiplier)
            {
                isBonusShow = true;

                bonusShowEvent?.Invoke(spineCharacter.transform.position + new Vector3(0,0,12f));
            }
        }

        if (currentMultiplier >= betBonus[0].show_at_multiplier)
        {
            bonusEvent?.Invoke(betBonus[0]);

            betBonus.RemoveAt(0);
            isBonusShow = false;
        }
    }

    private void CheckCurrentTrack()
    {
        if ((currentTrack < listTrack.Count) && listTrack[currentTrack].position.z <= spineCharacter.transform.position.z)
        {
            currentTrack += 1;
            Debug.Log("Current track: " + currentTrack);
        }
    }

    private void CheckLoseCondition()
    {
        timer += Time.deltaTime;
        if (timer >= gameDuration)
        {
            timer = gameDuration;
            Lose();
            Debug.Log("Lose because of time up");

            spineDynamite.AnimationState.SetAnimation(0, "explode", false);
            SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.explosiontimeup, true);
        }
    }

    private void OnAnimationStateComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry == null) return;
        if (trackEntry.ToString() == string.Empty) return;

        switch (trackEntry.ToString())
        {
            case "stop_variation_1":
                spineCharacter.AnimationState.SetAnimation(0, "stop_variation_1_loop", true);
                break;
            case "stop_variation_2":
                spineCharacter.AnimationState.SetAnimation(0, "stop_variation_2_loop", true);
                break;
            case "stop_variation_3":
                spineCharacter.AnimationState.SetAnimation(0, "stop_variation_3_loop", true);
                break;
            case "reward_low_start":
                spineCharacter.AnimationState.AddAnimation(0, "reward_low_idle", true, 0f);
                break;
            case "reward_mid_start":
                spineCharacter.AnimationState.AddAnimation(0, "reward_mid_idle", true, 0f);
                break;
            case "reward_high_start":
                spineCharacter.AnimationState.AddAnimation(0, "reward_high_idle", true, 0f);
                break;
            case "shot_start":
                spineCharacter.AnimationState.SetAnimation(0, "shot_idle", false);
                break;
            case "explode":
                spineDynamite.AnimationState.SetAnimation(0, "idle", true);
                break;
            case "stop_variation_1_loop":
                if (isCharacterMove)
                    spineCharacter.AnimationState.SetAnimation(0, "run", true);
                break;
            case "stop_variation_2_loop":
                if (isCharacterMove)
                    spineCharacter.AnimationState.SetAnimation(0, "run", true);
                break;
            case "stop_variation_3_loop":
                if (isCharacterMove)
                    spineCharacter.AnimationState.SetAnimation(0, "run", true);
                break;
        }
    }

    #region Public Function
    public void GameShow()
    {
        Reset();
    }

    public void GameReset()
    {
        Reset();

        gameStartEvent?.Invoke(false);
    }

    public void GameStart()
    {
        Debug.Log("Game start!");

        isGameStart = true;

        autoCashOut.SetActive(isAutoCashout);
        doCashout = false;

        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.placebetbutton, true);
        gameStartEvent?.Invoke(true);

        if(isAutoCashout)
            OnButtonDownEvent();
    }

    public void CheckoutSuccess(SocketIO.AnomaliJSON.Response.GameInfo infoCashout)
    {
        isGameStart = false;

        autoCashOut.SetActive(false);

        currentMultiplier = (float) infoCashout.data.range;
        try
        {
            textMultiplier.text = currentMultiplier.ToString("F2") + "x";
            textMultiplier.transform.GetChild(0).GetComponent<TMP_Text>().text = textMultiplier.text;
        } catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

        currentRewardCash = (float)infoCashout.data.winAmount;

        string tier = "low";

        if (infoCashout.data.range >= 15)
        {
            if (!string.IsNullOrEmpty(cacheFinalBonus))
            {
                if (cacheFinalBonus == "diamond")
                    tier = "high";
                else if (cacheFinalBonus == "coin" || cacheFinalBonus == "cash")
                    tier = "mid";
            }
        }
        else
            currentFinishMultiplier = 0.0;

        spineCharacter.AnimationState.ClearTrack(0);
        spineCharacter.Skeleton.SetToSetupPose();
        spineCharacter.AnimationState.AddAnimation(0, "reward_" + tier + "_start", false, 0f);
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.youwin, true);

        gameWinLoseEvent?.Invoke(true);

        try
        {
            SoundHandler.Instance.StopOneShotSFX(SoundHandler.SFX_NAME.pushrun);
        }
        catch (System.Exception ex) {
            Debug.Log(ex.Message);
        }
    }

    public void Win()
    {
        isGameStart = false;

        autoCashOut.SetActive(false);

        string tier = "";
        switch (UnityEngine.Random.Range(1, 4))
        {
            case 1:
                tier = "low";
                break;
            case 2:
                tier = "mid";
                break;
            case 3:
                tier = "high";
                break;
        }

        spineCharacter.AnimationState.AddAnimation(0, "reward_" + tier + "_start", false, .1f);
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.youwin, true);

        gameWinLoseEvent?.Invoke(true);
    }

    public bool IsLose(bool isForceLose = false)
    {
        if (!isGameStart) return false;

        if (isForceLose || (DollHandler.Instance.isDollActive && isCharacterMove))
        {
            Lose();

            return true;
        }
        else
            return false;
    }

    public Vector3 GetEndPos()
    {
        return endPosition.position;
    }

    public void ShakeCamera()
    {
        Camera.main.DOShakePosition(.2f, 1, 20, 90, true);
    }

    public void PrintTimer()
    {
        float totalSecond = gameDuration - timer;

        int minutes = Mathf.FloorToInt(totalSecond / 60); // 1 menit = 60 detik
        int secs = Mathf.FloorToInt(totalSecond % 60); // Sisa detik
        int millisecs = Mathf.FloorToInt((totalSecond % 1) * 100); // Sisa detik dikali 100 untuk mendapatkan milidetik

        if (!isUnder10Sec && secs <= 10)
        {
            isUnder10Sec = true;

            textTimer.color = new Color(.98f, .51f, .21f); // orange
            //textTimer.color = new Color(1f, .19f, .19f); // red
            textTimer.DOColor(new Color(1f, .19f, .19f), 1f).SetLoops(-1, LoopType.Yoyo);
        }

        textTimer.text = string.Format("{0:00}:{1:00}", minutes, secs);
        //textTimer.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, secs, millisecs);
    }

    public void PrintTimerBySocket(SocketIO.AnomaliJSON.Response.TimerState output)
    {
        float totalSecond = output.remainingTime;
        if (totalSecond < 0)
            totalSecond = 0;

        int minutes = Mathf.FloorToInt(totalSecond / 60); // 1 menit = 60 detik
        int secs = Mathf.FloorToInt(totalSecond % 60); // Sisa detik
        //int millisecs = Mathf.FloorToInt((totalSecond % 1) * 100); // Sisa detik dikali 100 untuk mendapatkan milidetik

        if (!isUnder10Sec && secs <= 10)
        {
            isUnder10Sec = true;

            textTimer.color = new Color(.98f, .51f, .21f); // orange
            //textTimer.color = new Color(1f, .19f, .19f); // red
            textTimer.DOColor(new Color(1f, .19f, .19f), 1f).SetLoops(-1, LoopType.Yoyo);
        }

        textTimer.text = string.Format("{0:00}:{1:00}", minutes, secs);
        //textTimer.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, secs, millisecs);
    }

    public void TriggerTimeoutEvent()
    {
        Lose();
        Debug.Log("Lose because of time up");

        spineDynamite.AnimationState.SetAnimation(0, "explode", false);
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.explosiontimeup, true);
    }

    public void SpeedUpActive()
    {
        //if (!isGameStart || isPause) return;

        //isRun = true;
        //ButtonRun.Instance.Refresh();

        //float distance = (endPosition.position.z - startPosition.position.z);
        //float currentDistance = (endPosition.position.z - spineCharacter.transform.position.z);
        //float currentDuration = currentDistance * movementDuration / distance;
        //float sprintDuration = currentDuration / 1.5f;

        //spineCharacter.transform.DOKill();
        //spineCharacter.transform.DOMoveZ(endPosition.position.z, sprintDuration).SetEase(Ease.Linear).OnComplete(Win);
        //if(!isCharacterMove)
        //    spineCharacter.transform.DOPause();
    }

    public void BombActive()
    {
        if (!isGameStart || isPause) return;

        //spineCharacter.transform.DOKill();
        //spineCharacter.transform.DOMoveZ(spineCharacter.transform.position.z - 10f, .25f).SetEase(Ease.Linear).OnComplete(() =>
        //{
        //    float distance = (endPosition.position.z - startPosition.position.z);
        //    float currentDistance = (endPosition.position.z - spineCharacter.transform.position.z);
        //    float currentDuration = currentDistance * movementDuration / distance;
        //    float sprintDuration = currentDuration / 1.5f;

        //    spineCharacter.transform.DOKill();
        //    spineCharacter.transform.DOMoveZ(endPosition.position.z, sprintDuration).SetEase(Ease.Linear).OnComplete(Win);
        //    if (!isCharacterMove)
        //        spineCharacter.transform.DOPause();

        //    PrintMultiplier();
        //});
    }
    #endregion

    #region Private Function
    private void Reset()
    {
        isGameStart = false;
        isCharacterMove = false;
        isBonusShow = false;
        isUnder10Sec = false;
        currentProgress = 0f;

        textTimer.DOKill();
        textTimer.color = new Color(1,1,1);
        textTimer.text = "";

        currentTrack = 1;
        currentMultiplier = 0;
        currentRewardCash = 0;
        distanceReached = 0f;
        endPositionZ = endPosition.position.z;
        timer = 0;
        //PrintTimer();
        Debug.Log("Current track: " + currentTrack);

        isRun = false;

        // INIT DUMMY INITIAL BET
        InitialBet initialBet = APIHandler.Instance.initialBet;

        initialBet.multiplier = maximumMultiplier;

        spineCharacter.transform.position = new Vector3(spineCharacter.transform.position.x, spineCharacter.transform.position.y, startPosition.position.z);
        spineCharacter.transform.DOKill();
        spineCharacter.transform.DOMoveZ(endPosition.position.z, movementDuration).SetEase(Ease.Linear);
        spineCharacter.transform.DOPause();
        spineCharacter.transform.DOGoto(movementDuration * currentProgress);
        spineCharacter.AnimationState.AddAnimation(0, "stop_variation_" + UnityEngine.Random.Range(1, 4), false, 0f);
        Debug.Log("Reset Pose");

        textMultiplier.text = "0.00x";
        textMultiplier.transform.GetChild(0).GetComponent<TMP_Text>().text = textMultiplier.text;
        textCashout.text = "0";
    }

    protected Tweener currentProgressTween = null;

    public void CharacterMoveBy(SocketIO.AnomaliJSON.Response.RangeUpdate output)
    {
        isCharacterMove = true;

        //if (IsLose()) return;
        var floatParsed = (float) double.Parse(output.range, CultureInfo.InvariantCulture);
        //currentProgress = floatParsed;
        //spineCharacter.transform.DOGoto(currentProgress);
        if (currentProgressTween != null)
            currentProgressTween.Kill();

        if (isAutoCashout && !doCashout)
        {
            if (floatParsed >= inputAutoCashout)
            {
                ClientSocketIO.Instance.SendEvent("cashOut");
                doCashout = true;
                CharacterStop();
                return;
            }
        }
        currentProgressTween = DOVirtual.Float(currentProgress, floatParsed, 0.5f, (floatUpdate) => {
            spineCharacter.transform.DOGoto(floatUpdate);
            currentProgress = floatUpdate;
            PrintMultiplier();
        }).SetEase(Ease.Linear);
        spineCharacter.AnimationState.AddAnimation(0, "run", true, .5f);
        Debug.Log("Far as: " + floatParsed);
    }

    public void CharacterStopBy()
    {
        isCharacterMove = false;

        //if (currentProgressTween != null)
        //    currentProgressTween.Kill();
        //spineCharacter.transform.DOPause();
        spineCharacter.AnimationState.ClearTrack(0);
        spineCharacter.Skeleton.SetToSetupPose();
        spineCharacter.AnimationState.AddAnimation(0, "stop_variation_" + UnityEngine.Random.Range(1, 4), false, 0f);

        PrintMultiplier();
    }

    private void CharacterMove()
    {
        //isCharacterMove = true;

        //if (IsLose()) return;

        //spineCharacter.transform.DOPlay();
        //spineCharacter.AnimationState.AddAnimation(0, "run", true, .1f);

        ClientSocketIO.Instance.SendEvent("startRunning");
        Debug.Log("Chara move by button run pressed");
    }

    private void CharacterStop()
    {
        ClientSocketIO.Instance.SendEvent("stopRunning");
    }

    public void InstantLose(SocketIO.AnomaliJSON.Response.RangeCrash output)
    {
        if (!isGameStart)
            return;

        isGameStart = false;
        isCharacterMove = false;

        autoCashOut.SetActive(false);

        spineCharacter.transform.DOPause();
        spineCharacter.AnimationState.ClearTrack(0);
        spineCharacter.AnimationState.AddAnimation(0, "shot_start", false, .1f);
        DollHandler.Instance.towerShootEvent?.Invoke();

        SoundHandler.Instance.StopOneShotSFX(SoundHandler.SFX_NAME.pushrun);
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.gameover, true);

        gameWinLoseEvent?.Invoke(false);
        Debug.Log("Instant Lose pose");
    }

    public void Lose()
    {
        if (!isGameStart)
            return;

        isGameStart = false;
        isCharacterMove = false;

        autoCashOut.SetActive(false);

        spineCharacter.transform.DOPause();
        spineCharacter.AnimationState.ClearTrack(0);
        spineCharacter.AnimationState.AddAnimation(0, "shot_start", false, .1f);
        DollHandler.Instance.towerShootEvent?.Invoke();

        SoundHandler.Instance.StopOneShotSFX(SoundHandler.SFX_NAME.pushrun);
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.gameover, true);

        gameWinLoseEvent?.Invoke(false);
    }
    #endregion

    #region Listener
    private IEnumerator DelayFunction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private bool IsDebounce()
    {
        if (isDebounce)
            return true;
        else
        {
            StartCoroutine(DelayFunction(.25f, () =>
            {
                buttonRun.interactable = true;
                isDebounce = false;
            }));

            buttonRun.interactable = false;
            isDebounce = true;

            return false;
        }
    }

    private void OnButtonDownEvent()
    {
        //if (isDebounce) return;
        if (!isGameStart || isPause) return;
        if (isCharacterMove) return;

        SoundHandler.Instance.PlayOneShotSFX(SoundHandler.SFX_NAME.pushrun, true);
        CharacterMove();
    }

    public void OnButtonUpEvent()
    {
        //if (IsDebounce()) return;
        if (!isGameStart || isPause) return;
        if (!isCharacterMove) return;

        SoundHandler.Instance.StopOneShotSFX(SoundHandler.SFX_NAME.pushrun);
        CharacterStop();
    }

    private void OnGamePauseEvent(bool isGamePaused)
    {
        isPause = isGamePaused;

        if (isPause && isCharacterMove)
        {
            SoundHandler.Instance.StopOneShotSFX(SoundHandler.SFX_NAME.pushrun);
            CharacterStop();
        }
    }
    #endregion
}