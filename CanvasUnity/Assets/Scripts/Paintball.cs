using System.Collections;
using UnityEngine;

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

    public Color MyColour { get; private set; }

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

    private Texture2D _paintTexture;

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

        var rand = Random.Range(0, pallet.Count);
        MyColour = pallet[rand];

        for (int i = 1; i < Tier + 1; i++)
        {
            rand = Random.Range(0, pallet.Count);
            MyColour = MixColours(MyColour, pallet[rand]);
        }
        _spriteRenderer.color = MyColour;
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
        _paintTexture = new Texture2D(16, 16);
        var colours = System.Buffers.ArrayPool<Color>.Shared.Rent(256);
        for (int i = 0; i < colours.Length; i++)
        {
            colours[i] = i < PalletCreator.MaxColours ? GameController.Instance.PalletCreator.Colours[i] : Color.white;
        }
        _paintTexture.SetPixels(colours);

        _paintTexture.filterMode = FilterMode.Point;

        System.Buffers.ArrayPool<Color>.Shared.Return(colours);
        _paintTexture.Apply(true, false);
        myMat.SetTexture("_Colours", _paintTexture);
    }

    private Color MixColours(Color colour1, Color colour2)
    {
        colour1 = colour1 * 0.5f + colour2 * 0.5f;
        colour1 = colour1 == Color.white ? new Color(0.9f, 0.9f, 0.9f) : colour1;

        return colour1;
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
        Tier++;
        GameController.Instance.CurrentScore += Tier * Tier;

        MyColour = MixColours(MyColour, paintball.MyColour);
        _spriteRenderer.color = MyColour;


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
