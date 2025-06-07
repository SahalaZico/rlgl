using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using DG.Tweening;
using Newtonsoft.Json;
using Spine;

public class UIGameplay : UIPage
{
    protected static UIGameplay instance = null;
    public static UIGameplay Instance
    {
        get
        {
            return instance;
        }
    }

    [Header("GameManager")]
    [SerializeField] protected AnomaliRunnerGameplay gameManager = null;

    [Header("UI Info")]
    [SerializeField] protected RectTransform rectPanelTop = null;
    [SerializeField] protected TMP_Text textBalance = null;
    [SerializeField] protected TMP_Text textTimer = null;
    [SerializeField] protected SkeletonGraphic skeleTimer = null;
    protected DG.Tweening.Sequence panelTopSeq = null;

    [Header("UI Submenu")]
    [SerializeField] protected RectTransform rectSubmenu = null;
    [SerializeField] protected float maxSlideClose = 550f;
    protected bool doSubmenuOpen = false;
    protected DG.Tweening.Sequence submenuSeq = null;

    [Header("Bet Menu")]
    [SerializeField] protected RectTransform rectBet = null;
    [SerializeField] protected CanvasGroup canvasBetMenu = null;
    [SerializeField] protected TMP_InputField inputBet = null;
    [SerializeField] protected TMP_InputField inputStopMultiplier = null;
    [SerializeField] protected TMP_InputField inputNumRound = null;
    [SerializeField] protected UIButtonToggle toggleAutoplay = null;
    [SerializeField] protected Button btnConfirmBet = null;
    [SerializeField] protected CanvasGroup canvasAutoplaySettings = null;
    [SerializeField] protected CanvasGroup canvasInputMultiplier = null;
    [SerializeField] protected CanvasGroup canvasInputRound = null;
    protected bool doAutoPlay = false;
    public bool DoAutoPlay
    {
        get
        {
            return doAutoPlay;
        }
    }
    protected DG.Tweening.Sequence autoDelaySeq = null;
    protected DG.Tweening.Sequence mainBetSeq = null;

    [Header("InRun Control or Menu")]
    [SerializeField] protected RectTransform rectInrun = null;
    [SerializeField] protected CanvasGroup canvasInrun = null;
    [SerializeField] protected CanvasGroup canvasInrunMultiplier = null;
    [SerializeField] protected RectTransform rectInrunMultiplier = null;
    [SerializeField] protected TMP_Text textMultiplierInrun = null;
    [SerializeField] protected TMP_Text textshaMultiplierInrun = null;
    [SerializeField] protected TMP_Text textInrunCashout = null;
    protected DG.Tweening.Sequence mainInrunSeq = null;

    [Header("InRun Bonus")]
    [SerializeField] protected RectTransform rectBonus = null;
    [SerializeField] protected SkeletonGraphic skeleBonus = null;
    [SerializeField] protected RectTransform rectBonusStart = null;
    [SerializeField] protected RectTransform rectBonusUp = null;
    protected DG.Tweening.Sequence bonusInrunSeq = null;

    [Header("InRun Bonus - Coin")]
    [SerializeField] protected RectTransform rectRendererCoin = null;
    [SerializeField] protected RectTransform rectBonusCoin = null;
    [SerializeField] protected CanvasGroup canvasBonusCoin = null;
    [SerializeField] protected SkeletonGraphic skeleBonusCoin = null;
    [SerializeField] protected TMP_Text textBonusCoin = null;
    protected Tweener calculateCoinMultiply = null;

    [Header("InRun Bonus - Time")]
    [SerializeField] protected RectTransform rectRendererTime = null;
    [SerializeField] protected RectTransform rectStartTime = null;
    [SerializeField] protected RectTransform rectEndTime = null;
    [SerializeField] protected TMP_Text textBonusTime = null;

    [Header("UIPanel Lose")]
    [SerializeField] protected RectTransform rectPanelLose = null;
    [SerializeField] protected SkeletonGraphic skelePanelLose = null;

    [Header("UIPanel Win")]
    [SerializeField] protected RectTransform rectPanelWin = null;
    [SerializeField] protected SkeletonGraphic skelePanelWin = null;
    [SerializeField] protected TMP_Text textWinAmount = null;
    [SerializeField] protected TMP_Text textshaWinAmount = null;
    protected Tweener tweenWinAmount = null;
    protected DG.Tweening.Sequence panelResultSeq = null;
    protected DG.Tweening.Sequence panelVictorySeq = null;

