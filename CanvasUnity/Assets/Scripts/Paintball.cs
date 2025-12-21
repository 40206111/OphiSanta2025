using System.Collections;
using UnityEngine;

public class Paintball : MonoBehaviour
{
    const string SplatTrigger = "Splat";
    const string ResetTrigger = "Reset";

    [SerializeField] private float timeOutTime = 0.5f;
    [SerializeField] private float scaleFactor = 1.25f;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private CircleCollider2D _collider;

    public bool Consumed;

    bool _fired;

    private int _tier;
    public int Tier
    {
        get { return _tier; }
        set 
        {
            _tier = value;
            transform.localScale = Vector3.one * Mathf.Pow(scaleFactor, _tier);
        }
    }

    float _restTime;

    float _failLine;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<CircleCollider2D>();
        ResetBall();
    }

    private void Update()
    {
        if (!_fired)
        {
            return;
        }

        if ( _rigidbody.linearVelocity.magnitude <= 0.01f && _rigidbody.angularVelocity <= 0.01f)
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


    public void FirePaintBall(float failLine)
    {
        _animator.ResetTrigger(ResetTrigger);
        _rigidbody.simulated = true;
        _fired = true;
        _failLine = failLine;
    }

    public void ResetBall()
    {
        gameObject.SetActive(false);
        _fired = false;
        Consumed = false;
        _rigidbody.simulated = false;
        _restTime = 0;
        Tier = Random.Range(0, 3);
        _animator.ResetTrigger(SplatTrigger);
        _animator.SetTrigger(ResetTrigger);
    }

    public void RemoveBall()
    {
        ResetBall();
        GameController.Instance.AddToPool(this);
    }

    public void Splat()
    {
        _animator.SetTrigger(SplatTrigger);
        _rigidbody.simulated = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_fired || Consumed)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<Paintball>(out var paintBall))
        {
            if (paintBall.Tier != Tier)
            {
                return;
            }
            paintBall.Consumed = true;
            paintBall.RemoveBall();
            Tier++;
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
