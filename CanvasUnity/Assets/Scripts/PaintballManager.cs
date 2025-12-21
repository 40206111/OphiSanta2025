using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PaintballManager : MonoBehaviour
{
    public static PaintballManager Instance;

    [SerializeField] Paintball paintballPrefab;
    [SerializeField] List<Transform> positionList = new List<Transform>();
    
    List<Paintball> paintballList = new List<Paintball>();

    Dictionary<string, Paintball> activeBalls = new Dictionary<string, Paintball>();
    Dictionary<string, Paintball> pooledBalls = new Dictionary<string, Paintball>();

    private int ballNo = 0;

    public void AddToPool(Paintball paintball)
    {
        activeBalls.Remove(paintball.gameObject.name);
        pooledBalls[paintball.gameObject.name] = paintball;
    }

    public Paintball GetNextBall()
    {
        var ball = paintballList[0];
        ball.transform.parent = null;
        activeBalls.Add(ball.gameObject.name, ball);

        for (int i = 0; i < paintballList.Count - 1; i++)
        {
            int next = i + 1;
            var ballToMove = paintballList[next];
            ballToMove.transform.parent = positionList[i];
            ballToMove.transform.localPosition = Vector3.zero;
            paintballList[i] = ballToMove;
        }

        var newBall = SpawnNewBall();
        newBall.transform.parent = positionList.Last();
        newBall.transform.localPosition = Vector3.zero;
        paintballList[^1] = newBall;
        newBall.gameObject.SetActive(true);

        return ball;
    }

    private void SetUpBalls()
    {
        for (int i = 0; i < positionList.Count; i++)
        {
            var newBall = SpawnNewBall();
            newBall.transform.parent = positionList[i];
            newBall.transform.localPosition = Vector3.zero;
            if (paintballList.Count <= i)
            {
                paintballList.Add(newBall);
            }
            else
            {
                paintballList[i] = newBall;
            }
            newBall.gameObject.SetActive(true);
        }
    }

    private Paintball SpawnNewBall()
    {
        Paintball newBall;
        if (pooledBalls.Count == 0)
        {
            newBall = Instantiate(paintballPrefab);
            newBall.gameObject.name = $"Paintball_{ballNo}";
            ballNo++;
        }
        else
        {
            var ballData = pooledBalls.Last();
            newBall = ballData.Value;
            pooledBalls.Remove(ballData.Key);
        }

        return newBall;
    }

    public void SplatBalls()
    {
        foreach (var ball in activeBalls)
        {
            ball.Value.Splat();
        }
    }
    public void OnRestart()
    {
        foreach (var paintball in activeBalls)
        {
            paintball.Value.ResetBall();
        }

        pooledBalls.AddRange(activeBalls);
        foreach (var ball in paintballList)
        {
            pooledBalls.Add(ball.gameObject.name, ball);
        }
        activeBalls.Clear();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GameController.Instance.GameStarted += SetUpBalls;
            GameController.Instance.Restart += OnRestart;
            GameController.Instance.GameLost += SplatBalls;
        }
        else
        {
            Debug.LogError($"{nameof(PaintballManager)} already exists cannot add a second");
        }
    }

    private void OnDestroy()
    {
        GameController.Instance.GameStarted -= SetUpBalls;
        GameController.Instance.Restart -= OnRestart;
        GameController.Instance.GameLost -= SplatBalls;
    }
}
