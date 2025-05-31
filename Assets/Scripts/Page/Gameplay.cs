using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Globalization;
using System;

public class Gameplay : Singleton<Gameplay>
{
    [Header("UI")]
    [SerializeField] GameObject parent;
    [SerializeField] Transform mainCamera;
    [SerializeField] Transform uiYouWin;
    [SerializeField] TMP_Text textYouWin;
    [SerializeField] Transform uiYouLose;
    [SerializeField] TMP_Text textYouLose;
    [SerializeField] TMP_Text textReward;

    [Header("TweenRecord")]
    [SerializeField] Transform player;
    [SerializeField] Transform start;
    [SerializeField] Transform end;

    [Header("Sprite")]
    [SerializeField] Sprite spriteDollInactive;
    [SerializeField] Sprite spriteDollActive;
    [SerializeField] Sprite[] spriteStop;
    [SerializeField] Sprite spriteRun;
    [SerializeField] Sprite[] spriteWin;
    [SerializeField] Sprite spriteLose;

    [Header("Doll")]
    [SerializeField] SpriteRenderer spriteRendererDoll;
    [SerializeField] float[] dollInactiveMin, dollInactiveMax;
    [SerializeField] float[] dollActiveMin, dollActiveMax;

    [Header("Button")]
    [SerializeField] GameObject buttonRun;

    bool isGameplay = false;
    bool isRun = false;
    bool isPlayerMove = false;
    bool isDollActive = false;
    int currentArea = 1;

    private void Awake()
    {
        parent.SetActive(false);

        StatusHandler.Instance.statusChangedEvent.AddListener(OnStatusChanged);
        PlatformHandler.Instance.platformEndEvent.AddListener(OnPlatformEnd);
        PlatformHandler.Instance.reachAreaEvent.AddListener(ReachArea);

        ButtonRun.Instance.buttonDownEvent.AddListener(OnButtonDownEvent);
        ButtonRun.Instance.buttonUpEvent.AddListener(OnButtonUpEvent);

        spriteRendererDoll.color = Color.white;
        spriteRendererDoll.DOColor(Color.red, .2f).SetLoops(-1, LoopType.Yoyo);
        spriteRendererDoll.DOPause();
    }

    private void Update()
    {
        if (isGameplay)
        {

        }
    }

    #region Public Function
    public void SetSpritePlayer(bool isRun)
    {
        if (isRun)
            player.GetComponent<SpriteRenderer>().sprite = spriteRun;
        else
            player.GetComponent<SpriteRenderer>().sprite = spriteStop[UnityEngine.Random.Range(0, spriteStop.Length)];
    }

    public void ShakeCamera()
    {
        mainCamera.DOShakePosition(.3f);
    }
    #endregion

    #region Private Function
    void RefreshOnBet()
    {
        parent.SetActive(true);
        start.gameObject.SetActive(false);
        end.gameObject.SetActive(false);
        uiYouWin.gameObject.SetActive(false);
        uiYouLose.gameObject.SetActive(false);
        buttonRun.gameObject.SetActive(false);

        BetHandler.Instance.Show();

        isGameplay = false;
        isRun = false;
        isPlayerMove = false;

        player.position = start.position;
        player.localScale = start.localScale;
        SetSpritePlayer(false);

        PlatformHandler.Instance.RefreshPlatform();
    }

    void RefreshOnGameplay()
    {
        isGameplay = true;
        isRun = false;
        isPlayerMove = false;
        isDollActive = false;

        buttonRun.gameObject.SetActive(true);

        StartCoroutine(ToggleDollStatus());
        RefreshDoll();

        PlatformHandler.Instance.StartPlatform();
    }

    private IEnumerator ToggleDollStatus()
    {
        while (isGameplay)
        {
            float interval = isDollActive
                ? UnityEngine.Random.Range(dollInactiveMin[currentArea - 1], dollInactiveMax[currentArea - 1])
                : UnityEngine.Random.Range(dollActiveMin[currentArea - 1], dollActiveMax[currentArea - 1]);

            StartCoroutine(ToggleDollColor(interval));
            yield return new WaitForSeconds(interval);

            if (isGameplay)
            {
                isDollActive = !isDollActive;

                if (isDollActive)
                    ShakeCamera();

                RefreshDoll();

                StartCoroutine(DelayFunction(.3f, ()=> IsPlayerLose()));
            }
        }
    }