    protected bool isPanelTopActive = false;
    protected bool isInrunActive = false;
    protected bool isBetActive = false;
    protected bool isPanelLoseActive = false;
    protected bool isPanelWinActive = false;

    protected bool isUnder10Sec = false;
    protected int currentRound = 0;
    protected int selectedIndexChip = 0;
    protected double selectedStopMultiplier = 0.5;
    protected int selectedNumRound = 1;
    protected UserDataResponse cacheUserData = null;
    protected List<long> availableChips = new List<long>();

    protected string cacheFinalBonus = "";
    protected double currentFinishMultiplier = 0.0;

    List<string> cacheBonus = new List<string>();
    List<double> cacheBonusPosition = new List<double>();
    List<double> cacheBonusValue = new List<double>();

    #region BASE-UI-LOGIC
    public void Clear()
    {
        SetActivePanelTop(true);
        SetActiveBet(true);
        SetActiveInrun(false);
        SetActivePanelLose(false);
        SetActivePanelWin(false);

        textMultiplierInrun.text = "";
        textshaMultiplierInrun.text = "";
        canvasInrunMultiplier.alpha = 0f;
        rectInrunMultiplier.anchoredPosition = new Vector3(0f, -455f);

        //btnConfirmBet.interactable = true;
        //canvasBetMenu.interactable = true;
        gameManager.BonusHandler.Clear();

        if (calculateCoinMultiply != null)
            calculateCoinMultiply.Kill();
        calculateCoinMultiply = null;

        textBonusCoin.text = "";
        rectBonusCoin.localScale = Vector3.zero;
        rectBonusCoin.anchoredPosition = new Vector3(-376f, 368f);
        rectRendererCoin.gameObject.SetActive(false);
        canvasBonusCoin.alpha = 0f;

        textTimer.text = "";
        skeleTimer.AnimationState.SetAnimation(0, "idle", true);

        currentRound++;
        if (doAutoPlay)
        {
            if (currentRound >= selectedNumRound)
            {
                //stop autoplay
                btnConfirmBet.interactable = true;
                canvasBetMenu.interactable = true;
                canvasAutoplaySettings.interactable = true;
                currentRound = 0;
                inputNumRound.text = StringUtility.ConvertDoubleToString(selectedNumRound);
            }
            else
            {
                //continue autoplay but with delay
                canvasBetMenu.interactable = true;
                canvasAutoplaySettings.interactable = false;
                if (autoDelaySeq != null)
                {
                    if (autoDelaySeq.IsActive())
                        autoDelaySeq.Kill();
                }

                autoDelaySeq = DOTween.Sequence();
                autoDelaySeq.AppendInterval(1.8f);
                autoDelaySeq.AppendCallback(() => {
                    OnClickConfirmBet();
                });

                inputNumRound.text = StringUtility.ConvertDoubleToString(selectedNumRound - currentRound);
            }
        } else
        {
            btnConfirmBet.interactable = true;
            canvasBetMenu.interactable = true;
            canvasAutoplaySettings.interactable = true;
            currentRound = 0;
        }
        Debug.Log(string.Format("Current round {0} from {1} round(s)", currentRound, selectedNumRound));
    }

    public string GetCurrency()
    {
        if (cacheUserData == null)
            return "usd";

        if (cacheUserData.data == null)
            return "usd";

        if (cacheUserData.data.player == null)
            return "usd";

        if (string.IsNullOrEmpty(cacheUserData.data.player.player_currency))
            return "usd";

        return cacheUserData.data.player.player_currency.ToLower();
    }

    public double GetSelectedStopMultiplier()
    {
        return selectedStopMultiplier;
    }
    public long GetUserBalance()
    {
        return cacheUserData.data.player.player_balance;
    }

    public double GetLimitBet()
    {
        return cacheUserData.data.game.limit_bet.maximal;
    }

    public long[] GetChips()
    {
        return cacheUserData.data.game.chip_base;
    }

    public long GetSelectedChipAmount()
    {
        return availableChips[selectedIndexChip];
    }

    public void OnClickNumRoundPrev()
    {
        selectedNumRound--;
        if (selectedNumRound < 1)
            selectedNumRound = 1;

        UpdateNumRound();
        AudioManager.Instance.Play("sfx", "click");
    }

