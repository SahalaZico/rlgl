using UnityEngine;

public class UIPage : MonoBehaviour
{
    [SerializeField] protected GameObject container = null;

    public virtual void SetActive(bool state)
    {
        container?.SetActive(state);
    }

    protected virtual void Awake()
    {
        container.SetActive(false);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }
}
