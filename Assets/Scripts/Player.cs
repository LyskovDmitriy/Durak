using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

public class Player : MonoBehaviour 
{
	
	public int NumberOfCards { get { return hand.Count; } }


	public ObjectPool cardImagesPool;
	public float imageWidth = 190;
	public Button endTurnButton;


	protected List<Card> hand;
	protected List<GameObject> cardObjects;
	private RectTransform rTrans;


	public void AddCards(Card[] cardsToAdd)
	{
		if (cardsToAdd != null && cardsToAdd.Length > 0)
		{
			for (int i = 0; i < cardsToAdd.Length; i++)
			{
				hand.Add(cardsToAdd[i]);
			}

			SortHand();
			RefreshCardsObjects();
		}
	}


	public bool ChooseCard(int index, CardInteraction cardInteraction)
	{
		Card cardToUse = hand[index];
		bool cardCanBeUsed = GameController.Instance.TryUseCard(this, cardToUse);

		if (!cardCanBeUsed)
		{
			return false;
		}
			
		hand.RemoveAt(index);
		cardObjects.RemoveAt(index);
		cardInteraction.MoveToUsedCards();

		SortHand();
		RefreshCardsObjects();
		SetInteractivity(false);

		return true;
	}


	public Card GetLowestTrump()
	{
		Suit trump = GameController.Instance.Trump;
		Card lowestTrump = null;

		for (int i = hand.Count - 1; i >= 0; i--)
		{
			if (hand[i].suit == trump)
			{
				lowestTrump = hand[i];
				break;
			}
		}

		return lowestTrump;
	}


	public Card GetLowestNotTrumpCard()
	{
		if (hand[hand.Count - 1].suit != GameController.Instance.Trump)
		{
			return hand[hand.Count - 1];
		}

		return null;
	}


	public void EndTurn()
	{
		GameController.Instance.EndTurn(this);
	}


	public virtual void SetInteractivity(bool isInteractable)
	{
		//can loop through children if all of them have Button component that is to be enabled/disabled
		for (int i = 0; i < cardObjects.Count; i++)
		{
			cardObjects[i].GetComponent<Button>().interactable = isInteractable;
		}
		endTurnButton.interactable = isInteractable;
	}


	public void ClearHand()
	{
		hand.Clear();
		for (int i = 0; i < cardObjects.Count; i++)
		{
			cardObjects[i].GetComponent<PoolSignature>().ReturnToPool();
		}
		cardObjects.Clear();
	}


	void Awake()
	{
		hand = new List<Card>(); 
		rTrans = GetComponent<RectTransform>();
		cardObjects = new List<GameObject>();
	}


	void SortHand()
	{
		Suit trumpSuit = GameController.Instance.Trump;

		hand.Sort((x, y) => 
			{
				bool xIsTrump = (x.suit == trumpSuit);
				bool yIsTrump = (y.suit == trumpSuit);

				if (xIsTrump && !yIsTrump)
				{
					return -1;
				}
				else if (!xIsTrump && yIsTrump)
				{
					return 1;
				}
				else
				{
					return ((int)y.value).CompareTo((int)x.value);
				}
			});
	}


	void RefreshCardsObjects()
	{
		while (cardObjects.Count > 0)
		{
			cardObjects[0].GetComponent<PoolSignature>().ReturnToPool();
			cardObjects.RemoveAt(0);
		}
			
		Vector3 cardPosition = -new Vector3((hand.Count - 1) * imageWidth / 2, 0.0f, 0.0f);
		Vector3 distanceBetweenCards;
		//checks if the distance between cards should be less than default image width
		bool hasEnoghSpaceForCards = -rTrans.sizeDelta.x / 2 < (cardPosition.x - imageWidth / 2);

		if (hasEnoghSpaceForCards)
		{
			distanceBetweenCards = new Vector3(imageWidth, 0.0f, 0.0f);
		}
		else
		{
			cardPosition.x = -rTrans.sizeDelta.x / 2 + imageWidth / 2;
			distanceBetweenCards = new Vector3((rTrans.sizeDelta.x - imageWidth) / (hand.Count - 1), 0.0f, 0.0f);
		}

		for (int i = 0; i < hand.Count; i++, cardPosition += distanceBetweenCards)
		{
			GameObject cardObject = cardImagesPool.GetObject();
			cardObject.transform.SetParent(transform);
			cardObject.SetActive(true);
			cardObject.transform.localPosition = cardPosition;

			CardInteraction interactiveComponent = cardObject.GetComponent<CardInteraction>();
			interactiveComponent.cardHolder = this;
			interactiveComponent.indexInPlayerDeck = i;

			cardObjects.Add(cardObject);
		}

		SetCardsSprites();
	}

	//is overriden for AI player to hide cards
	protected virtual void SetCardsSprites()
	{
		for (int i = 0; i < cardObjects.Count; i++)
		{
			Image cardImage = cardObjects[i].GetComponent<Image>();
			Button cardButton = cardObjects[i].GetComponent<Button>();
			cardButton.interactable = true;
			cardImage.sprite = GameController.Instance.GetCardSprite(hand[i]);
		}
	}
}