    private IEnumerator ToggleDollColor(float interval)
    {
        float timeChangeColor = .6f;

        yield return new WaitForSeconds(interval - timeChangeColor);

        spriteRendererDoll.DOPlay();

        yield return new WaitForSeconds(timeChangeColor);

        spriteRendererDoll.color = Color.white;
        spriteRendererDoll.DOPause();
    }

    private void RefreshDoll()
    {
        spriteRendererDoll.sprite = (isDollActive) ? spriteDollActive : spriteDollInactive;
    }

    private IEnumerator DelayFunction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        action?.Invoke();
    }

    private bool IsPlayerLose()
    {
        if (!isGameplay) return false;
        if (!isDollActive) return false;
        if (!isRun) return false;

        if (isPlayerMove)
            player.DOPause();
        else
            PlatformHandler.Instance.SetMove(false);

        isGameplay = false;
        isRun = false;
        isPlayerMove = false;

        buttonRun.gameObject.SetActive(false);
        player.GetComponent<SpriteRenderer>().sprite = spriteLose;

        uiYouLose.gameObject.SetActive(true);
        textYouLose.transform.localScale = Vector3.zero;

        Sequence winSequence = DOTween.Sequence();
        winSequence.Insert(.4f, textYouLose.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), .2f));

        StartCoroutine(DelayFunction(3f, () =>
        {
            StatusHandler.Instance.ChangeStatus(StatusHandler.STATUS.OnBet, true);
        }));

        return true;
    }

    void RefreshOnResult()
    {
        isGameplay = false;
        isRun = false;
        isPlayerMove = false;

        buttonRun.gameObject.SetActive(false);

        player.GetComponent<SpriteRenderer>().sprite = spriteWin[UnityEngine.Random.Range(0, spriteWin.Length)];

        uiYouWin.gameObject.SetActive(true);
        textYouWin.transform.localScale = Vector3.zero;
        textReward.transform.localScale = Vector3.zero;
        textReward.text = "0";

        var nfi = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };

        Sequence winSequence = DOTween.Sequence();
        winSequence.Insert(.4f, textYouWin.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), .2f));
        winSequence.Insert(.4f, textReward.transform.DOScale(new Vector3(1, 1, 1), .2f));
        winSequence.Insert(.7f, textReward.transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1), .4f, 0, 0));
        winSequence.Insert(.7f, DOTween.To(() => 0, randomValue =>
        {
            string result = randomValue.ToString("#,##0", nfi);
            textReward.text = result;
        }, 50000, 1).SetEase(Ease.Linear));
        winSequence.Insert(2, textReward.transform.DOScale(new Vector3(1, 1, 1), 1));
        winSequence.OnComplete(()=>
        {
            StatusHandler.Instance.ChangeStatus(StatusHandler.STATUS.OnBet, true);
        });
    }
    #endregion

    #region Listener
    private void ReachArea(int area)
    {
        Debug.Log("REACH AREA: " + area);
        currentArea = area;
    }

    private void OnStatusChanged(StatusHandler.STATUS status)
    {
        switch (status)
        {
            case StatusHandler.STATUS.OnBet:
                RefreshOnBet();
                break;
            case StatusHandler.STATUS.OnGameplay:
                RefreshOnGameplay();
                break;
            case StatusHandler.STATUS.OnResult:
                RefreshOnResult();
                break;
        }
    }

    private void OnPlatformEnd()
    {
        isPlayerMove = true;

        player.localScale = start.localScale;
        player.position = start.position;

        player.DOScale(end.localScale, 3);
        player.DOLocalMove(end.position, 3).SetEase(Ease.Linear).OnComplete(() =>
        {
            StatusHandler.Instance.ChangeStatus(StatusHandler.STATUS.OnResult, true);
        });
    }

    private void OnButtonDownEvent()
    {
        if (!isGameplay) return;

        isRun = true;

        if (IsPlayerLose()) return;

        if (isPlayerMove)
        {
            player.DOPlay();
            SetSpritePlayer(true);
        }
    }

    private void OnButtonUpEvent()
    {
        if (!isGameplay) return;

        isRun = false;

        if (isPlayerMove)
        {
            player.DOPause();
            SetSpritePlayer(false);
        }
    }
    #endregion
}