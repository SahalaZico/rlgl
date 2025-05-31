using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonRun : Singleton<ButtonRun>, IPointerDownHandler, IPointerUpHandler
{
    [Header("Spine")]
    [SerializeField] SkeletonGraphic spine;
    [SerializeField] Transform coinAbsorb;

    [Header("Event")]
    [SerializeField] public UnityEvent buttonDownEvent;
    [SerializeField] public UnityEvent buttonUpEvent;

    void Start()
    {
        GameplayHandler.Instance.gameWinLoseEvent.AddListener(OnGameWinLoseEvent);
    }

    public void Refresh()
    {
        if (GameplayHandler.Instance.isCharacterMove)
            spine.AnimationState.SetAnimation(0, GameplayHandler.Instance.isRun ? "sprint hit" : "run hit", true);
        else
            spine.AnimationState.SetAnimation(0, GameplayHandler.Instance.isRun ? "sprint" : "run", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        coinAbsorb.DOKill();
        coinAbsorb.DOScale(Vector3.one, .3f);
        coinAbsorb.gameObject.SetActive(true);

        spine.AnimationState.SetAnimation(0, GameplayHandler.Instance.isRun ? "sprint hit" : "run hit", true);

        buttonDownEvent?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        coinAbsorb.DOScale(Vector3.zero, .3f).OnComplete(() => coinAbsorb.gameObject.SetActive(false));

        spine.AnimationState.SetAnimation(0, GameplayHandler.Instance.isRun ? "sprint" : "run", false);

        buttonUpEvent?.Invoke();
    }

    private void OnGameWinLoseEvent(bool isWin)
    {
        spine.AnimationState.SetAnimation(0, "run", false);
    }
}