using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText; // Change from 'TMPro scoreText' to 'TMP_Text scoreText'
    public TMP_Text comboText;
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
