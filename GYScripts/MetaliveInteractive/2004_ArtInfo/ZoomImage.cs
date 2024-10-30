using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ZoomImage : MonoBehaviour
{
    public float zoomSpeed = 0.001f;
    public float maxScale = 2f;
    public float minScale = 0.2f;
    private Vector2 touch1PrevPos, touch2PrevPos;
    private float prevTouchDeltaMag, touchDeltaMag;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    // 아무 곳이나 터치 가능
    private void Update()
    // 사진만 터치 가능
    // public void OnDrag(PointerEventData eventData)
    {
        if (!canvas.enabled)
        {
            if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one;
            }
            return;
        }

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                touch1PrevPos = touch1.position - touch1.deltaPosition;
                touch2PrevPos = touch2.position - touch2.deltaPosition;
                prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
                touchDeltaMag = (touch1.position - touch2.position).magnitude;
                float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

                Vector3 newScale = transform.localScale + new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, 0) * zoomSpeed;
                // if (newScale.x <= 0 || newScale.y <= 0)
                // {
                //     return;
                // }
                newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
                newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
                transform.localScale = newScale;
            }
        }
        else if (Input.touchCount == 1)
        {
            Touch touch1 = Input.GetTouch(0);
            if (touch1.phase == TouchPhase.Moved)
            {
                transform.position = new Vector2(transform.position.x + touch1.deltaPosition.x, transform.position.y + touch1.deltaPosition.y);
            }
        }
    }
}