using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] Paintball paintBallPrefab;
    [SerializeField] Transform DropLine;
    [SerializeField] Transform RightLine;
    [SerializeField] Transform LeftLine;

    Paintball myBall;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (myBall == null)
        {
            myBall = Instantiate(paintBallPrefab, GetWorldPoint(eventData.position), Quaternion.identity);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        myBall.FirePaintBall();
        myBall = null;
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        if (myBall == null)
        {
            return;
        }

        myBall.transform.position = GetWorldPoint( eventData.position );
    }

    private Vector2 GetWorldPoint( Vector2 eventPos )
    {
        var pos = eventPos;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.y = DropLine.position.y;
        pos.x = Mathf.Clamp(pos.x, LeftLine.position.x + LeftLine.localScale.x, RightLine.position.x - LeftLine.localScale.x);
        return pos;
    }

}
