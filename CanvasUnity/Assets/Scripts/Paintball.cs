using System.Collections;
using UnityEngine;

public class Paintball : MonoBehaviour
{
    [SerializeField] private float startingScale = 0.5f;
    [SerializeField] private float timeOutTime = 0.5f;
    [SerializeField] private float scaleFactor = 1.25f;
    private Rigidbody2D _rigidbody;

    public bool Consumed;

    bool _fired;

    public int Tier = 0;

    float _restTime;

    float _failLine;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale *= startingScale;
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
                var yVal = transform.position.y + (transform.localScale.y * 0.5f);
                if (yVal > _failLine)
                {
                    GameController.Instance.ChangeState(eGameState.Lost);
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
        _rigidbody.simulated = true;
        _fired = true;
        _failLine = failLine;
    }

    public void ResetBall()
    {
        gameObject.SetActive(false);
        _fired = false;
        Consumed = false;
        Tier = 0;
        _rigidbody.simulated = false;
        transform.localScale = Vector3.one;
        transform.localScale *= startingScale;
        _restTime = 0;

        GameController.Instance.PooledBalls.Add(this);
    }

    public void RemoveBall()
    {
        ResetBall();
        GameController.Instance.ActiveBalls.Remove(this);
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
            transform.localScale *= scaleFactor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_fired || Consumed)
        {
            return;
        }

        GameController.Instance.ChangeState(eGameState.Lost);
    }
}
