using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Card Match/Game Settings", order = 2)]
public class GameSettings : ScriptableObject
{
    [Header("Layout")]
    public int rows = 4;
    public int cols = 4;
    public float revealDelay = 0.8f;

    [Header("Scoring")]
    public int baseScore = 100;
    public int comboMultiplier = 100;

    [Header("Gameplay")]
    public int pairSize = 2; // how many cards must match
}
