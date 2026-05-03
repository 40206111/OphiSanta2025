using UnityEngine;

public class MenuControllerComponent : MonoBehaviour
{
    [SerializeField] GameObject Menu;

    private void Start()
    {
        GameController.Instance.OnPauseChanged += OnPauseChanged;
        OnPauseChanged(false);
    }
    private void OnDestroy()
    {
        GameController.Instance.OnPauseChanged -= OnPauseChanged;
    }

    private void OnPauseChanged( bool isPaused )
    {
        Menu.SetActive( isPaused );
    }

    public void ResetGame_Ui()
    {
        GameController.Instance.ChangeState(eGameState.Reset);
    }

    public void ClearCanvas_Ui()
    {
        GameController.Instance.ClearCanvas();
    }
}
