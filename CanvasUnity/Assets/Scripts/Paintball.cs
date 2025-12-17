using UnityEngine;

public class Paintball : MonoBehaviour
{
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }


    public void FirePaintBall()
    {
        _rigidbody.simulated = true;
    }
}