    public void OnClickNumRoundNext()
    {
        selectedNumRound++;

        UpdateNumRound();
        AudioManager.Instance.Play("sfx", "click");
    }

    public void OnClickStopMultiPrev()
    {
        selectedStopMultiplier-=0.5;
        if (selectedStopMultiplier < 0.5)
            selectedStopMultiplier = 0.5;

        UpdateSelectedStopMultiplier();
        AudioManager.Instance.Play("sfx", "click");
    }

    public void OnClickStopMultiNext()
    {
        selectedStopMultiplier += 0.5;
        if (selectedStopMultiplier >= 100.0)
            selectedStopMultiplier = 100.0 - 1.0;

        UpdateSelectedStopMultiplier();
        AudioManager.Instance.Play("sfx", "click");
    }

    public void OnClickChipPrev()
    {
        selectedIndexChip--;
        if (selectedIndexChip < 0)
            selectedIndexChip = 0;

        UpdateSelectedChip();
        AudioManager.Instance.Play("sfx", "click");
    }

    public void OnClickChipNext()
    {
        selectedIndexChip++;
        if (selectedIndexChip >= availableChips.Count)
            selectedIndexChip = availableChips.Count - 1;

        UpdateSelectedChip();
        AudioManager.Instance.Play("sfx", "click");
    }

    protected void UpdateNumRound()
    {
        inputNumRound.text = StringUtility.ConvertDoubleToString(selectedNumRound);
    }

    protected void UpdateSelectedStopMultiplier()
    {
        inputStopMultiplier.text = StringUtility.ConvertDoubleToStringWithDecimal(selectedStopMultiplier);
    }

    protected void UpdateSelectedChip()
    {
        long chip = availableChips[selectedIndexChip];
        inputBet.text = GetCurrency().ToUpper() + " " + StringUtility.ConvertDoubleToString(chip, GetCurrency());
    }

    public void UpdateBalanceByAmount(long amount)
    {
        cacheUserData.data.player.player_balance = amount;
        UpdateBalance();
    }

    protected void UpdateBalance()
    {
        textBalance.text = string.Format("{0}: {1} {2}", LocalizationManager.Instance.GetVirtualTranslateText("balance"), 
            GetCurrency().ToUpper(), 
            StringUtility.ConvertDoubleToString(GetUserBalance(), GetCurrency()));
    }

    public void UpdateTextMultiplierInrunByAmount(double input)
    {
        textshaMultiplierInrun.text = StringUtility.ConvertDoubleToStringWithDecimal(input, GetCurrency()) +"x";
        textMultiplierInrun.text = StringUtility.ConvertDoubleToStringWithDecimal(input, GetCurrency()) +"x";
    }

    public void UpdateTextCashoutInrunByAmount(double inputMultiplier)
    {
        double actualCash = inputMultiplier * GetSelectedChipAmount();
        textInrunCashout.text = string.Format("{0} {1}", GetCurrency().ToUpper(), StringUtility.ConvertDoubleToString(actualCash, GetCurrency()));
    }

    public virtual void OnReceivePlayerData(UserDataResponse userData)
    {
        cacheUserData = userData;

        selectedNumRound = 1;
        selectedStopMultiplier = 0.5;

        long[] chips = GetChips();
        double limitBet = GetLimitBet();
        availableChips = new List<long>();

        for (int i = 0; i < chips.Length; i++) {
            if (chips[i] <= limitBet)
                availableChips.Add(chips[i]);
        }

        UpdateSelectedChip();
        UpdateSelectedStopMultiplier();
        UpdateNumRound();
        UpdateBalance();
    }

    public void OnClickSubmenu()
    {
        doSubmenuOpen = !doSubmenuOpen;

        if (submenuSeq != null)
        {
            if (submenuSeq.IsActive())
                submenuSeq.Kill();
        }

        submenuSeq = DOTween.Sequence();
        submenuSeq.Append(rectSubmenu.DOAnchorPos((doSubmenuOpen) ? Vector2.zero : (Vector2.up * maxSlideClose), 0.5f));
        AudioManager.Instance.Play("sfx", "click");
    }

