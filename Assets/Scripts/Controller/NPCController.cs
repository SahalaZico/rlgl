using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.TextCore.Text;
using Spine.Unity;

public class NPCController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;

    SkeletonAnimation spineCharacter;
    float speed, lifeDuration, timeLifeDuration = 0, timeScale;
    bool isAlive = true;
    bool isPause = false;
    bool isWin = false;
    bool forceStopSfx = false;

    Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        GameplayHandler.Instance.gameStartEvent.AddListener(OnGameStartEvent);
        GameplayHandler.Instance.dollTurnAroundEvent.AddListener(StopOrResumeMovement);
        GameplayHandler.Instance.gameWinLoseEvent.AddListener(ResetNPC);
        GameplayHandler.Instance.gamePauseEvent.AddListener(OnGamePauseEvent);

        spineCharacter = transform.GetComponent<SkeletonAnimation>();
        spineCharacter.AnimationState.Complete += OnAnimationStateComplete;
    }

    private void Update()
    {
        if(isAlive && !isPause)
        {
            timeLifeDuration += Time.deltaTime;

            if (timeLifeDuration >= lifeDuration)
                isAlive = false;
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
                spineCharacter.AnimationState.SetAnimation(0, "reward_low_idle", true);
                break;
            case "reward_mid_start":
                spineCharacter.AnimationState.SetAnimation(0, "reward_mid_idle", true);
                break;
            case "reward_high_start":
                spineCharacter.AnimationState.SetAnimation(0, "reward_high_idle", true);
                break;
            case "shot_start":
                spineCharacter.AnimationState.SetAnimation(0, "shot_idle", false);
                break;
        }
    }

    private void Reset()
    {
        isAlive = true;
        isWin = false;
        forceStopSfx = false;

        spineCharacter.Skeleton.A = 1;
        speed = Random.Range(minSpeed, maxSpeed);
        lifeDuration = Random.Range(speed-15, speed-5);

        spineCharacter.AnimationState.AddAnimation(0, "stop_variation_" + Random.Range(1, 4), false, .1f);
        System.Random rnd = new System.Random();
        timeScale = rnd.Next(80, 111) * .01f;
        spineCharacter.timeScale = timeScale;

        transform.position = startPos;
        transform.DOKill();
        transform.DOMoveZ(GameplayHandler.Instance.GetEndPos().z, speed).SetEase(Ease.Linear).OnComplete(() => {
            isWin = true;

            string tier = "";
            switch (Random.Range(1, 4))
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
        });
        transform.DOPause();
    }

    void ResetNPC(bool isWin)
    {
        forceStopSfx = true;

        StopOrResumeMovement(true);

        InitLifeDuration();
        StartCoroutine(ResetAfterDelay());
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        Reset();
    }

    public void OnGameStartEvent(bool isStart)
    {
        if (isStart)
        {
            Reset();
            InitLifeDuration();
            StartRun();
            transform.DOPlay();
        }
    }

    public void StopOrResumeMovement(bool isActive)
    {
        if (isWin) return;

        if (isAlive)
        {
            if (isActive)
            {
                StopRun();
                transform.DOPause();
            }
            else
            {
                StartRun();
                transform.DOPlay();
            }
        }
        else
        {
            StartCoroutine(CharacterIsDeadIE());
        }
    }

    IEnumerator CharacterIsDeadIE()
    {
        yield return new WaitForSeconds(.1f);

        if (spineCharacter.AnimationName != "shot_idle")
        {
            spineCharacter.AnimationState.AddAnimation(0, "shot_start", false, .1f);

            if (!forceStopSfx)
                DollHandler.Instance.towerShootEvent?.Invoke();

            transform.DOKill();
            yield return new WaitForSeconds(1.5f);
            //transform.position = startPos;
            //spineCharacter.Skeleton.A = 0;
        }
    }

    void InitLifeDuration()
    {
        timeLifeDuration = 0;
        isAlive = true;
    }

    void StartRun()
    {
        spineCharacter.AnimationState.SetAnimation(0, "run", true);
    }
    
    void StopRun()
    {
        spineCharacter.AnimationState.SetAnimation(0, "stop_variation_" + Random.Range(1, 4), false);
    }

    private void OnGamePauseEvent(bool isGamePaused)
    {
        isPause = isGamePaused;

        spineCharacter.timeScale = isPause ? 0 : timeScale;

        if (isPause)
            transform.DOPause();
        else
            transform.DOPlay();
    }
}
