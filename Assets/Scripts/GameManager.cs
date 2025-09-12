using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public GridBuilder gridBuilder;
    public Card cardPrefab;
    public RectTransform gridContainer;
    [Min(1)] public int rows = 4;
    [Min(1)] public int cols = 4;
    [Range(0.1f, 2f)] public float revealDelay = 0.8f;
    [Min(2)] public int pairSize = 2;

    [Header("Score")]
    public int score = 0;
    public int combo = 0;
    public UIManager uiManager;

    // Gameplay state
    private readonly Queue<Card> compareQueue = new Queue<Card>();
    private bool comparing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        score = 0;
        combo = 0;
        uiManager?.UpdateScore(score, combo);

        // Clear any pending comparisons
        compareQueue.Clear();
        comparing = false;

        gridBuilder.BuildGrid(cardPrefab, gridContainer);
    }

    public void RegisterFlip(Card card)
    {
        Debug.Log($"RegisterFlip called for card {card.faceId}. IsRevealed: {card.IsRevealed}, IsMatched: {card.IsMatched}");

        // Only register if card is actually revealed and not matched
        if (!card.IsRevealed || card.IsMatched)
        {
            Debug.Log($"Card {card.faceId} registration skipped - not revealed or already matched");
            return;
        }

        // Add the flipped card to the queue
        compareQueue.Enqueue(card);
        Debug.Log($"Card {card.faceId} added to queue. Queue count: {compareQueue.Count}");

        // Start comparison routine if not already running
        if (!comparing)
        {
            Debug.Log("Starting comparison process");
            StartCoroutine(ProcessComparisons());
        }
    }

    private IEnumerator ProcessComparisons()
    {
        comparing = true;

        while (compareQueue.Count >= pairSize)
        {
            Debug.Log($"Processing comparison with {compareQueue.Count} cards in queue");

            // Collect group of flipped cards
            var group = new List<Card>();
            for (int i = 0; i < pairSize; i++)
            {
                if (compareQueue.Count > 0)
                    group.Add(compareQueue.Dequeue());
            }

            Debug.Log($"Collected {group.Count} cards for comparison");

            // Skip if invalid (already matched or flipped back)
            if (group.Any(c => c.IsMatched || !c.IsRevealed))
            {
                Debug.Log("Skipping group - some cards are matched or not revealed");
                continue;
            }

            // Small delay for visuals
            yield return new WaitForSeconds(0.15f);

            // Check match
            bool isMatch = IsMatch(group);
            Debug.Log($"Match result: {isMatch} for cards with IDs: {string.Join(", ", group.Select(c => c.faceId))}");

            if (isMatch)
            {
                // MATCH - Mark all cards as matched
                foreach (var c in group)
                    c.MarkMatched();

                combo++;
                int gainedPoints = 100 * combo;
                score += gainedPoints;

                Debug.Log($"Match! Score: {score}, Combo: {combo}");

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayMatch();

                uiManager?.ShowMatchEffect(group, gainedPoints, combo);
            }
            else
            {
                // MISMATCH - Reset combo and hide cards after delay
                combo = 0;
                Debug.Log($"Mismatch! Cards will flip back. Combo reset to 0");

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayMismatch();

                // Use the proper HideAfterMismatch method
                foreach (var c in group)
                {
                    if (!c.IsMatched)
                        c.HideAfterMismatch();
                }
            }

            // Update score after each check
            uiManager?.UpdateScore(score, combo);

            // Check end condition
            if (gridBuilder.AllMatched())
            {
                Debug.Log("All cards matched! Game over.");
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayGameOver();
                uiManager?.ShowGameOver(score);
                break;
            }
        }

        comparing = false;
        Debug.Log("Comparison process ended");
    }

    private bool IsMatch(List<Card> group)
    {
        if (group.Count == 0) return false;

        int id = group[0].faceId;
        bool match = group.All(c => c.faceId == id);

        Debug.Log($"Checking match for {group.Count} cards. Reference ID: {id}, All match: {match}");
        return match;
    }

    // --- Game State Saving & Loading ---
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