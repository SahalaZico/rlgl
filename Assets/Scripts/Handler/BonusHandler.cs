using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Globalization;

public class BonusHandler : Singleton<BonusHandler>
{
    [Header("Helicopter")]
    [SerializeField] Transform helicopterPrefab;
    [SerializeField] Transform world3d;

    [Header("Component")]
    [SerializeField] SkeletonGraphic spineBonus;
    [SerializeField] CanvasGroup bonusCanvas = null;

    [Header("Target")]
    [SerializeField] Transform timer;
    [SerializeField] TMP_Text textTimerAdditional;
    [SerializeField] Transform run;
    [SerializeField] Transform win;

    [Header("Bonus Coin")]
    [SerializeField] RectTransform bucketSpawn = null;
    [SerializeField] CanvasGroup bucketCanvas = null;
    [SerializeField] RectTransform coinSpawn = null;
    [SerializeField] CanvasGroup coinCanvas = null;
    [SerializeField] RectTransform cointStart = null;
    [SerializeField] RectTransform coinEnd = null;
    [SerializeField] RectTransform coinGoalBucket = null;
    [SerializeField] AnimationCurve curveGoalBucket = null;
    [SerializeField] AnimationCurve curveGoalBucketUp = null;
    [SerializeField] TMP_Text bucketMultiplier = null;
    protected Sequence doSequenceCoin = null;
    protected float lastMultiplier = 0f;

    SkeletonAnimation spineHelicopter;
    InitialBetBonus betBonus;

    private void Start()
    {
        textTimerAdditional.gameObject.SetActive(false);

        GameplayHandler.Instance.gameStartEvent.AddListener(OnGameStartEvent);
        GameplayHandler.Instance.bonusShowEvent.AddListener(OnBonusShowEvent);
        GameplayHandler.Instance.bonusEvent.AddListener(OnBonusEvent);

        spineBonus.AnimationState.Complete += OnAnimationStateComplete;
    }

    private void TriggerEventDropCoin(bool overrideValue = false)
    {
        //spineBonus.transform.DOScale(Vector3.zero, .5f).OnComplete(() =>
        //{
        //    spineBonus.gameObject.SetActive(false);
        //});

        if (doSequenceCoin != null)
        {
            if (doSequenceCoin.IsActive())
                doSequenceCoin.Kill();
        }

        coinSpawn.localScale = Vector3.one;
        coinSpawn.localPosition = cointStart.localPosition;
        coinCanvas.alpha = 0f;

        doSequenceCoin = DOTween.Sequence();
        doSequenceCoin.Append(coinSpawn.DOLocalMove(coinEnd.localPosition, 0.5f));
        doSequenceCoin.Join(coinCanvas.DOFade(1f, 0.5f));
        doSequenceCoin.Append(bucketSpawn.DOScale(Vector3.one, 0.5f));
        doSequenceCoin.AppendInterval(0.8f);
        doSequenceCoin.Append(coinSpawn.DOLocalMoveX(coinGoalBucket.localPosition.x, 0.5f).SetEase(curveGoalBucket));
        doSequenceCoin.Join(coinSpawn.DOLocalMoveY(coinGoalBucket.localPosition.y, 0.5f).SetEase(curveGoalBucketUp));
        doSequenceCoin.Join(coinSpawn.DOScale(Vector2.zero, 0.5f));
        doSequenceCoin.Append(DOVirtual.Float((!overrideValue) ? lastMultiplier : 0f, (!overrideValue) ? betBonus.value : 5.55f, 0.5f, (floatParsed) => {
            lastMultiplier = floatParsed;
            string result = lastMultiplier.ToString("F2") + "x";
            bucketMultiplier.text = result;
        }));
        //doSequenceCoin.Append()
    }

