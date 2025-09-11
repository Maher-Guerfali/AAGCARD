using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game")]
    public GridBuilder gridBuilder;
    public Card cardPrefab;
    public RectTransform gridContainer; // UI container or empty gameobject
    public int rows = 4;
    public int cols = 4;
    public float revealDelay = 0.8f;
    public int pairSize = 2; // number of cards per match (2 for classic)

    // gameplay state
    private List<Card> flippedQueue = new List<Card>();
    private Queue<Card> compareQueue = new Queue<Card>();
    private bool comparing = false;

    [Header("Score")]
    public int score = 0;
    public int combo = 0;
    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        score = 0;
        combo = 0;
        uiManager?.UpdateScore(score, combo);
        gridBuilder.BuildGrid(cardPrefab, gridContainer); 
    }




    public void RegisterFlip(Card card)
    {
        // allow flipping at any time; just queue for comparison
        compareQueue.Enqueue(card);
        if (!comparing) StartCoroutine(ProcessComparisons());
    }

    private IEnumerator ProcessComparisons()
    {
        comparing = true;
        while (compareQueue.Count >= pairSize)
        {
            var group = new List<Card>();
            for (int i = 0; i < pairSize; i++)
                group.Add(compareQueue.Dequeue());

            // if any in group were already matched or currently locked, skip them
            if (group.Any(c => c.IsMatched || !c.IsRevealed))
                continue;

            // Wait small buffer for visuals if user keeps flipping
            yield return new WaitForSeconds(0.15f);

            bool isMatch = IsMatch(group);
            if (isMatch)
            {
                foreach (var c in group)
                {
                    c.MarkMatched();
                }
                combo++;
                int gained = 100 * combo;
                score += gained;
                SoundManager.Instance.PlayMatch();
                uiManager?.ShowMatchEffect(group, gained, combo);
            }
            else
            {
                combo = 0;
                SoundManager.Instance.PlayMismatch();
                yield return new WaitForSeconds(revealDelay);
                foreach (var c in group)
                {
                    if (!c.IsMatched) c.HideInstant();
                }
            }

            uiManager?.UpdateScore(score, combo);

            // check end condition
            if (gridBuilder.AllMatched())
            {
                SoundManager.Instance.PlayGameOver();
                uiManager?.ShowGameOver(score);
            }
        }
        comparing = false;
    }

    private bool IsMatch(List<Card> group)
    {
        if (!group.Any()) return false;
        var id = group[0].faceId;
        return group.All(c => c.faceId == id);
    }

    // Save current state
    public GameState CaptureState()
    {
        return gridBuilder.CaptureState(score, combo);
    }

    public void LoadState(GameState state)
    {
        score = state.score;
        combo = state.combo;
        uiManager?.UpdateScore(score, combo);
        gridBuilder.RestoreState(state);
    }
}
