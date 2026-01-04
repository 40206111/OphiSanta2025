using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PaintballManager : MonoBehaviour
{
    public static PaintballManager Instance;

    [SerializeField] Paintball paintballPrefab;
    [SerializeField] List<Transform> positionList = new List<Transform>();
    Material canvasMat;
    [SerializeField] SpriteRenderer canvasSpriteRenderer;
    
    List<Paintball> paintballList = new List<Paintball>();

    Dictionary<string, Paintball> activeBalls = new Dictionary<string, Paintball>();
    Dictionary<string, Paintball> pooledBalls = new Dictionary<string, Paintball>();

    private int ballNo = 0;
    private readonly float canvasResScaleFactor = 1.5f;

    private Texture2D _canvasTexture;

    public void AddToPool(Paintball paintball)
    {
        activeBalls.Remove(paintball.gameObject.name);
        pooledBalls[paintball.gameObject.name] = paintball;
    }

    public Paintball GetNextBall()
    {
        var ball = paintballList[0];
        ball.Target = null;
        ball.transform.position = positionList[0].position;
        activeBalls.Add(ball.gameObject.name, ball);

        for (int i = 0; i < paintballList.Count - 1; i++)
        {
            int next = i + 1;
            var ballToMove = paintballList[next];
            ballToMove.Target = positionList[i];
            ballToMove.gameObject.SetActive(true);
            paintballList[i] = ballToMove;
        }

        var newBall = SpawnNewBall();
        newBall.transform.position = positionList.Last().position;
        paintballList[^1] = newBall;

        return ball;
    }

    private void SetUpBalls()
    {
        for (int i = 0; i < positionList.Count; i++)
        {
            var newBall = SpawnNewBall();
            newBall.transform.position = positionList[i].position;
            if (paintballList.Count <= i)
            {
                paintballList.Add(newBall);
            }
            else
            {
                paintballList[i] = newBall;
            }
            
            newBall.gameObject.SetActive(i != positionList.Count - 1);
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

    public void OnRestart()
    {
        foreach (var paintball in activeBalls)
        {
            paintball.Value.ResetBall();
        }

        pooledBalls.AddRange(activeBalls);
    }

    public void OnGameLost()
    {
        var bounds = canvasSpriteRenderer.localBounds;
        var scaledBounds = Vector3.Scale( bounds.extents, canvasSpriteRenderer.transform.localScale);
        var scaledSize = Vector3.Scale( bounds.size, canvasSpriteRenderer.transform.localScale);
        var zero = canvasSpriteRenderer.transform.position - scaledBounds; 
        foreach (var ball in activeBalls)
        {
            ball.Value.Splat();

            BallSplatOnCanvas(ball.Value, zero, scaledSize);

        }
        _canvasTexture.Apply(true, false);
        canvasMat.SetTexture("_PaintingTex", _canvasTexture);

        foreach (var ball in paintballList)
        {
            ball.ResetBall();
            pooledBalls.Add(ball.gameObject.name, ball);
        }
        activeBalls.Clear();
    }

    public void BigBallSplat(Paintball ball)
    {
        ball.Splat();

        var bounds = canvasSpriteRenderer.localBounds;
        var scaledBounds = Vector3.Scale(bounds.extents, canvasSpriteRenderer.transform.localScale);
        var scaledSize = Vector3.Scale(bounds.size, canvasSpriteRenderer.transform.localScale);
        var zero = canvasSpriteRenderer.transform.position - scaledBounds;
        
        BallSplatOnCanvas(ball, zero, scaledSize);

        _canvasTexture.Apply(true, false);
        canvasMat.SetTexture("_PaintingTex", _canvasTexture);
    }

    public void BallSplatOnCanvas(Paintball ball, Vector3 zero, Vector3 scaledSize)
    {
        var pos = ball.transform.position - zero;

        if (pos.x < 0 || pos.y < 0 || pos.x > scaledSize.x || pos.y > scaledSize.y)
        {
            pos *= canvasResScaleFactor;
        }
        else
        {
            pos *= canvasResScaleFactor;
            _canvasTexture.SetPixel((int)pos.x, (int)pos.y, ball.PaintTexture.GetPixel(0, 0));
        }

        var tier = ball.Tier;
        var min = Mathf.Max(tier - 3, 0);
        var splats = Random.Range(min, tier + tier * canvasResScaleFactor);
        int xVariation = 0;
        int yVariation = 0;
        for (int i = 0; i <= splats; ++i)
        {
            while (Mathf.Abs(xVariation) + Mathf.Abs(yVariation) == 0)
            {
                var range = (int)(ball.transform.localScale.x * 1.2f);
                xVariation = Random.Range(-range, range);
                yVariation = Random.Range(-range, range);
            }
            var newPos = pos;
            newPos.x += xVariation;
            newPos.y += yVariation;
            xVariation = 0;
            yVariation = 0;
            var textureSize = (int)Mathf.Pow(2, ball.Tier);
            int randColIndex = Random.Range(0, textureSize);
            int x = randColIndex % 16;
            int y = randColIndex / 16;
            _canvasTexture.SetPixel((int)newPos.x, (int)newPos.y, ball.PaintTexture.GetPixel(x, y));
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GameController.Instance.GameStarted += SetUpBalls;
            GameController.Instance.Restart += OnRestart;
            GameController.Instance.GameLost += OnGameLost;
            GameController.Instance.MaxBallPop += BigBallSplat;

            canvasMat = canvasSpriteRenderer.material;

            var bounds = canvasSpriteRenderer.localBounds;
            var scaledSize = Vector3.Scale( bounds.size, canvasSpriteRenderer.transform.localScale);
            var increasedSize = scaledSize * canvasResScaleFactor;
            var width = (int)(increasedSize.x);
            var height = (int)(increasedSize.y);
            _canvasTexture = new Texture2D(width, height);
            var colours = System.Buffers.ArrayPool<Color>.Shared.Rent(width * height);
            for (int i = 0; i < colours.Length; i++)
            {
                colours[i] = Color.white;
            }
            _canvasTexture.SetPixels(colours);

            System.Buffers.ArrayPool<Color>.Shared.Return(colours);
            _canvasTexture.Apply(true, false);
            canvasMat.SetTexture("_PaintingTex", _canvasTexture);
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
        GameController.Instance.GameLost -= OnGameLost;
        GameController.Instance.MaxBallPop -= BigBallSplat;
    }
}