    protected void OnToggleAutoplay(bool state)
    {
        doAutoPlay = state;
        switch (doAutoPlay)
        {
            case true:
                canvasInputMultiplier.alpha = 1f;
                canvasInputMultiplier.interactable = true;
                canvasInputMultiplier.blocksRaycasts = true;
                canvasInputRound.alpha = 1f;
                canvasInputRound.interactable = true;
                canvasInputRound.blocksRaycasts = true;
                break;
            default:
                if (autoDelaySeq != null)
                {
                    if (autoDelaySeq.IsActive())
                    {
                        btnConfirmBet.interactable = true;
                        canvasBetMenu.interactable = true;
                        canvasAutoplaySettings.interactable = true;
                        currentRound = 0;
                        autoDelaySeq.Kill();
                        autoDelaySeq = null;
                    }
                }
                canvasInputMultiplier.alpha = 0.25f;
                canvasInputMultiplier.interactable = false;
                canvasInputMultiplier.blocksRaycasts = false;
                canvasInputRound.alpha = 0.25f;
                canvasInputRound.interactable = false;
                canvasInputRound.blocksRaycasts = false;
                break;
        }

        AudioManager.Instance.Play("sfx", "click");
    }

    public void SetActiveBet(bool state)
    {
        isBetActive = state;

        if (mainBetSeq != null)
        {
            if (mainBetSeq.IsActive())
                mainBetSeq.Kill();

            mainBetSeq = null;
        }

        mainBetSeq = DOTween.Sequence();
        mainBetSeq.Append(rectBet.DOAnchorPos(isBetActive ? Vector2.zero : (Vector2.up * -1050.0f), 0.5f));
    }

    public void SetActiveInrun(bool state)
    {
        isInrunActive = state;

        if (mainInrunSeq != null)
        {
            if (mainInrunSeq.IsActive())
                mainInrunSeq.Kill();

            mainInrunSeq = null;
        }

        mainInrunSeq = DOTween.Sequence();
        mainInrunSeq.Append(rectInrun.DOAnchorPos(isInrunActive ? Vector2.zero : (Vector2.up * -1050.0f), 0.3f));
    }

    public void SetActivePanelTop(bool state)
    {
        isPanelTopActive = state;

        if (panelTopSeq != null)
        {
            if (panelTopSeq.IsActive())
                panelTopSeq.Kill();

            panelTopSeq = null;
        }

        panelTopSeq = DOTween.Sequence();
        panelTopSeq.Append(rectPanelTop.DOAnchorPos(isPanelTopActive ? (Vector2.up * -15.0f) : (Vector2.up * 550.0f), 0.35f));
    }

    public void SetActivePanelWin(bool state, double winAmount = 0f, float delay = 0f, float delayToCallback = 0f, System.Action onReturnCallback = null)
    {
        isPanelWinActive = state;
        if (isPanelWinActive)
            skelePanelWin.gameObject.SetActive(true);

        if (panelVictorySeq != null) {
            if (panelVictorySeq.IsActive())
                panelVictorySeq.Kill();

            panelVictorySeq = null;
        }

        if (textWinAmount != null)
            textWinAmount.text = "";

        if (textshaWinAmount != null)
            textshaWinAmount.text = "";

        panelVictorySeq = DOTween.Sequence();
        panelVictorySeq.AppendInterval(delay);
        if (isPanelWinActive)
        {
            panelVictorySeq.Append(rectInrunMultiplier.DOAnchorPos(Vector3.zero, 0.65f));
            panelVictorySeq.Join(canvasInrunMultiplier.DOFade(0f, 0.65f));
        }
        if (rectRendererCoin.gameObject.activeSelf && isPanelWinActive)
        {
            panelVictorySeq.Append(rectBonusCoin.DOAnchorPos(Vector3.zero, 0.65f));
            panelVictorySeq.Join(canvasBonusCoin.DOFade(0f, 0.65f));
        }
        panelVictorySeq.Append(rectPanelWin.DOAnchorPos(isPanelWinActive ? (Vector2.up * -660f) : (Vector2.up * 450.0f), 0.25f));
        panelVictorySeq.AppendCallback(() => {
            if (!isPanelWinActive)
                skelePanelWin.gameObject.SetActive(false);
            else
            {
                if (tweenWinAmount != null)
                    tweenWinAmount.Kill();

                tweenWinAmount = DOVirtual.Float(0f, (float) winAmount, 0.35f, (floatUpdate) =>
                {
                    if (textWinAmount != null)
                        textWinAmount.text = string.Format("{0} {1}", GetCurrency().ToUpper(), StringUtility.ConvertDoubleToString(floatUpdate, GetCurrency())); ;
                    if (textshaWinAmount != null)
                        textshaWinAmount.text = textWinAmount.text;
                }).OnComplete(() => {
                    long currentBalance = GetUserBalance();
                    currentBalance = currentBalance + (long)winAmount;
                    UpdateBalanceByAmount(currentBalance);
                });
            }
        });
        panelVictorySeq.AppendInterval(delayToCallback);
        panelVictorySeq.AppendCallback(() => {
            onReturnCallback?.Invoke();
        });
        if (isPanelWinActive)
        {
            skelePanelWin.AnimationState.SetAnimation(0, "spawn", false);
            AudioManager.Instance.Play("sfx", "win");
        }
    }

