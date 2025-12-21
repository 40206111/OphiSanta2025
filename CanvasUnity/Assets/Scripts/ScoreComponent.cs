using TMPro;
using UnityEngine;

public class ScoreComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _highScoreLable;
    [SerializeField] TextMeshProUGUI _scoreLable;

    int _lastScore = -100;

    private void OnRestart()
    {
        if ( GameController.Instance.HighScores.Count > 0 )
        {
            _highScoreLable.text = GameController.Instance.HighScores[0].ToString();
            _highScoreLable.transform.parent.gameObject.SetActive(true);
        }
    }
    private void Awake()
    {
        if (GameController.Instance.HighScores.Count == 0)
        {
            _highScoreLable.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _highScoreLable.text = GameController.Instance.HighScores[0].ToString();
        }
        GameController.Instance.Restart += OnRestart;
    }

    private void OnDestroy()
    {
        GameController.Instance.Restart -= OnRestart;
    }

    void Update()
    {
        if (_lastScore != GameController.Instance.CurrentScore)
        {
            _lastScore = GameController.Instance.CurrentScore;
            _scoreLable.text = _lastScore.ToString();
        }
    }
}
