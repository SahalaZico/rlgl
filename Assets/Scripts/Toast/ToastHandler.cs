using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToastHandler : MonoBehaviour
{
    public static ToastHandler Instance { get; protected set; }

    [SerializeField] protected GameObject container = null;
    [SerializeField] protected CanvasGroup canvas = null;
    [SerializeField] protected TMP_Text text = null;

    protected Sequence currentSequence = null;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    public void Close()
    {
        if (currentSequence != null)
        {
            if (currentSequence.IsActive())
                currentSequence.Kill();
        }

        canvas.alpha = 0f;
        this.text.text = "";
        container.SetActive(false);
    }

    public void Show(string text, float durationFadeOut)
    {
        if (currentSequence != null)
        {
            if (currentSequence.IsActive())
                currentSequence.Kill();

            currentSequence = null;
        }
        canvas.alpha = 0f;
        this.text.text = text;
        container.SetActive(true);
        currentSequence = DOTween.Sequence();
        currentSequence.Append(canvas.DOFade(1f, 0.5f));
        currentSequence.AppendInterval(durationFadeOut);
        currentSequence.Append(canvas.DOFade(0f, 0.8f));
        currentSequence.OnComplete(() => {
            canvas.alpha = 0f;
            container.SetActive(false);
            this.text.text = "";
        });
    }
}
