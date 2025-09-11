using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSet", menuName = "Card Match/Card Set", order = 1)]
public class CardSet : ScriptableObject
{
    public List<CardData> cards;
    public Sprite backSprite;
}
