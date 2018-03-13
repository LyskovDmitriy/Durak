using System.Collections.Generic;
using UnityEngine;

public class Deck 
{

	public int CurrentSize { get { return (currentCardIndex + 1); } }
	public bool isEmpty { get { return (currentCardIndex < 0); } }


	private List<Card> cards;
	private int currentCardIndex; //i.e. index of the card in the deck that will be drawn next


	public Deck(bool isSmallDeck = true)
	{
		cards = new List<Card>();

		int startingNumber = 0;

		if (isSmallDeck) //a small deck doesn't have 2-5
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
		currentCardIndex = cards.Count - 1;
	}


	private Deck() { }

	//A shortcut for getting trumps
	public Card GetLastCard()
	{
		return cards[0];
	}


	public Card[] GetCards(int count)
	{
		int cardsToDraw = (count < CurrentSize) ? count : CurrentSize;

		Card[] cardsToReturn = new Card[cardsToDraw];

		for (int i = 0; i < cardsToDraw; i++)
		{
			cardsToReturn[i] = cards[currentCardIndex];
			currentCardIndex--;
		}

		return cardsToReturn;
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
		//get new index that doesn't equal the old one
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
	}
}
