using UnityEngine;

public class GameOverComponent : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
        GameController.Instance.GameLost += OnLose;
        GameController.Instance.Restart += OnRestart;
    }
    private void OnDestroy()
    {
        GameController.Instance.GameLost += OnLose;
        GameController.Instance.Restart -= OnRestart;
    }

    public void OnLose()
    {
        gameObject.SetActive(true);
    }


    public void OnRestart()
    {
        gameObject.SetActive(false);
    }
}
