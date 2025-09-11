using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridBuilder : MonoBehaviour
{
    [Header("Data")]
    public CardSet cardSet;           // ScriptableObject with card faces + back
    public GameSettings settings;     // ScriptableObject with rows, cols, etc.

    private List<Card> cards = new List<Card>();

    /// <summary>
    /// Builds a new grid of cards inside the given container.
    /// </summary>
    // Remove int rows parameter
    public void BuildGrid(Card cardPrefab, RectTransform container)
    {
        ClearGrid();

        int total = settings.rows * settings.cols;

        if (total % settings.pairSize != 0)
            Debug.LogWarning("Grid size not divisible by pair size, some cards may not match.");

        // --- prepare deck
        List<CardData> pool = new List<CardData>();
        int groupCount = total / settings.pairSize;

        for (int i = 0; i < groupCount; i++)
        {
            var cd = cardSet.cards[i % cardSet.cards.Count];
            for (int j = 0; j < settings.pairSize; j++)
                pool.Add(cd);
        }

        Shuffle(pool);

        // --- instantiate cards
        foreach (var cd in pool)
        {
            var c = Instantiate(cardPrefab, container);
            c.Initialize(cd.id, cd.faceSprite, cardSet.backSprite);
            cards.Add(c);
        }
    }


    /// <summary>
    /// Capture the current state into a serializable GameState.
    /// </summary>
    public GameState CaptureState(int score, int combo)
    {
        GameState s = new GameState
        {
            rows = settings.rows,
            cols = settings.cols,
            score = score,
            combo = combo,
            cards = cards.Select(c => new CardState
            {
                faceId = c.faceId,
                isMatched = c.IsMatched,
                isRevealed = c.IsRevealed
            }).ToList()
        };
        return s;
    }

    /// <summary>
    /// Restore game state (cards, matched/revealed, score/combo handled in GameManager).
    /// </summary>
    public void RestoreState(GameState state)
    {
        if (state == null || state.cards == null) return;

        // If grid size differs, rebuild grid
        if (state.rows != settings.rows || state.cols != settings.cols)
        {
            Debug.Log("Saved state grid does not match current settings. Rebuilding grid.");
            settings.rows = state.rows;
            settings.cols = state.cols;
            BuildGrid(GameManager.Instance.cardPrefab, GameManager.Instance.gridContainer);
        }

        if (state.cards.Count != cards.Count)
        {
            Debug.LogWarning("Card count mismatch when restoring state.");
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            var cs = state.cards[i];
            var cd = cardSet.cards[cs.faceId % cardSet.cards.Count];
            cards[i].Initialize(cd.id, cd.faceSprite, cardSet.backSprite);

            if (cs.isRevealed) cards[i].Reveal();
            if (cs.isMatched) cards[i].MarkMatched();
        }
    }

    /// <summary>
    /// Returns true if all cards are matched.
    /// </summary>
    public bool AllMatched()
    {
        return cards.All(c => c.IsMatched);
    }

    /// <summary>
    /// Destroy existing cards and clear list.
    /// </summary>
    public void ClearGrid()
    {
        foreach (var c in cards)
        {
            if (c != null) Destroy(c.gameObject);
        }
        cards.Clear();
    }

    /// <summary>
    /// Fisher-Yates shuffle.
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
