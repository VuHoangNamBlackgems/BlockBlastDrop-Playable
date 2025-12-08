using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float pressScale = 0.98f;
    [SerializeField] float smoothSpeed = 15f;

    [HideInInspector]
    public bool IsPressed;

    private Vector3 originalScale;
    private Vector3 targetScale;
    
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * smoothSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressScale;
        IsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;
        IsPressed = false;
    }
}