    private void OnAnimationStateComplete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry == null) return;
        if (trackEntry.ToString() == string.Empty) return;

        switch (trackEntry.ToString())
        {
            case "drop_box":
                spineHelicopter.AnimationState.SetAnimation(0, "box_closed", true);

                break;
            case "box_broken":
                DestroyBox();

                spineBonus.gameObject.SetActive(true);
                switch (betBonus.type)
                {
                    case "money":
                        TriggerEventDropCoin();
                        //doSequenceCoin.Append()
                        break;
                    case "add_time":
                        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.bonustimer, true);

                        spineBonus.transform.DOMove(timer.position, .5f).OnComplete(() =>
                        {
                            textTimerAdditional.text = "+" + betBonus.value + "s";
                            textTimerAdditional.color = new Color(1, 1, 0, 1);
                            textTimerAdditional.transform.localScale = Vector3.zero;
                            textTimerAdditional.transform.position = timer.position;
                            textTimerAdditional.gameObject.SetActive(true);

                            Sequence sequence = DOTween.Sequence();
                            sequence.Insert(0, textTimerAdditional.transform.DOScale(Vector3.one, .5f));
                            sequence.Insert(0, textTimerAdditional.transform.DOLocalMoveY(-0.05f, 2f));
                            sequence.Insert(1, textTimerAdditional.DOColor(new Color(1, 1, 0, 0), 1f));

                            //GameplayHandler.Instance.timer -= 10f;
                            //GameplayHandler.Instance.PrintTimer();

                            spineBonus.gameObject.SetActive(false);
                        });

                        break;
                    case "speed_up":
                        spineBonus.transform.DOMove(run.position, .5f).OnComplete(() =>
                        {
                            GameplayHandler.Instance.SpeedUpActive();

                            spineBonus.gameObject.SetActive(false);
                        });

                        break;
                    case "electrocute":
                        spineBonus.AnimationState.SetAnimation(0, "Electrocute explode", false);

                        break;
                    case "bomb":
                        spineBonus.AnimationState.SetAnimation(0, "bomb explode", false);
                        break;
                }

                break;
            case "Electrocute explode":
                DollHandler.Instance.ElectrocuteActive();

                spineBonus.gameObject.SetActive(false);

                break;
            case "bomb explode":
                SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.bonusexplosion, true);

                GameplayHandler.Instance.BombActive();

                spineBonus.gameObject.SetActive(false);

                break;
        }
    }

    private void DestroyBox()
    {
        Destroy(spineHelicopter.gameObject);
        spineHelicopter = null;
    }

    #region Listener
    private void OnGameStartEvent(bool isGameStart)
    {
        if(spineHelicopter != null)
        {
            Destroy(spineHelicopter.gameObject);
            spineHelicopter = null;
        }

        bucketSpawn.localScale = Vector3.zero;
        bucketCanvas.alpha = 1f;
        lastMultiplier = 0f;
        bucketMultiplier.text = "";
    }

    private void OnBonusShowEvent(Vector3 position)
    {
        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.helicopter, true);

        Transform helicopter = Instantiate(helicopterPrefab, position, Quaternion.identity);
        helicopter.parent = world3d;

        spineHelicopter = helicopter.GetComponent<SkeletonAnimation>();
        spineHelicopter.AnimationState.Complete += OnAnimationStateComplete;

        spineHelicopter.AnimationState.SetAnimation(0, "drop_box", false);
    }

    private void OnBonusEvent(InitialBetBonus betBonus)
    {
        this.betBonus = betBonus;

        SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.boxbreak, true);

        // show text bonus
        spineBonus.gameObject.SetActive(true);
        RectTransform rect = spineBonus.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        spineBonus.transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.Insert(0, rect.DOAnchorPos(new Vector2(0, 50), .3f).SetEase(Ease.InBounce));
        sequence.Insert(0, spineBonus.transform.DOScale(Vector3.one, .5f));

        string bonusType = string.Empty;
        switch (betBonus.type)
        {
            case "money":
                bonusType = "Cash";
                break;
            case "add_time":
                bonusType = "time";
                break;
            case "speed_up":
                SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.bonusspeedup, true);

                bonusType = "speed";
                break;
            case "electrocute":
                SoundHandler.Instance.PlaySfx(SoundHandler.SFX_NAME.bonuselectricbomb, true);

                bonusType = "Electrocute idle";
                break;
            case "bomb":
                bonusType = "bomb idle";
                break;
        }

        if (bonusType == "Cash")
            bonusCanvas.alpha = 0f;
        else
            bonusCanvas.alpha = 1f;
        spineBonus.AnimationState.SetAnimation(0, bonusType, true);
        spineHelicopter.AnimationState.SetAnimation(0, "box_broken", false);
    }
    #endregion

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                TriggerEventDropCoin(true);
            }
        }
    }
#endif
}