using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ReticleHandle : Singleton<ReticleHandle>
{
    [Header("Component")]
    [SerializeField] Image icon;

    RectTransform rectIcon, rectParent;
    float xMin, xMax, yMin, yMax;

    // Start is called before the first frame update
    void Start()
    {
        icon.color = new Color(1, 1, 1, 0);

        rectIcon = icon.GetComponent<RectTransform>();
        rectParent = icon.transform.parent.GetComponent<RectTransform>();
        float width = rectParent.sizeDelta.x;
        float height = rectParent.sizeDelta.y;
        xMin = -width / 2;
        xMax = width / 2;
        yMin = -height / 2;
        yMax = height / 2;

        StartReticle();

        GameplayHandler.Instance.dollTurnAroundEvent.AddListener(OnDollTurnAroundEvent);
        GameplayHandler.Instance.gameWinLoseEvent.AddListener(OnGameWinLoseEvent);
    }

    #region Private Function
    private IEnumerator DelayFunction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private void StartReticle()
    {
        float xPos = Random.Range(xMin, xMax);
        float yPos = Random.Range(yMin, yMax);
        Vector2 targetPosition = new Vector2(xPos, yPos);
        float duration = Random.Range(.2f, .5f);

        rectIcon.DOAnchorPos(targetPosition, duration).OnComplete(() => StartCoroutine(DelayFunction(.1f, StartReticle)));
    }

    private void Show()
    {
        icon.DOFade(1, .3f);
    }

    private void Hide()
    {
        icon.DOFade(0, .3f);
    }
    #endregion

    #region Listener
    private void OnDollTurnAroundEvent(bool isActive)
    {
        if (isActive)
            Show();
        else
            Hide();
    }

    private void OnGameWinLoseEvent(bool isWin)
    {
        Hide();
    }
    #endregion
}