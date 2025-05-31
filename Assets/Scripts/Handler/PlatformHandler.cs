using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class PlatformHandler : Singleton<PlatformHandler>
{
    [Header("Setting")]
    [SerializeField] Transform trackPrefab;
    [SerializeField] Transform trackDetector;
    [SerializeField] Transform trackFinishPrefab;
    [SerializeField] Transform trackParent;
    [SerializeField] Transform doll;
    [SerializeField] Transform player;
    [SerializeField] Color[] trackColors;
    [SerializeField] float trackLength;
    [SerializeField] float trackTime;
    [SerializeField] float trackYOffset;

    [Header("Bush")]
    [SerializeField] Transform dollPoint;
    [SerializeField] Transform[] insidePrefab;
    [SerializeField] Transform[] outsidePrefab;
    [SerializeField] List<BushPoint> bushLeftPoints;
    [SerializeField] List<BushPoint> bushMiddlePoints;
    [SerializeField] List<BushPoint> bushRightPoints;
    [SerializeField] float bushDuration;

    [Header("Event")]
    [SerializeField] public UnityEvent platformEndEvent;
    [SerializeField] public UnityEvent<int> reachAreaEvent;

    Vector3 defaultTrackParentPosition;
    Vector3 defaultDollPosition, defaultDollScale;

    bool isPlatformMove = false;
    List<Bush> listLeftBush, listMiddleBush, listRightBush;
    List<Transform> listDOTweenBush, listDetector;
    int bushLeftIndex = 0, bushMiddleIndex = 0, bushRightIndex = 0, detectorIndex = 0;
    int middleIndexPrefab = 0;

    [Serializable]
    public class Bush
    {
        public Transform bush;
        public BushPoint bushPoint;
    }
    
    [Serializable]
    public class BushPoint
    {
        public Transform start;
        public Transform end;
    }


    private void Awake()
    {
        defaultTrackParentPosition = trackParent.position;
        defaultDollPosition = doll.position;
        defaultDollScale = doll.localScale;

        ButtonRun.Instance.buttonDownEvent.AddListener(OnButtonDownEvent);
        ButtonRun.Instance.buttonUpEvent.AddListener(OnButtonUpEvent);
    }

    private void Update()
    {
        if (isPlatformMove)
        {
            bool isLeftBushExist = (bushLeftIndex < (listLeftBush.Count - 1));
            if (isLeftBushExist && listLeftBush[bushLeftIndex].bush.position.y <= dollPoint.position.y)
            {
                Bush bush = listLeftBush[bushLeftIndex];
                RegisterBush(bush);

                bushLeftIndex++;
            }

            bool isMiddleBushExist = (bushMiddleIndex < (listMiddleBush.Count - 1));
            if (isMiddleBushExist && listMiddleBush[bushMiddleIndex].bush.position.y <= dollPoint.position.y)
            {
                Bush bush = listMiddleBush[bushMiddleIndex];
                RegisterBush(bush);

                bushMiddleIndex++;
            }

            bool isRightBushExist = (bushRightIndex < (listRightBush.Count - 1));
            if (isRightBushExist && listRightBush[bushRightIndex].bush.position.y <= dollPoint.position.y)
            {
                Bush bush = listRightBush[bushRightIndex];
                RegisterBush(bush);

                bushRightIndex++;
            }
        }

        if (listDetector != null && (detectorIndex < listDetector.Count)
            && listDetector[detectorIndex].position.y <= player.position.y)
        {
            detectorIndex += 1;
            reachAreaEvent?.Invoke(detectorIndex + 1);
        }
    }

    #region Public Function
    public void RefreshPlatform()
    {
        Debug.Log("REFRESH!");

        isPlatformMove = false;

        bushLeftIndex = 0;
        bushMiddleIndex = 0;
        bushRightIndex = 0;
        detectorIndex = 0;

        listLeftBush = new List<Bush>();
        listMiddleBush = new List<Bush>();
        listRightBush = new List<Bush>();
        listDOTweenBush = new List<Transform>();
        listDetector = new List<Transform>();

        trackParent.position = defaultTrackParentPosition;
        doll.position = defaultDollPosition;
        doll.localScale = defaultDollScale;

        trackParent.DOKill();
        doll.DOKill();

        foreach (Transform child in trackParent.transform)
            Destroy(child.gameObject);

        // Track
        for (int i = 0; i < trackColors.Length; i++)
        {
            Transform track = Instantiate(trackPrefab, Vector3.zero, Quaternion.identity);
            track.name = "Track" + (i + 1);
            track.parent = trackParent;
            track.localScale = new Vector3(track.localScale.x, trackLength, track.localScale.z);
            track.localPosition = new Vector3(0, trackYOffset + i * trackLength, 0);
            track.GetComponent<SpriteRenderer>().color = trackColors[i];
        }

        Transform trackFinish = Instantiate(trackFinishPrefab, Vector3.zero, Quaternion.identity);
        trackFinish.name = "FinishTrack";
        trackFinish.parent = trackParent;
        trackFinish.localPosition = new Vector3(0, trackYOffset + trackColors.Length * trackLength, 0);

        // Track Detector
        for(int i = 0; i < trackColors.Length; i++)
        {
            Transform detector = Instantiate(trackDetector, Vector3.zero, Quaternion.identity);
            detector.name = "Detector" + (i + 1);
            detector.parent = trackParent;
            detector.localScale = Vector3.one;
            detector.localPosition = new Vector3(0, trackYOffset + (i + 1) * trackLength, 0);

            listDetector.Add(detector);
        }

        // Grass
        SpawnBush(1);
        SpawnBush(2);
        SpawnBush(3);

        reachAreaEvent?.Invoke(detectorIndex + 1);
    }

    public void StartPlatform()
    {
        isPlatformMove = true;

        float yStop = -9.6f - (trackLength * (trackColors.Length - 1));
        trackParent.DOLocalMoveY(yStop, trackTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            isPlatformMove = false;

            foreach (var bush in listDOTweenBush)
                bush.DOPause();

            platformEndEvent?.Invoke();
        });

        Vector3 targetPosition = new Vector3(dollPoint.position.x, dollPoint.position.y, doll.position.z);
        doll.DOMove(targetPosition, trackTime).SetEase(Ease.Linear);
        doll.DOScale(Vector3.one, trackTime).SetEase(Ease.Linear);

        trackParent.DOPause();
        doll.DOPause();
    }

    public void SetMove(bool isMove)
    {
        if (isMove)
        {
            trackParent.DOPlay();
            doll.DOPlay();

            foreach (var bush in listDOTweenBush)
            {
                if (isPlatformMove)
                    bush.DOPlay();
            }
        }
        else
        {
            trackParent.DOPause();
            doll.DOPause();

            foreach (var bush in listDOTweenBush)
                bush.DOPause();
        }
    }
    #endregion

    #region Private Function
    private void SpawnBush(int position)
    {
        List<BushPoint> listBushPoint = new List<BushPoint>();
        List<Bush> listBush = new List<Bush>();
        string naming = string.Empty;

        switch (position)
        {
            case 1:
                listBushPoint = bushLeftPoints;
                listBush = listLeftBush;
                naming = "left";

                break;
            case 2:
                listBushPoint = bushMiddlePoints;
                listBush = listMiddleBush;
                naming = "middle";

                break;
            case 3:
                listBushPoint = bushRightPoints;
                listBush = listRightBush;
                naming = "right";

                break;
        }

        int bushId = 1;
        float currentTrackLength = (position == 2) ? 0 : Math.Abs(trackParent.position.y - dollPoint.position.y);
        while (currentTrackLength <= (trackColors.Length * trackLength))
        {
            int index = UnityEngine.Random.Range(0, listBushPoint.Count);

            Transform prefab;
            switch (position)
            {
                case 2:
                    middleIndexPrefab = (middleIndexPrefab == 0) ? 1 : 0;
                    index = middleIndexPrefab;
                    prefab = insidePrefab[UnityEngine.Random.Range(0, insidePrefab.Length)];
                    break;
                default:
                    prefab = outsidePrefab[UnityEngine.Random.Range(0, outsidePrefab.Length)];
                    break;
            }

            Transform bushTransform = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            bushTransform.name = "Grass_" + naming + "_" + bushId;
            bushTransform.parent = trackParent;
            bushTransform.position = listBushPoint[index].start.position;
            bushTransform.localPosition = new Vector3(bushTransform.localPosition.x, currentTrackLength, bushTransform.localPosition.z);
            bushTransform.localScale = new Vector3(0, 0, 0);

            Bush bush = new Bush()
            {
                bush = bushTransform,
                bushPoint = listBushPoint[index]
            };
            listBush.Add(bush);

            if (bushTransform.position.y <= dollPoint.position.y)
            {
                if (position == 2)
                    RegisterBush(bush);

                bush.bush.DOPause();

                switch (position)
                {
                    case 1:
                        bushLeftIndex++;
                        break;
                    case 2:
                        bushMiddleIndex++;
                        break;
                    case 3:
                        bushRightIndex++;
                        break;
                }
            }

            bushId++;
            currentTrackLength += (UnityEngine.Random.Range(20, 40) / 10f);
        }
    }

    private void RegisterBush(Bush bush)
    {
        Transform bushTransform = bush.bush;
        BushPoint bushPoint = bush.bushPoint;

        float totalDuration = trackTime / trackColors.Length + 2;
        float maxDistance = trackLength;
        float currentDistance = Vector3.Distance(bushTransform.position, bushPoint.end.position);
        float percentage = (currentDistance / maxDistance);
        float duration = totalDuration * percentage;

        float newPercentage = (1 - percentage) <= 0 ? 0 : (1 - percentage);
        bushTransform.localScale = newPercentage * bushPoint.end.localScale;
        bushTransform.position = new Vector3(
            bush.bushPoint.start.position.x + ((bushPoint.end.position.x - bush.bushPoint.start.position.x) * newPercentage),
            bushTransform.position.y, bushTransform.position.z);

        bushTransform.DOScale(bushPoint.end.localScale, duration).SetEase(Ease.Linear);
        bushTransform.DOMoveX(bushPoint.end.position.x, duration).SetEase(Ease.Linear);

        listDOTweenBush.Add(bushTransform);
    }
    #endregion

    #region Listener
    private void OnButtonDownEvent()
    {
        if (StatusHandler.Instance.status != StatusHandler.STATUS.OnGameplay) return;

        SetMove(true);
        Gameplay.Instance.SetSpritePlayer(true);
    }

    private void OnButtonUpEvent()
    {
        if (StatusHandler.Instance.status != StatusHandler.STATUS.OnGameplay) return;

        SetMove(false);
        Gameplay.Instance.SetSpritePlayer(false);
    }
    #endregion
}