using UnityEngine;

public class DeleteHighScoresComponent : MonoBehaviour
{
    [SerializeField] GameObject MainButton;
    [SerializeField] GameObject ConfirmButtons;
    [SerializeField] float ProtectionDelay = 0.25f;

    private float _timePassed = 0;

    private void OnEnable()
    {
        MainButton.SetActive(true);
        ConfirmButtons.SetActive(false);
    }

    private void Update()
    {
        if (_timePassed < ProtectionDelay)
        {
            _timePassed += Time.deltaTime;
        }
    }

    public void AskToDelete_Ui()
    {
        MainButton.SetActive(false);
        ConfirmButtons.SetActive(true);
        _timePassed = 0;
    }

    public void ConfirmYes_Ui()
    {
        if (_timePassed >= ProtectionDelay)
        {
            return;
        }

        GameController.Instance.DeleteHighScores();

        MainButton.SetActive(true);
        ConfirmButtons.SetActive(false);
    }

    public void ConfirmNo_Ui()
    {
        MainButton.SetActive(true);
        ConfirmButtons.SetActive(false);
    }
}