    public void SetActivePanelLose(bool state, float delay = 0f, float delayToCallback = 0f, System.Action onReturnCallback = null)
    {
        isPanelLoseActive = state;
        if (isPanelLoseActive)
        {
            skelePanelLose.gameObject.SetActive(true);

            rectRendererCoin.gameObject.SetActive(false);
            rectBonusCoin.localScale = Vector3.zero;
        }
        if (panelResultSeq != null)
        {
            if (panelResultSeq.IsActive())
                panelResultSeq.Kill();

            panelResultSeq = null;
        }

        panelResultSeq = DOTween.Sequence();
        panelResultSeq.AppendInterval(delay);
        panelResultSeq.Append(rectPanelLose.DOAnchorPos(isPanelLoseActive ? (Vector2.up * -550f) : (Vector2.up * 350.0f), 0.25f));
        panelResultSeq.AppendCallback(() => {
            if (!isPanelLoseActive)
                skelePanelLose.gameObject.SetActive(false);
        });
        panelResultSeq.AppendInterval(delayToCallback);
        panelResultSeq.AppendCallback(() => {
            onReturnCallback?.Invoke();
        });
        if (isPanelLoseActive)
            skelePanelLose.AnimationState.SetAnimation(0, "spawn", false);
    }

    public void ShowWinOrLosePanel(bool doWin, double winAmount = 0.0)
    {
        SetActivePanelTop(false);
        SetActiveInrun(false);
        switch (doWin)
        {
            case true:
                SetActivePanelWin(true, winAmount, 2f, 3f, () => {
                    gameManager.Clear();
                    Clear();
                });
                break;
            case false:
                SetActivePanelLose(true, 2f, 3f, () => {
                    gameManager.Clear();
                    Clear();
                });
                break;
        }
    }

    public void OnClickConfirmBet()
    {
        btnConfirmBet.interactable = false;
        canvasBetMenu.interactable = false;

        var body = new SocketIO.AnomaliJSON.Body.Bet
        {
            button_bet = new SocketIO.AnomaliJSON.Body.ButtonBet
            {
                amount = (double) GetSelectedChipAmount()
            }
        };

        ClientSocketIO.Instance.SendEvent("sendBet", body);

        ToastHandler.Instance.Show("Please wait...", 8.0f);

        AudioManager.Instance.Play("sfx", "click");
    }

    public void OnClickCashout()
    {
        if (!gameManager.GameStart)
            return;

        ClientSocketIO.Instance.SendEvent("cashOut");
        canvasInrun.interactable = false;

        AudioManager.Instance.Play("sfx", "click");
        ToastHandler.Instance.Show("Please Wait...", 8f);
    }

    protected void AddBonusByRule(string catchItem, double catchPosition, object value)
    {
        if (value == null)
            return;

        if (value is string)
        {
            var fetchValue = value.ToString();
            if (string.IsNullOrEmpty(fetchValue))
            {
                return;
            }
            else
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

            if (value is double || value is int)
            {
                cacheBonusValue.Add((double)value);
            }
            else
            {
                cacheBonusValue.Add(0.0);
            }
        }
    }

