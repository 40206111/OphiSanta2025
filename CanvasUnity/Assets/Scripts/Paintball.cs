using System.Collections;
using UnityEngine;

public class Paintball : MonoBehaviour
{
    private Rigidbody2D _rigidbody;

    public bool Consumed;

    bool _fired;

    public int Tier = 0;

    float _restTime;

    float _failLine;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!_fired)
        {
            return;
        }

        if ( _rigidbody.linearVelocity.magnitude <= 0.01f && _rigidbody.angularVelocity <= 0.01f)
        {
            if (_restTime >= 1f)
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

    private void OnCollisionEnter2D(Collision2D collision)
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
            GameObject.Destroy(paintBall.gameObject);
            Tier++;
            transform.localScale *= 1.5f;
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
