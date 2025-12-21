using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] Transform DropLine;
    [SerializeField] Transform RightLine;
    [SerializeField] Transform LeftLine;
    [SerializeField] float SpeedToTop = 20f;

    Paintball myBall;
    bool Ready => !_trackingBall && GameRunning;
    bool _trackingBall = false;
    bool GameRunning = false;

    bool _pointerDown = false;

    Vector3 trackPoint;

    private void Awake()
    {
        GameController.Instance.GameLost += OnLoss;
        GameController.Instance.GameStarted += OnStart;
    }

    private void OnDestroy()
    {
        GameController.Instance.GameLost -= OnLoss;
        GameController.Instance.GameStarted -= OnStart;
    }

    public void OnLoss()
    {
        GameRunning = false;
        myBall = null;
    }
    public void OnStart()
    {
        GameRunning = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameController.Instance.GameState == eGameState.PreStart)
        {
            GameController.Instance.ChangeState(eGameState.Running);
            return;
        }
        if (GameController.Instance.GameState == eGameState.Lost)
        {
            GameController.Instance.ChangeState(eGameState.PreStart);
            return;
        }

        if (Ready && myBall == null)
        {
            _pointerDown = true;
            myBall = PaintballManager.Instance.GetNextBall();
            StartCoroutine(TrackBallToPoint( GetWorldPoint(eventData.position)));
        }
    }
    IEnumerator<YieldInstruction> TrackBallToPoint( Vector3 point )
    {
        if (_trackingBall)
        {
            yield return null;
        }
        trackPoint = point;
        _trackingBall = true;
        point.z = myBall.transform.position.z;
        while (myBall != null && (myBall.transform.position - trackPoint).sqrMagnitude > 0.01f)
        {
            myBall.transform.position = Vector3.Lerp(myBall.transform.position, trackPoint, Time.deltaTime * SpeedToTop);
            yield return null;
        }

        if (myBall != null)
        {
            myBall.transform.position = trackPoint;
        }
        _trackingBall = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (myBall == null)
        {
            return;
        }
        _pointerDown = false;
        StartCoroutine(FireBall());
    }

    IEnumerator<YieldInstruction> FireBall()
    {
        while (_trackingBall)
        {
            if (_pointerDown)
            {
                yield break;
            }
            yield return null;
        }
        if (myBall == null)
        {
            yield break;
        }
        myBall.FirePaintBall(DropLine.position.y);
        myBall = null;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (myBall == null)
        {
            return;
        }

        if (_trackingBall)
        {
            trackPoint = GetWorldPoint(eventData.position);
            return;
        }

        myBall.transform.position = GetWorldPoint( eventData.position );
    }

    private Vector2 GetWorldPoint( Vector2 eventPos )
    {
        var pos = eventPos;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.y = DropLine.position.y;
        var offset = Random.Range(-0.001f, 0.001f);
        pos.x += offset;
        pos.x = Mathf.Clamp(pos.x, LeftLine.position.x + LeftLine.localScale.x, RightLine.position.x - LeftLine.localScale.x);
        return pos;
    }

    private void OnDrawGizmos()
    {
        if (myBall != null)
        {
            Gizmos.color = Color.blueViolet;
            Gizmos.DrawLine(myBall.transform.position, trackPoint);
        }
    }

}
