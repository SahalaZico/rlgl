using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITutorialBtn : Button
{
    [SerializeField] protected float onScale = 0.9f;
    [SerializeField] protected float duration = 0.25f;

    protected CanvasGroup canvasGrp = null;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        transform.DOScale(Vector3.one * onScale, duration);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        transform.DOScale(Vector3.one, duration);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        if (canvasGrp == null)
            return;

        if (state == SelectionState.Disabled)
            canvasGrp.alpha = 0.6f;
        else
            canvasGrp.alpha = 1f;
    }

    protected override void Awake()
    {
        base.Awake();
        canvasGrp = GetComponent<CanvasGroup>();
    }

    protected override void Start()
    {
        base.Start();
    }
}
