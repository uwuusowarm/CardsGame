using System.Collections.Generic;
using UnityEngine;

public class EnemyCardManager : MonoBehaviour
{
    [Header("UI-Referenzen")]
    [SerializeField] private Transform handGrid;
    [SerializeField] private GameObject cardPrefab;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();

    public void InitializeDeck(List<CardData> cards)
    {
        deck.Clear();
        deck.AddRange(cards);
        Shuffle(deck);
        DrawCards(2); 
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    public void DrawCards(int count)
    {
        Debug.Log("Enemy zieht Karten: " + count);
        int toDraw = Mathf.Min(count, deck.Count);
        for (int i = 0; i < toDraw; i++)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
        UpdateAllUI();
    }

    public void PlayCard(CardData card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            UpdateAllUI();
        }
    }

    public void UpdateAllUI()
    {
        RebuildContainer(handGrid, hand);
    }

    private void RebuildContainer(Transform parent, List<CardData> list)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);

        foreach (var c in list)
        {
            var go = Instantiate(cardPrefab, parent);
            go.GetComponent<CardUI>().Initialize(c);
        }
    }

    public List<CardData> GetHand() => hand;
}