    public void RenderBonusByPoint(AnomaliRunnerBonusPoint fetchPoint)
    {
        string getKeyBonus = fetchPoint.key;
        string keyAnimation = "";
        switch (getKeyBonus) {
            case "add_time":
                keyAnimation = "time";
                break;
            case "money":
                keyAnimation = "Cash";
                break;
            case "bomb":
                keyAnimation = "bomb idle";
                break;
            case "speed_up":
                keyAnimation = "speed";
                break;
            case "electrocute":
                keyAnimation = "Electrocute idle";
                break;
            default:
                keyAnimation = "time";
                break;
        }

        skeleBonus.gameObject.SetActive(true);
        TrackEntry availableTrack = skeleBonus.AnimationState.SetAnimation(0, keyAnimation, true);
        if (availableTrack != null) {
            availableTrack.MixDuration = 0;
        }
        if (bonusInrunSeq != null)
        {
            if (bonusInrunSeq.IsActive())
            {
                bonusInrunSeq.Kill();
            }
        }

        rectBonus.DOKill();
        rectBonus.localScale = Vector3.zero;
        rectBonus.anchoredPosition = rectBonusStart.anchoredPosition;
        bonusInrunSeq = DOTween.Sequence();
        bonusInrunSeq.Append(rectBonus.DOScale(Vector3.one, 0.5f));

        if (getKeyBonus == "money")
        {
            rectRendererCoin.gameObject.SetActive(true);
            rectBonusCoin.localScale = Vector3.zero;
            textBonusCoin.text = "";
            bonusInrunSeq.AppendInterval(1f);
            bonusInrunSeq.AppendCallback(() => {
                skeleBonusCoin.AnimationState.SetAnimation(0, "animation", true);
            });
            bonusInrunSeq.Append(rectBonusCoin.DOScale(Vector3.one, 0.35f));
            bonusInrunSeq.Append(rectBonus.DOAnchorPos(rectBonusCoin.anchoredPosition, 0.35f));
            bonusInrunSeq.Join(rectBonus.DOScale(Vector3.one * 1.5f, 0.15f).OnComplete(() =>
            {
                rectBonus.DOScale(Vector3.zero, 0.15f);
                if (calculateCoinMultiply != null)
                    calculateCoinMultiply.Kill();
                calculateCoinMultiply = DOVirtual.Float(0f, fetchPoint.baseValue, 0.5f, (floatUpdate) => {
                    textBonusCoin.text = StringUtility.ConvertDoubleToStringWithDecimal(floatUpdate, GetCurrency()) + "x";
                });
            }));
        } 
        else if (getKeyBonus == "add_time")
        {
            textBonusTime.DOKill();
            rectRendererTime.DOKill();

            rectRendererTime.gameObject.SetActive(true);
            rectRendererTime.anchoredPosition = rectStartTime.anchoredPosition;
            textBonusTime.color = Color.green;
            textBonusTime.text = string.Format("+{0}s", StringUtility.ConvertDoubleToString(fetchPoint.baseValue, GetCurrency()));
            
            rectRendererTime.DOAnchorPos(rectEndTime.anchoredPosition, 1.2f).OnComplete(() => {
                textBonusTime.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    rectRendererTime.gameObject.SetActive(false);
                });
            });
            bonusInrunSeq.AppendInterval(2f);
            bonusInrunSeq.Append(rectBonus.DOAnchorPos(rectBonusUp.anchoredPosition, 0.35f));
            bonusInrunSeq.Join(rectBonus.DOScale(Vector3.one * 1.5f, 0.15f).OnComplete(() =>
            {
                rectBonus.DOScale(Vector3.zero, 0.15f);
            }));
        }
        else
        {
            bonusInrunSeq.AppendInterval(2f);
            bonusInrunSeq.Append(rectBonus.DOAnchorPos(rectBonusUp.anchoredPosition, 0.35f));
            bonusInrunSeq.Join(rectBonus.DOScale(Vector3.one * 1.5f, 0.15f).OnComplete(() =>
            {
                rectBonus.DOScale(Vector3.zero, 0.15f);
            }));
        }
    }
    #endregion

    #region RESPONSE-BY-SOCKET
    public void OnGetRangeData(SocketIO.AnomaliJSON.Response.RangeData.Root config)
    {
        gameManager.SetMaxRange((float) config.baseMaxRange);
    }
    public void OnConfirmBetSuccess(SocketIO.AnomaliJSON.Response.GameInfo gameInfo)
    {
        SetActiveBet(false);
        SetActiveInrun(true);
        ToastHandler.Instance.Close();
        canvasInrun.interactable = true;
        canvasInrunMultiplier.alpha = 1f;
        rectInrunMultiplier.anchoredPosition = new Vector3(0f, -455f);

        cacheBonus = new List<string>();
        cacheBonusPosition = new List<double>();
        cacheBonusValue = new List<double>();

        canvasBonusCoin.alpha = 1f;
        rectBonusCoin.anchoredPosition = new Vector3(-376f, 368f);

        var drops = gameInfo.data.drops;

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

        for (int i = 0; i < cacheBonus.Count; i++)
        {
            string type = cacheBonus[i];
            float positionRange = (float)cacheBonusPosition[i];
            float baseValue = (float)cacheBonusValue[i];
            gameManager.BonusHandler.AddPoint(new AnomaliRunnerBonusPoint
            {
                key = type,
                range = positionRange,
                baseValue = baseValue,
            });
        }

        gameManager.SetFinishData(currentFinishMultiplier, cacheFinalBonus);
        gameManager.GameStart = true; //before this make sure assign data such as bonus event...

        long selectedChip = GetSelectedChipAmount();
        long currentBalance = GetUserBalance();
        currentBalance = currentBalance - selectedChip;
        UpdateBalanceByAmount(currentBalance);

        if (doAutoPlay)
            canvasAutoplaySettings.interactable = false;

        AudioManager.Instance.Play("sfx", "gamestart");

        string jsonResponse = JsonConvert.SerializeObject(gameInfo);
        Debug.Log("Bet Confirmed: " + jsonResponse);
    }

    public void OnTimerUpdate(SocketIO.AnomaliJSON.Response.TimerState output)
    {
        float totalSecond = output.remainingTime;
        if (totalSecond < 0)
        {
            totalSecond = 0;
        }

        int minutes = Mathf.FloorToInt(totalSecond / 60); // 1 menit = 60 detik
        int secs = Mathf.FloorToInt(totalSecond % 60); // Sisa detik

        if (!isUnder10Sec && secs <= 10)
        {
            isUnder10Sec = true;

            textTimer.color = new Color(.98f, .51f, .21f);
            textTimer.DOColor(new Color(1f, .19f, .19f), 1f).SetLoops(-1, LoopType.Yoyo);
        }

        textTimer.text = string.Format("{0:00}:{1:00}", minutes, secs);
    }

    public void OnTimeUp()
    {
        gameManager.PlayDefeat();
        skeleTimer.AnimationState.SetAnimation(0, "explode", false);
    }
    #endregion

    public override void SetActive(bool state)
    {
        base.SetActive(state);
        if (state)
        {
            isBetActive = false;
            rectBet.anchoredPosition = Vector2.up * -1050f;
            isInrunActive = false;
            rectInrun.anchoredPosition = Vector2.up * -1050f;
            isPanelTopActive = false;
            rectPanelTop.anchoredPosition = Vector2.up * 550f;
            isPanelLoseActive = false;
            rectPanelLose.anchoredPosition = Vector2.up * 350f;
            isPanelWinActive = false;
            rectPanelWin.anchoredPosition = Vector2.up * 450f;
            textMultiplierInrun.text = "";
            textshaMultiplierInrun.text = "";
            canvasInrunMultiplier.alpha = 0f;

            rectInrunMultiplier.anchoredPosition = new Vector3(0f, -455f);
            rectBonusCoin.anchoredPosition = new Vector3(-376f, 368f);

            SetActivePanelTop(true);
            SetActiveBet(true);
            SetActiveInrun(false);
            SetActivePanelLose(false);
            SetActivePanelWin(false);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        UIGameplay.instance = this;
    }

    protected override void Start()
    {
        base.Start();

        toggleAutoplay.OnStateChanged += this.OnToggleAutoplay;
        rectSubmenu.anchoredPosition = Vector2.up * maxSlideClose;

        btnConfirmBet.interactable = true;
        canvasBetMenu.interactable = true;

        skelePanelLose.AnimationState.Complete += (trackEntry) =>
        {
            if (trackEntry == null)
                return;

            if (trackEntry.Animation == null)
                return;

            switch (trackEntry.Animation.Name) {
                case "spawn":
                    skelePanelLose.AnimationState.SetAnimation(0, "idle", true);
                    break;
            }
        };

        skelePanelWin.AnimationState.Complete += (trackEntry) =>
        {
            if (trackEntry == null)
                return;

            if (trackEntry.Animation == null)
                return;

            switch (trackEntry.Animation.Name)
            {
                case "spawn":
                    skelePanelWin.AnimationState.SetAnimation(0, "idle", true);
                    break;
            }
        };
    }

    protected void OnDestroy()
    {
        //agent.OnStopAtGridIndex -= OnStopAtIndex;
        toggleAutoplay.OnStateChanged -= this.OnToggleAutoplay;
    }
}
