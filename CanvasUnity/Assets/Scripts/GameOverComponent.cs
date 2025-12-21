using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameOverComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI numberEntryPrefab;
    [SerializeField] TextMeshProUGUI valueEntryPrefab;
    [SerializeField] Transform numberParent;
    [SerializeField] Transform valueParent;

    List<TextMeshProUGUI> numberEntries = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> valueEntries = new List<TextMeshProUGUI>();

    private void Awake()
    {
        gameObject.SetActive(false);
        GameController.Instance.GameLost += OnLose;
        GameController.Instance.Restart += OnRestart;
    }
    private void OnDestroy()
    {
        GameController.Instance.GameLost -= OnLose;
        GameController.Instance.Restart -= OnRestart;
    }

    public void OnLose()
    {
        var scores = GameController.Instance.HighScores;
        for (int i = 0; i < scores.Count; i++)
        {
            if (numberEntries.Count <= i)
            {
                var newNumber = Instantiate(numberEntryPrefab, numberParent);
                newNumber.text = $"{i + 1}.";
                numberEntries.Add(newNumber);
            }
            if (valueEntries.Count <= i)
            {
                var newValue = Instantiate(valueEntryPrefab, valueParent);
                newValue.text = $"{scores[i]}";
                valueEntries.Add(newValue);
            }
            else
            {
                var newValue = valueEntries[i];
                newValue.text = $"{scores[i]}";
            }
        }
        gameObject.SetActive(true);
    }


    public void OnRestart()
    {
        gameObject.SetActive(false);
    }
}
