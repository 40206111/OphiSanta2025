using UnityEngine;

public class StartMenuComponent : MonoBehaviour
{
    private void Awake()
    {
        GameController.Instance.GameStarted += OnStart;
        GameController.Instance.Restart += OnRestart;
    }
    private void OnDestroy()
    {
        GameController.Instance.GameStarted -= OnStart;
        GameController.Instance.Restart -= OnRestart;
    }

    public void OnStart()
    {
        gameObject.SetActive(false);
    }


    public void OnRestart()
    {
        gameObject.SetActive(true);
    }

}
