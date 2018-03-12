using System.Collections.Generic;
using UnityEngine;

public class Deck 
{

	private List<Card> cards;


	public Deck(bool isSmallDeck = true)
	{
		cards = new List<Card>();

		int startingNumber = 0;

		if (isSmallDeck)
		{
			startingNumber = 4;
		}

		for (int suit = 0; suit <= 3; suit++)
		{
			for (int value = startingNumber; value <= 12; value++)
			{
				cards.Add(new Card((Suit)suit, (Value)value));
			}
		}
	}


	private Deck() { }


	public int GetFirstCardIndex()
	{
		return cards.Count - 1;
	}


	public Card GetLastCard()
	{
		return cards[0];
	}


	public Card GetCard(int index)
	{
		if (index < 0 || cards.Count - 1 < index)
		{
			return null;
		}

		return cards[index];
	}


	public void RandomizeCards()
	{
		for (int i = 0; i < cards.Count; i++)
		{
			ChangeCard(i);
		}

		if (cards[0].value == Value.Ace)
		{
			ChangeCard(0);
		}
	}


	void ChangeCard(int oldIndex)
	{
		int newIndex = 0;
		do
		{
			newIndex = Random.Range(0, cards.Count);
		}
		while(newIndex == oldIndex);
		SwapCards(oldIndex, newIndex);
	}


	void SwapCards(int oldIndex, int newIndex)
	{
		Card tmp = cards[newIndex];
		cards[newIndex] = cards[oldIndex];
		cards[oldIndex] = tmp;
		tmp = null;
	}
}
