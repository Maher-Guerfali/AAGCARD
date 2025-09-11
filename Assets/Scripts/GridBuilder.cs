using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridBuilder : MonoBehaviour
{
    [Header("Data")]
    public CardSet cardSet;       // ScriptableObject with front/back sprites
    public GameSettings settings; // rows, cols, pairSize, etc.

    private List<Card> cards = new List<Card>();

    /// <summary>
    /// Builds a new grid of cards inside the given container using CardSet sprites.
    /// </summary>
    public void BuildGrid(Card cardPrefab, RectTransform container)
    {
        ClearGrid();

        int total = settings.rows * settings.cols;

        if (total % settings.pairSize != 0)
            Debug.LogWarning("Grid size not divisible by pairSize, some cards may not match.");

        // --- prepare deck
        List<int> poolIds = new List<int>();
        int groupCount = total / settings.pairSize;

        for (int i = 0; i < groupCount; i++)
        {
            for (int j = 0; j < settings.pairSize; j++)
                poolIds.Add(i); // assign same ID for pair/triplet
        }

        Shuffle(poolIds);

        // --- instantiate cards
        for (int i = 0; i < total; i++)
        {
            var c = Instantiate(cardPrefab, container);
            int id = poolIds[i];
            Sprite frontSprite = cardSet.frontSprites[id % cardSet.frontSprites.Count];

            c.Initialize(id, frontSprite, cardSet.backSprite);
            cards.Add(c);
        }

        // optional: setup GridLayoutGroup constraints if using UI
        var layout = container.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (layout != null)
        {
            layout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = settings.cols;
        }
    }

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

    public void RestoreState(GameState state)
    {
        if (state == null || state.cards == null) return;

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
            Sprite frontSprite = cardSet.frontSprites[cs.faceId % cardSet.frontSprites.Count];
            cards[i].Initialize(cs.faceId, frontSprite, cardSet.backSprite);

            if (cs.isRevealed) cards[i].Reveal();
            if (cs.isMatched) cards[i].MarkMatched();
        }
    }

    public bool AllMatched()
    {
        return cards.All(c => c.IsMatched);
    }

    public void ClearGrid()
    {
        foreach (var c in cards)
        {
            if (c != null) Destroy(c.gameObject);
        }
        cards.Clear();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
