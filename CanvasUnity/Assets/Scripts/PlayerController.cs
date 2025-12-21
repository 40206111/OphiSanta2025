using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] Transform DropLine;
    [SerializeField] Transform RightLine;
    [SerializeField] Transform LeftLine;
    [SerializeField] float Cooldown = 0.2f;

    Paintball myBall;
    float _timeSinceFired;
    bool Ready => _timeSinceFired >= Cooldown && GameRunning;
    bool GameRunning = false;

    private void Awake()
    {
        GameController.Instance.GameLost += OnLoss;
        GameController.Instance.GameStarted += OnStart;
    }

    private void Update()
    {
        if (!Ready)
        {
            _timeSinceFired += Time.deltaTime;
        }
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
            myBall = PaintballManager.Instance.GetNextBall();
            myBall.transform.position = GetWorldPoint(eventData.position);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (myBall == null)
        {
            return;
        }
        _timeSinceFired = 0;
        myBall.FirePaintBall(DropLine.position.y);
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
        var offset = Random.Range(-0.001f, 0.001f);
        pos.x += offset;
        pos.x = Mathf.Clamp(pos.x, LeftLine.position.x + LeftLine.localScale.x, RightLine.position.x - LeftLine.localScale.x);
        return pos;
    }

}
