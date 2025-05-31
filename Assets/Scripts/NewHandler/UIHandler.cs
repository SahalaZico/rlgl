using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] GameObject bet;
    [SerializeField] GameObject timer;
    [SerializeField] TMP_Text textBalance;
    [SerializeField] Button buttonStart;
    [SerializeField] Button buttonMinus;
    [SerializeField] Button buttonPlus;
    [SerializeField] Button buttonMinusCash;
    [SerializeField] Button buttonPlusCash;
    [SerializeField] Button buttonMinusRound;
    [SerializeField] Button buttonPlusRound;
    [SerializeField] TMP_InputField inputFieldBet;
    [SerializeField] TMP_InputField inputFieldCashout;
    [SerializeField] TMP_InputField inputFieldRound;
    [SerializeField] Button buttonAutoCashout;
    [SerializeField] Button buttonCashout;
    [SerializeField] Button buttonRun;
    [SerializeField] GameObject coinAbsorb;
    [SerializeField] TMP_Text textMultiplier;
    [SerializeField] TMP_Text textTimer;
    [SerializeField] SkeletonGraphic spineYouWin;
    [SerializeField] TMP_Text textReward;
    [SerializeField] SkeletonGraphic spineYouLose;
    [SerializeField] GameObject autoCashOut;
    [SerializeField] Button buttonCancelAuto;

    [Header("Finish Result")]
    [SerializeField] TMP_Text textResultRace = null;
    [SerializeField] TMP_Text textResultFinish = null;
    [SerializeField] RectTransform rectBucket = null;
    [SerializeField] CanvasGroup canvasBucket = null;
    [SerializeField] RectTransform rectMerge = null;

    protected Vector3 textResultRaceInitPos = Vector3.zero;
    protected Vector3 textResultFinishInitPos = Vector3.zero;
    protected Vector3 rectBucketInitPos = Vector3.zero;

    List<long> chips = new List<long> { 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000 };

    bool isAutoPlay = false;
    float timerAutoPlay = 0;

    public void SetInteractableBtnStart(bool state)
    {
        buttonStart.interactable = state;
    }

    public void CheckoutSuccess(SocketIO.AnomaliJSON.Response.GameInfo infoCashout)
    {
        if (infoCashout.data == null)
            return;

        SetBalance(infoCashout.data.balance);
    }

    public void OnBetConfirmedBySocket(SocketIO.AnomaliJSON.Response.GameInfo output)
    {
        if (output.data == null)
            return;

        SetBalance(output.data.balance);
    }

    public void SetBalance(double valueBalance)
    {
        var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };
        string result = valueBalance.ToString("#,##0", nfi);
        textBalance.text = "Balance: " + result;
    }

    public void FetcheUserInfo(UserDataResponse response)
    {
        UserDataResponse userDataResponse = response;

        var maximal_bet = userDataResponse.data.game.limit_bet.maximal;
        var internal_chips = userDataResponse.data.game.chip_base;
        var target_chips = new List<long>();
        for (int i = 0; i < internal_chips.Length; i++)
        {
            if (internal_chips[i] <= maximal_bet)
                target_chips.Add(internal_chips[i]);
        }

        chips = target_chips;
        double playerBalance = userDataResponse.data.player.player_balance;
        var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };
        string result = playerBalance.ToString("#,##0", nfi);
        textBalance.text = "Balance: " + result;
        GameplayHandler.Instance.inputBet = chips[0];
        string strresult = GameplayHandler.Instance.inputBet.ToString("#,##0", nfi);
        inputFieldBet.text = strresult;
    }

    void Start()
    {
        bet.SetActive(false);
        timer.SetActive(false);
        buttonCashout.gameObject.SetActive(false);
        autoCashOut.SetActive(false);
        buttonRun.gameObject.SetActive(false);
        coinAbsorb.SetActive(false);
        textMultiplier.gameObject.SetActive(false);
        textTimer.gameObject.SetActive(false);

        buttonStart.interactable = false;
        buttonAutoCashout.transform.Find("Off").gameObject.SetActive(!GameplayHandler.Instance.isAutoCashout);
        buttonAutoCashout.transform.Find("On").gameObject.SetActive(GameplayHandler.Instance.isAutoCashout);

        spineYouWin.gameObject.SetActive(false);
        spineYouLose.gameObject.SetActive(false);

        buttonStart.onClick.AddListener(OnButtonStart);
        buttonMinus.onClick.AddListener(OnButtonMinus);
        buttonPlus.onClick.AddListener(OnButtonPlus);
        buttonMinusCash.onClick.AddListener(OnButtonMinusCash);
        buttonPlusCash.onClick.AddListener(OnButtonPlusCash);
        buttonMinusRound.onClick.AddListener(OnButtonMinusRound);
        buttonPlusRound.onClick.AddListener(OnButtonPlusRound);
        inputFieldCashout.onEndEdit.AddListener(OnInputFieldCashoutEndEdit);
        inputFieldRound.onEndEdit.AddListener(OnInputFieldRoundEndEdit);
        buttonAutoCashout.onClick.AddListener(OnButtonAutoCashout);
        buttonCashout.onClick.AddListener(OnButtonCashout);
        buttonCancelAuto.onClick.AddListener(OnButtonCancelAuto);

        GameplayHandler.Instance.gameStartEvent.AddListener(OnGameStartEvent);
        GameplayHandler.Instance.gameWinLoseEvent.AddListener(OnGameWinLoseEvent);

        spineYouWin.AnimationState.Complete += OnAnimationStateComplete;
        spineYouLose.AnimationState.Complete += OnAnimationStateComplete;

        rectBucketInitPos = rectBucket.localPosition;
        textResultFinishInitPos = textResultFinish.transform.localPosition;
        textResultRaceInitPos = textResultFinish.transform.localPosition;
    }

    private void Update()
    {
        if (isAutoPlay)
        {
            timerAutoPlay -= Time.deltaTime;
            if(timerAutoPlay <= 0f)
            {
                isAutoPlay = false;
                OnButtonStart();
            }
        }
    }

    private void OnAnimationStateComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry == null) return;
        if (trackEntry.ToString() == string.Empty) return;

        switch (trackEntry.ToString())
        {
            case "spawn":
                spineYouLose.AnimationState.SetAnimation(0, "idle", true);
                spineYouLose.AnimationState.SetAnimation(0, "idle", true);
                break;
        }
    }

    #region Private Function
    private IEnumerator DelayFunction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private void Reset()
    {
        bet.SetActive(true);
        timer.SetActive(true);
        buttonCashout.gameObject.SetActive(false);
        buttonRun.gameObject.SetActive(false);
        coinAbsorb.SetActive(false);
        textMultiplier.gameObject.SetActive(false);
        textTimer.gameObject.SetActive(false);

        spineYouWin.gameObject.SetActive(false);
        spineYouLose.gameObject.SetActive(false);

        //UserDataResponse userDataResponse = APIHandler.Instance.userDataResponse;
        //long playerBalance = userDataResponse.data.player.player_balance;
        //var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };
        //string result = playerBalance.ToString("#,##0", nfi);
        //textBalance.text = "Balance: " + result;

        buttonMinus.interactable = true;
        buttonPlus.interactable = true;
        buttonStart.interactable = (GameplayHandler.Instance.inputBet > 0);
        inputFieldBet.interactable = true;
        inputFieldCashout.interactable = true;
        inputFieldRound.interactable = true;

        textResultFinish.text = "";
        textResultRace.text = "";
        textResultFinish.transform.localPosition = textResultFinishInitPos;
        textResultRace.transform.localPosition = textResultRaceInitPos;
        rectBucket.localPosition = rectBucketInitPos;
        canvasBucket.alpha = 1f;
        textResultFinish.alpha = 1f;
        textResultRace.alpha = 1f;
    }

    private void GameStart()
    {
        bet.SetActive(false);
        buttonCashout.gameObject.SetActive(true);
        buttonRun.gameObject.SetActive(true);
        coinAbsorb.SetActive(false);
        textMultiplier.gameObject.SetActive(true);
        textTimer.gameObject.SetActive(true);

        spineYouWin.gameObject.SetActive(false);
        spineYouLose.gameObject.SetActive(false);

        textResultFinish.text = "";
        textResultRace.text = "";
        textResultFinish.transform.localPosition = textResultFinishInitPos;
        textResultRace.transform.localPosition = textResultRaceInitPos;
        rectBucket.localPosition = rectBucketInitPos;
        canvasBucket.alpha = 1f;
        textResultFinish.alpha = 1f;
        textResultRace.alpha = 1f;
    }

    private void GameWin()
    {
        Debug.Log("You win..");

        timer.SetActive(false);

        coinAbsorb.SetActive(false);
        textMultiplier.gameObject.SetActive(false);
        buttonCashout.gameObject.SetActive(false);
        buttonRun.gameObject.SetActive(false);

        textResultFinish.transform.localScale = Vector3.zero;
        textResultRace.text = textMultiplier.text;
        textResultFinish.text = (GameplayHandler.Instance.currentFinishMultiplier > 0) ? GameplayHandler.Instance.currentFinishMultiplier.ToString("F2") + "x" : "";

        textReward.transform.localScale = Vector3.zero;
        textReward.text = "0";
        textReward.transform.GetChild(0).GetComponent<TMP_Text>().text = textReward.text;

        float multiplier = GameplayHandler.Instance.currentMultiplier;
        //multiplier = (float) Math.Round(multiplier, 2);
        long reward = (long) (GameplayHandler.Instance.currentRewardCash);

        var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };

        Sequence openSequence = DOTween.Sequence();
        openSequence.Append(textResultRace.transform.DOLocalMove(rectMerge.localPosition, 1f));
        openSequence.Append(textResultRace.DOFade(0f, 0.5f));
        openSequence.Append(textResultFinish.transform.DOScale(Vector3.one, 1f));
        openSequence.Append(textResultFinish.transform.DOLocalMove(rectMerge.localPosition, 1f));
        openSequence.Join(rectBucket.transform.DOLocalMove(rectMerge.localPosition, 1f));
        openSequence.Append(textResultFinish.DOFade(0f, 0.5f));
        openSequence.Join(canvasBucket.DOFade(0f, 0.5f));
        openSequence.AppendCallback(() => {

            spineYouWin.gameObject.SetActive(true);
            spineYouWin.AnimationState.SetAnimation(0, "spawn", false);

            Sequence winSequence = DOTween.Sequence();
            winSequence.Insert(1.4f, textReward.transform.DOScale(new Vector3(1, 1, 1), .2f));
            winSequence.Insert(1.7f, textReward.transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1), .4f, 0, 0));
            winSequence.Insert(1.7f, DOTween.To(() => 0, randomValue =>
            {
                string result = randomValue.ToString("#,##0", nfi);
                textReward.text = result;
                textReward.transform.GetChild(0).GetComponent<TMP_Text>().text = textReward.text;
            }, reward, 1).SetEase(Ease.Linear));
            //winSequence.Insert(2, textReward.transform.DOScale(new Vector3(1, 1, 1), 1));
            winSequence.OnComplete(() =>
            {
                StartCoroutine(DelayFunction(4, GameplayHandler.Instance.GameReset));
            });
        });
    }

    private void GameLose()
    {
        Debug.Log("You lose..");

        buttonCashout.gameObject.SetActive(false);
        buttonRun.gameObject.SetActive(false);

        StartCoroutine(DelayFunction(2, () =>
        {
            timer.SetActive(false);
            coinAbsorb.SetActive(false);
            textMultiplier.gameObject.SetActive(false);
        }));
        StartCoroutine(DelayFunction(3, () =>
        {
            spineYouLose.gameObject.SetActive(true);
            spineYouLose.AnimationState.SetAnimation(0, "spawn", false);
        }));
        StartCoroutine(DelayFunction(5, GameplayHandler.Instance.GameReset));
    }

    private void RefreshBalance()
    {
        long inputBet = GameplayHandler.Instance.inputBet;

        var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };
        string result = inputBet.ToString("#,##0", nfi);
        inputFieldBet.text = result;
    }
    #endregion

    #region Listener
    private void OnButtonStart() //emit to socket
    {
        if (GameplayHandler.Instance.GetSendingBet())
            return;

        var body = new SocketIO.AnomaliJSON.Body.Bet
        {
            button_bet = new SocketIO.AnomaliJSON.Body.ButtonBet
            {
                amount = GameplayHandler.Instance.inputBet
            }
        };

        ClientSocketIO.Instance.SendEvent("sendBet", body);

        GameplayHandler.Instance.SetSendingBet(true);
        buttonStart.interactable = false;
        //GameplayHandler.Instance.GameStart();
    }

    private void OnButtonMinusCash()
    {
        var latestCashout = GameplayHandler.Instance.inputAutoCashout;
        latestCashout -= 0.5;
        if (latestCashout < 0.5)
            latestCashout = 0.5;

        GameplayHandler.Instance.inputAutoCashout = latestCashout;
        inputFieldCashout.text = latestCashout.ToString("F1");
    }

    private void OnButtonPlusCash() {
        var latestCashout = GameplayHandler.Instance.inputAutoCashout;
        latestCashout += 0.5;
        if (latestCashout >= GameplayHandler.Instance.movementDuration)
            latestCashout = GameplayHandler.Instance.movementDuration - 0.5;

        GameplayHandler.Instance.inputAutoCashout = latestCashout;
        inputFieldCashout.text = latestCashout.ToString("F1");
    }

    private void OnButtonMinusRound()
    {
        var latestRound = GameplayHandler.Instance.roundAutoCashout;
        latestRound--;
        if (latestRound < 1)
            latestRound = 1;

        GameplayHandler.Instance.roundAutoCashout = latestRound;
        inputFieldRound.text = latestRound.ToString();
    }

    private void OnButtonPlusRound()
    {
        var latestRound = GameplayHandler.Instance.roundAutoCashout;
        latestRound++;

        GameplayHandler.Instance.roundAutoCashout = latestRound;
        inputFieldRound.text = latestRound.ToString();
    }

    private void OnButtonMinus()
    {
        long inputBet = GameplayHandler.Instance.inputBet;

        for (int i = chips.Count - 1; i >= 0; i--)
        {
            if (chips[i] < inputBet)
            {
                inputBet = chips[i];
                GameplayHandler.Instance.inputBet = inputBet;
                break;
            }
        }

        buttonStart.interactable = (inputBet > 0);

        RefreshBalance();
    }

    private void OnButtonPlus()
    {
        long inputBet = GameplayHandler.Instance.inputBet;

        for(int i=0; i < chips.Count; i++)
        {
            if (inputBet < chips[i])
            {
                inputBet = chips[i];
                GameplayHandler.Instance.inputBet = inputBet;
                break;
            }
        }

        buttonStart.interactable = (inputBet > 0);

        RefreshBalance();
    }

    private void OnButtonAutoCashout()
    {
        GameplayHandler.Instance.isAutoCashout = !GameplayHandler.Instance.isAutoCashout;
        GameplayHandler.Instance.roundAutoCashout = int.Parse(inputFieldRound.text);

        buttonAutoCashout.transform.Find("Off").gameObject.SetActive(!GameplayHandler.Instance.isAutoCashout);
        buttonAutoCashout.transform.Find("On").gameObject.SetActive(GameplayHandler.Instance.isAutoCashout);

        if (!GameplayHandler.Instance.isAutoCashout)
        {
            isAutoPlay = false;

            buttonMinus.interactable = true;
            buttonPlus.interactable = true;
            buttonStart.interactable = (GameplayHandler.Instance.inputBet > 0);
            inputFieldBet.interactable = true;
            inputFieldCashout.interactable = true;
            inputFieldRound.interactable = true;
        }
    }

    private void OnInputFieldCashoutEndEdit(string text)
    {
        double inputAutoCashout = Math.Round(float.Parse(text), 1);

        if (inputAutoCashout < 1.0d)
            inputAutoCashout = 1.0d;

        GameplayHandler.Instance.inputAutoCashout = inputAutoCashout;
        inputFieldCashout.text = inputAutoCashout.ToString("F1");
    }

    private void OnInputFieldRoundEndEdit(string text)
    {
        int inputRound = int.Parse(text);

        if (inputRound < 1)
            inputRound = 1;

        GameplayHandler.Instance.roundAutoCashout = inputRound;
    }

    private void OnButtonCashout()
    {
        //GameplayHandler.Instance.Win();
        ClientSocketIO.Instance.SendEvent("cashOut");
    }

    private void OnButtonCancelAuto()
    {
        if (!GameplayHandler.Instance.isAutoCashout)
            return;

        GameplayHandler.Instance.isAutoCashout = false;
        GameplayHandler.Instance.roundAutoCashout = int.Parse(inputFieldRound.text);

        buttonAutoCashout.transform.Find("Off").gameObject.SetActive(!GameplayHandler.Instance.isAutoCashout);
        buttonAutoCashout.transform.Find("On").gameObject.SetActive(GameplayHandler.Instance.isAutoCashout);

        GameplayHandler.Instance.OnButtonUpEvent();

        autoCashOut.SetActive(false);
    }

    private void OnGameStartEvent(bool isGameStart)
    {
        if (isGameStart)
        {
            GameStart();
            if (GameplayHandler.Instance.isAutoCashout)
            {
                GameplayHandler.Instance.roundAutoCashout -= 1;
                if (GameplayHandler.Instance.roundAutoCashout <= 0)
                    GameplayHandler.Instance.roundAutoCashout = 0;
                inputFieldRound.text = "" + GameplayHandler.Instance.roundAutoCashout;
            }
        }
        else
        {
            Reset();

            if(GameplayHandler.Instance.isAutoCashout)
            {
                if(GameplayHandler.Instance.roundAutoCashout <= 0)
                {
                    GameplayHandler.Instance.roundAutoCashout = 0;
                    GameplayHandler.Instance.isAutoCashout = false;
                    inputFieldRound.text = GameplayHandler.Instance.roundAutoCashout.ToString();
                    //GameplayHandler.Instance.roundAutoCashout = int.Parse(inputFieldRound.text);

                    buttonAutoCashout.transform.Find("Off").gameObject.SetActive(!GameplayHandler.Instance.isAutoCashout);
                    buttonAutoCashout.transform.Find("On").gameObject.SetActive(GameplayHandler.Instance.isAutoCashout);

                    return;
                }

                buttonMinus.interactable = false;
                buttonPlus.interactable = false;
                buttonStart.interactable = false;
                inputFieldBet.interactable = false;
                inputFieldCashout.interactable = false;
                inputFieldRound.interactable = false;

                isAutoPlay = true;
                timerAutoPlay = 3.0f;
            }
        }
    }

    private void OnGameWinLoseEvent(bool isWin)
    {
        if (isWin)
            GameWin();
        else
            GameLose();
    }
    #endregion
}