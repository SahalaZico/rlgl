using DG.Tweening;
using Spine.Unity;
using UnityEngine;

[System.Serializable]
public class AnomaliRunnerNPC
{
    public enum ActionState
    {
        None = 0,
        Idle = 1,
        Run = 2,
        Finish = 3,
        Dead = 4
    }

    [SerializeField] protected Transform transformRunner = null;
    public Transform TransformRunner
    {
        get
        {
            return transformRunner;
        }
    }

    [SerializeField] protected SkeletonAnimation skeleRunner = null;
    public SkeletonAnimation SkeleRunner
    {
        get
        {
            return skeleRunner;
        }
    }

    protected float currentRange = 0f;
    protected int currentSpeed = 1;
    protected float maximumRange = 1f;

    protected ActionState currentState = ActionState.None;
    public ActionState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    public AnomaliRunnerNPC()
    {
        currentState = ActionState.None;
    }

    public void Init()
    {
        skeleRunner.AnimationState.Complete += (track) =>
        {
            if (track == null)
                return;

            if (track.Animation == null)
                return;

            switch (track.Animation.Name)
            {
                case "stop_variation_1":
                    if (CurrentState == ActionState.Dead)
                        return;
                    skeleRunner.AnimationState.SetAnimation(0, "stop_variation_1_loop", true);
                    break;
                case "stop_variation_2":
                    if (CurrentState == ActionState.Dead)
                        return;
                    skeleRunner.AnimationState.SetAnimation(0, "stop_variation_2_loop", true);
                    break;
                case "stop_variation_3":
                    if (CurrentState == ActionState.Dead)
                        return;
                    skeleRunner.AnimationState.SetAnimation(0, "stop_variation_3_loop", true);
                    break;
                case "reward_low_start":
                    Spine.TrackEntry entryRewardLow = skeleRunner.AnimationState.SetAnimation(0, "reward_low_idle", true);
                    entryRewardLow.MixDuration = 0f;
                    break;
                case "reward_mid_start":
                    Spine.TrackEntry entryRewardMid = skeleRunner.AnimationState.SetAnimation(0, "reward_mid_idle", true);
                    entryRewardMid.MixDuration = 0f;
                    break;
                case "reward_high_start":
                    Spine.TrackEntry entryRewardHigh = skeleRunner.AnimationState.SetAnimation(0, "reward_high_idle", true);
                    entryRewardHigh.MixDuration = 0f;
                    break;
                case "shot_start":
                    Spine.TrackEntry entryShot = skeleRunner.AnimationState.SetAnimation(0, "shot_idle", false);
                    entryShot.MixDuration = 0f;
                    break;
            }
        };
    }

    public void Clear()
    {
        currentRange = 0f;
        transformRunner.DOKill();
        var fetchPosition = transformRunner.position;
        fetchPosition.z = 0f;
        transformRunner.position = fetchPosition;
        currentState = ActionState.None;
        SetState(ActionState.Idle);
    }

    public void ConstructTrackAndAvatar(Vector3 startPosition, Vector3 endPosition, float maxRange)
    {
        currentRange = 0f;
        maximumRange = maxRange;
        var fetchPosition = transformRunner.position;
        fetchPosition.z = startPosition.z;
        transformRunner.position = fetchPosition;
        transformRunner.DOKill();
        transformRunner.DOMoveZ(endPosition.z, maximumRange).SetEase(Ease.Linear);
        transformRunner.DOPause();
        transformRunner.DOGoto(currentRange * maximumRange);
        SetState(ActionState.None);
    }

    public void SetState(ActionState state) {
        if (currentState == ActionState.Finish || currentState == ActionState.Dead)
            return;
        if (currentState == state)
            return;

        switch (state) { 
            case ActionState.Idle:
                int indexRandom = Random.Range(0, 3) + 1;
                skeleRunner.AnimationState.SetAnimation(0, "stop_variation_" + indexRandom, false);
                currentState = state;
                break;
            case ActionState.Run:
                currentSpeed = Random.Range(5, 8);
                skeleRunner.AnimationState.SetAnimation(0, "run", true);
                currentState = state;
                break;
            case ActionState.Finish:
                string[] tiers = new string[] { "low", "mid", "high" };
                skeleRunner.AnimationState.SetAnimation(0, "reward_" + tiers[Random.Range(0, tiers.Length)] + "_start", false);
                currentState = state;
                break;
            case ActionState.Dead:
                skeleRunner.AnimationState.SetAnimation(0, "shot_start", false);
                currentState = state;
                break;
            default:
                break;
        }
    }

    public void UpdateRange(float range)
    {
        if (currentState != ActionState.Run)
            return;

        currentRange = currentRange + (range * (0.1f * currentSpeed));
        if (currentRange > maximumRange)
        {
            SetState(ActionState.Finish);
            currentRange = maximumRange;
        }
        transformRunner.DOGoto(currentRange);
    }
}
