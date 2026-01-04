using System.Collections;
using UnityEngine;
using static UnityEditor.UIElements.ToolbarMenu;

public class Paintball : MonoBehaviour
{
    const string SplatTrigger = "Splat";
    const string ResetTrigger = "Reset";

    [SerializeField] private float timeOutTime = 0.5f;
    [SerializeField] private float scaleFactor = 1.25f;
    [SerializeField] private float trackTargetSpeed = 2f;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;

    [HideInInspector] public bool Consumed;
    [HideInInspector] public Transform Target;

    bool _fired;

    private int _tier;
    public int Tier
    {
        get { return _tier; }
        set 
        {
            _tier = value;
            transform.localScale = Vector3.one * Mathf.Pow(scaleFactor, _tier);
            myMat.SetInt("_Tier", _tier);
        }
    }

    float _restTime;

    float _failLine;

    private Material myMat;

    public Texture2D PaintTexture { get; private set; }
    private const float _dimValue = 0.1f;
    private Color _dimColour = new Color(_dimValue, _dimValue, _dimValue, 0.0f);

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        myMat = new Material(Shader.Find("Custom/BallShader"));
        _spriteRenderer.material = myMat;
        SetUpTexture();
        _collider = GetComponent<CircleCollider2D>();
        ResetBall();
    }

    private void OnEnable()
    {
        var pallet = GameController.Instance.PalletCreator.Colours;

        var amount = Mathf.Pow(2, Tier);

        var lastCol = Color.white;
        for (int i = 0; i < amount; i++)
        {
            var rand = Random.Range(0, pallet.Count);
            var newCol = pallet[rand];

            if ( lastCol == newCol )
            {
                newCol -= _dimColour;
            }
            PaintTexture.SetPixel(i, 0, newCol);

            lastCol = newCol;
        }
        PaintTexture.Apply(true, false);
        myMat.SetTexture("_Colours", PaintTexture);
    }

    private void Update()
    {
        if (Target != null)
        {
            transform.position = Vector3.Slerp(transform.position, Target.position, Time.deltaTime * trackTargetSpeed);
            return;
        }

        if (!_fired)
        {
            return;
        }

        if ( _rigidbody.IsSleeping() )
        {
            if (_restTime >= timeOutTime)
            {
                var yVal = transform.position.y + (transform.localScale.y * _collider.radius);
                if (yVal > _failLine)
                {
                    _fired = false;
                    GameController.Instance.ChangeState(eGameState.Lost);
                    Debug.Log($"Game Loss, Ball at rest over line {gameObject.name}");
                }
            }
            else
            {
                _restTime += Time.deltaTime;
            }
        }
        else
        {
            _restTime = 0;
        }
    }

    private void SetUpTexture()
    {
        PaintTexture = new Texture2D(16, 16)
        {
            filterMode = FilterMode.Point
        };

        PaintTexture.Apply(true, false);
        myMat.SetTexture("_Colours", PaintTexture);
    }

    public void FirePaintBall(float failLine)
    {
        _animator.ResetTrigger(ResetTrigger);
        _rigidbody.simulated = true;
        _fired = true;
        _failLine = failLine;
        _spriteRenderer.sortingOrder = -1;
    }

    public void ResetBall()
    {
        gameObject.SetActive(false);
        _fired = false;
        Consumed = false;
        _rigidbody.simulated = false;
        _rigidbody.angularVelocity = 0;
        _rigidbody.linearVelocity = Vector2.zero;
        _restTime = 0;
        Tier = Random.Range(0, 3);
        _animator.ResetTrigger(SplatTrigger);
        _animator.SetTrigger(ResetTrigger);
        _spriteRenderer.sortingOrder = 1;
        Target = null;
    }

    public void RemoveBall()
    {
        ResetBall();
        PaintballManager.Instance.AddToPool(this);
    }

    public void Splat()
    {
        _animator.SetTrigger(SplatTrigger);
        _rigidbody.simulated = false;
    }

    private void OnCollisionStay2D(Collision2D collision) => Collide(collision);

    private void OnCollisionEnter2D(Collision2D collision) => Collide( collision );

    private void Collide(Collision2D collision)
    {
        if (!_fired || Consumed)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<Paintball>(out var paintball))
        {
            if (paintball.Consumed)
            {
                return;
            }
            if (paintball.Tier != Tier)
            {
                return;
            }

            HitPaintball(paintball);
        }
    }

    public IEnumerator RemoveBallAfterWait()
    {
        yield return new WaitForSeconds(0.5f);
        RemoveBall();
    }

    private void HitPaintball( Paintball paintball )
    {
        paintball.Consumed = true;

        paintball.RemoveBall();

        int colourAmount = (int)Mathf.Pow(2, Tier);
        var colours = paintball.PaintTexture.GetPixels();

        var lastCol = Color.white;
        for (int i = 0; i < colourAmount; i++)
        {
            int colourIndex = colourAmount + i;
            int x = colourIndex % 16;
            int y = colourIndex / 16;
            var newCol = colours[i];
            if (lastCol == newCol)
            {
                newCol -= _dimColour;
            }
            PaintTexture.SetPixel(x, y, newCol);
        }

        PaintTexture.Apply(true, false);
        myMat.SetTexture("_Colours", PaintTexture);

        Tier++;
        GameController.Instance.CurrentScore += Tier * Tier;

        if (Tier == 8)
        {
            GameController.Instance.MaxBallPop(this);
            StartCoroutine(RemoveBallAfterWait());
            StartCoroutine(paintball.RemoveBallAfterWait());
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_fired || Consumed)
        {
            return;
        }

        Debug.Log($"Game Loss, Ball out of bounds {gameObject.name}");
        GameController.Instance.ChangeState(eGameState.Lost);
    }
}
