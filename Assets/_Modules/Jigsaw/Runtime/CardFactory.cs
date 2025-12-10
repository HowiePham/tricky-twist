using UnityEditor;
using UnityEngine;

public class CardFactory
{
    private Card cardPrefab;
    private Transform parent;

    public CardFactory(Card cardPrefab, Transform parent)
    {
        this.cardPrefab = cardPrefab;
        this.parent = parent;
    }

    public Card CreateCard(Vector3 cardPosition)
    {
        var card = (Card)PrefabUtility.InstantiatePrefab(this.cardPrefab, this.parent);
        card.transform.localPosition = cardPosition;

        return card;
    }
}