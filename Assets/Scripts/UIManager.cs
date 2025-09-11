using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text comboText;
    public GameObject gameOverPanel;

    public void UpdateScore(int score, int combo)
    {
        if (scoreText) scoreText.text = $"Score: {score}";
        if (comboText) comboText.text = $"Combo: {combo}";
    }

    public void ShowMatchEffect(List<Card> group, int gained, int combo)
    {
        // small popup or particle (optional). Keep minimal: update UI
        UpdateScore(GameManager.Instance.score, GameManager.Instance.combo);
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        UpdateScore(finalScore, GameManager.Instance.combo);
    }
}
