using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardSet", menuName = "Card Game/CardSet")]
public class CardSet : ScriptableObject
{
    public List<Sprite> frontSprites; // all front images
    public Sprite backSprite;         // shared back
}
