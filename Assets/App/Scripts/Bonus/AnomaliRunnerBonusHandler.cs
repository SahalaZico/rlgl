using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnomaliRunnerBonusHandler : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] protected List<AnomaliRunnerBonusPoint> availablePoints = new List<AnomaliRunnerBonusPoint>();

    [Header("Game Props")]
    [SerializeField] protected Transform poolContainer = null;
    [SerializeField] protected Transform rendererContainer = null;
    [SerializeField] protected float spawnBeforeDistance = 1f;

    [SerializeField] UnityEvent<AnomaliRunnerBonusPoint> OnOpenDropBox;

    protected Queue<GameObject> pool = new Queue<GameObject>();
    protected List<GameObject> spawnedObjects = new List<GameObject>();
    protected List<SkeletonAnimation> spawnedSkeletons = new List<SkeletonAnimation>();
    protected Vector3 cacheStart = Vector3.zero;
    protected Vector3 cacheEnd = Vector3.zero;
    protected float cacheMaxRange = 0f;
    protected float currentRange = 0f;

    public void Clear()
    {
        availablePoints.Clear();

        for (int i = 0; i < spawnedObjects.Count; i++) {
            if (spawnedObjects[i] == null)
                continue;

            if (spawnedSkeletons[i] == null)
                continue;

            spawnedObjects[i].transform.parent = poolContainer;
            spawnedObjects[i].transform.localPosition = Vector3.zero;
            spawnedSkeletons[i].gameObject.SetActive(true);
            spawnedObjects[i].SetActive(false);
            pool.Enqueue(spawnedObjects[i]);
        }

        spawnedSkeletons.Clear();
        spawnedObjects.Clear();
    }

    public void AddPoint(AnomaliRunnerBonusPoint newPoint)
    {
        if (availablePoints == null)
            availablePoints = new List<AnomaliRunnerBonusPoint>();

        availablePoints.Add(newPoint);
    }

    public void ConstructByTrack(float maxRange, Vector3 inputStart, Vector3 inputEnd)
    {
        cacheMaxRange = maxRange;
        cacheStart = inputStart;
        cacheEnd = inputEnd;

        //make sure all available points already assign
        for (int i = 0; i < availablePoints.Count; i++) {
            if (availablePoints[i] == null)
                continue;

            GameObject spawnedObject = pool.Dequeue();
            if (spawnedObject == null)
                continue;
            SkeletonAnimation spawnedSkeleton = spawnedObject.GetComponentInChildren<SkeletonAnimation>();
            if (spawnedSkeleton == null)
                continue;
            float maxDistance = Vector3.Distance(inputStart, inputEnd);
            spawnedObject.SetActive(true);
            spawnedObject.transform.parent = rendererContainer;
            spawnedObject.transform.localPosition = Vector3.forward * (maxDistance * (availablePoints[i].range / maxRange));
            spawnedSkeleton.gameObject.SetActive(false);
            spawnedObjects.Add(spawnedObject);
            spawnedSkeletons.Add(spawnedSkeleton);

            availablePoints[i].SpawnAt = availablePoints[i].range - spawnBeforeDistance;
            availablePoints[i].DoSpawn = false;
            availablePoints[i].DoOpen = false;
        }
    }

    public void UpdateRangeByRunner(float inputRange)
    {
        currentRange = inputRange;

        for (int i = 0; i < spawnedObjects.Count; i++) {
            if (spawnedObjects[i] == null)
                continue;

            if (availablePoints[i] == null)
                continue;

            if (availablePoints[i].DoSpawn)
                continue;

            if (currentRange >= availablePoints[i].SpawnAt)
            {
                spawnedSkeletons[i].gameObject.SetActive(true);
                spawnedSkeletons[i].AnimationState.SetAnimation(0, "drop_box", false);

                availablePoints[i].DoSpawn = true;
            }
        }

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] == null)
                continue;

            if (availablePoints[i] == null)
                continue;

            if (availablePoints[i].DoOpen)
                continue;

            if (currentRange >= availablePoints[i].range)
            {
                AnomaliRunnerBonusPoint getPointClass = availablePoints[i];
                spawnedSkeletons[i].gameObject.SetActive(true);
                spawnedSkeletons[i].AnimationState.SetAnimation(0, "box_broken", false);
                OnOpenDropBox?.Invoke(getPointClass);

                availablePoints[i].DoOpen = true;
            }
        }
    }

    protected void OnCompleteInstanceAnimation(TrackEntry entryAcc)
    {
        if (entryAcc == null)
            return;

        if (entryAcc.Animation == null)
            return;

        switch (entryAcc.Animation.Name)
        {
            case "drop_box":
                //AudioManager.Instance.Play("sfx", "heli");
                break;
            default:
                break;
        }
    }

    protected void OnStartInstanceAnimation(TrackEntry entryAcc)
    {
        if (entryAcc == null)
            return;

        if (entryAcc.Animation == null)
            return;

        switch (entryAcc.Animation.Name)
        {
            case "drop_box":
                AudioManager.Instance.Play("sfx", "heli");
                break;
            case "box_broken":
                AudioManager.Instance.Play("sfx", "boxbrake");
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < poolContainer.childCount; i++)
        {
            SkeletonAnimation getSkeleton = poolContainer.GetChild(i).gameObject.GetComponentInChildren<SkeletonAnimation>();
            if (getSkeleton != null)
            {
                getSkeleton.AnimationState.Complete -= OnCompleteInstanceAnimation;
                getSkeleton.AnimationState.Start -= OnStartInstanceAnimation;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < poolContainer.childCount; i++)
        {
            pool.Enqueue(poolContainer.GetChild(i).gameObject);
            SkeletonAnimation getSkeleton = poolContainer.GetChild(i).gameObject.GetComponentInChildren<SkeletonAnimation>();
            if (getSkeleton != null)
            {
                getSkeleton.AnimationState.Complete += OnCompleteInstanceAnimation;
                getSkeleton.AnimationState.Start += OnStartInstanceAnimation;
            }
        }

        Debug.Log("Pool size: " + pool.Count);
    }
}
