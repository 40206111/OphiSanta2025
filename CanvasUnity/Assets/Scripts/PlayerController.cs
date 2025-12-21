using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] Paintball paintBallPrefab;
    [SerializeField] Transform DropLine;
    [SerializeField] Transform RightLine;
    [SerializeField] Transform LeftLine;
    [SerializeField] float Cooldown = 0.2f;

    Paintball myBall;
    float _timeSinceFired;
    bool Ready => _timeSinceFired >= Cooldown && GameRunning;
    bool GameRunning = false;

    private int ballNo = 0;

    private void Awake()
    {
        GameController.Instance.GameLost += OnLoss;
        GameController.Instance.GameStarted += OnStart;
        GameController.Instance.Restart += OnRestart;
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
        GameController.Instance.Restart -= OnRestart;
    }

    public void OnLoss()
    {
        GameRunning = false;
        myBall = null;
        GameController.Instance.SplatBalls();
    }
    public void OnStart()
    {
        GameRunning = true;
    }

    public void OnRestart()
    {
        foreach ( var paintball in GameController.Instance.ActiveBalls )
        {
            paintball.Value.ResetBall();
        }

        GameController.Instance.PooledBalls.AddRange( GameController.Instance.ActiveBalls );
        GameController.Instance.ActiveBalls.Clear();
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
            if (GameController.Instance.PooledBalls.Count == 0)
            {
                myBall = Instantiate(paintBallPrefab);
                myBall.gameObject.name = $"Paintball_{ballNo}";
                ballNo++;
            }
            else
            {
                var ballData = GameController.Instance.PooledBalls.Last();
                myBall = ballData.Value;
                GameController.Instance.PooledBalls.Remove(ballData.Key);
            }
            myBall.gameObject.SetActive(true);
            myBall.transform.position = GetWorldPoint(eventData.position);
            GameController.Instance.ActiveBalls.Add(myBall.gameObject.name, myBall);
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
