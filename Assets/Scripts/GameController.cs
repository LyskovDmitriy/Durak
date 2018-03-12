using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour 
{

	public static GameController Instance { get; private set; }


	public Suit Trump { get; private set; }


	public Player[] players;
	public Image TrumpCardImage;
	public GameObject DeckImage;
	public Text cardsLeftNumberText;
	public GameObject gameOverUI;
	public Text winnerText;
	public Toggle deckSizeToggle;
	public GameObject deckChoiceScreen;


	private Dictionary<string, Sprite> cardToSprite;
	private List<Card> usedInTurnCards;
	private Deck currentDeck;
	private int currentCardIndex;
	private int defaultCardsCount = 6;
	private int currentAttackingPlayer;
	private int currentActivePlayer;
	private bool isGameOver = false;


	public Sprite GetCardSprite(Card card)
	{
		string spriteName = string.Format("{0}_{1}", card.value, card.suit);

		if (cardToSprite.ContainsKey(spriteName))
		{
			return cardToSprite[spriteName];
		}
		else
		{
			return null;
		}
	}


	public Sprite GetCardBack()
	{
		string backName = "Back_Red";
		if (cardToSprite.ContainsKey(backName))
		{
			return cardToSprite[backName];
		}
		else
		{
			return null;
		}
	}


	public bool TryUseCard(Player player, Card cardToUse)
	{
		bool canBeUsed = false;

		if (PlayerIsAttacker(player))
		{
			//it is first card to use or a card with the same value has already been used
			if ((usedInTurnCards.Count == 0) || CardValueWasUsed(cardToUse)) 
			{
				canBeUsed = true;
			}
			else
			{
				canBeUsed = false;
			}
		}
		else
		{
			Card lastUsedCard = usedInTurnCards.Last();

			//active card is trump but the last one to be used was not
			if ((cardToUse.suit == Trump) && (lastUsedCard.suit != Trump))
			{
				canBeUsed = true;
			}
			//active card isn't trump but the last one to be used was
			else if ((cardToUse.suit != Trump) && (lastUsedCard.suit == Trump))
			{
				canBeUsed = false;
			}
			else
			{
				//if both cards have the same suit
				if (cardToUse.suit == lastUsedCard.suit)
				{
					canBeUsed = lastUsedCard.value < cardToUse.value;
				}
				else
				{
					canBeUsed = false;
				}
			}
		}

		if (canBeUsed)
		{
			usedInTurnCards.Add(cardToUse);
		}

		return canBeUsed;
	}


	public void EndTurn(Player playerThatEndedTurn)
	{
		if (usedInTurnCards.Count == 0)
		{
			return;
		}

		UsedCardsHolder.Instance.ClearUsedCards();

		CheckForGameEnd();

		if (isGameOver)
		{
			return;
		}

		if (PlayerIsAttacker(playerThatEndedTurn))
		{
			playerThatEndedTurn.AddCards(DrawCards(playerThatEndedTurn));
			currentAttackingPlayer = (currentAttackingPlayer + 1) % players.Length;
		}
		else
		{
			playerThatEndedTurn.AddCards(usedInTurnCards.ToArray());
		}
			
		usedInTurnCards.Clear();

		//if attacker ended the turn then the other player is defender
		//if defender ended the turn then the other player is attacker
		Player theOtherPlayer = GetTheOtherPlayer(playerThatEndedTurn);
		theOtherPlayer.AddCards(DrawCards(theOtherPlayer));

		currentActivePlayer = currentAttackingPlayer;
		SetInteractivity();
	}


	public void RestartGame()
	{
		usedInTurnCards.Clear();
		gameOverUI.SetActive(false);
		DeckImage.SetActive(true);
		TrumpCardImage.gameObject.SetActive(true);
		for (int i = 0; i < players.Length; i++)
		{
			players[i].ClearHand();
		}
		deckChoiceScreen.SetActive(true);
	}


	public void StartGame () 
	{
		deckChoiceScreen.SetActive(false);
		isGameOver = false;
		CreateDeck();
		for (int i = 0; i < players.Length; i++)
		{
			players[i].AddCards(DrawCards(defaultCardsCount));
		}

		ChoosePlayerToAttack();
	}


	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}

		UsedCardsHolder.Instance.onCardMoved += OnCardUsed;
		usedInTurnCards = new List<Card>();

		LoadSprites();
	}


	void CreateDeck()
	{
		currentDeck = new Deck(deckSizeToggle.isOn);
		currentDeck.RandomizeCards();
		currentCardIndex = currentDeck.GetFirstCardIndex();

		Card lastCard = currentDeck.GetLastCard();
		Trump = lastCard.suit;
		TrumpCardImage.sprite = GetCardSprite(lastCard);
	}


	void LoadSprites()
	{
		cardToSprite = new Dictionary<string, Sprite>();

		Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites");
		for (int i = 0; i < sprites.Length; i++)
		{
			cardToSprite.Add(sprites[i].name, sprites[i]);
		}
	}


	Card[] DrawCards(Player player)
	{
		int cardsToDraw = defaultCardsCount - player.NumberOfCards;

		if (cardsToDraw < 0)
		{
			return null;
		}
		else
		{
			return DrawCards(cardsToDraw);
		}
	}


	Card[] DrawCards(int count)
	{
		if ((count <= 0) || (currentCardIndex < 0))
		{
			return null;
		}

		List<Card> cards = new List<Card>();

		for (int i = 0; i < count; i++)
		{
			cards.Add(currentDeck.GetCard(currentCardIndex));
			currentCardIndex--;
			if (currentCardIndex < 0)
			{
				break;
			}
		}
		cardsLeftNumberText.text = (currentCardIndex + 1).ToString();
		if (currentCardIndex <= 0)
		{
			DeckImage.SetActive(false);
			if (currentCardIndex < 0)
			{
				TrumpCardImage.gameObject.SetActive(false);
			}
		}
		return cards.ToArray();
	}


	Player GetTheOtherPlayer(Player player)
	{
		return (player == players[0]) ? players[1] : players[0];
	}


	void OnCardUsed()
	{
		currentActivePlayer = (currentActivePlayer + 1) % players.Length;

		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].NumberOfCards <= 0)
			{
				if (!PlayerIsAttacker(players[i]))
				{
					//must be tested
					if (currentCardIndex < 0)
					{
						EndTurn(GetTheOtherPlayer(players[i]));
					}
					else
					{
						EndGame();
						return;
					}
				}
			}
		}

		SetInteractivity();
	}


	void SetInteractivity(bool disableAll = false)
	{
		for (int i = 0; i < players.Length; i++)
		{
			if (disableAll)
			{
				players[i].SetInteractivity(false);
			}
			else
			{
				players[i].SetInteractivity(i == currentActivePlayer);
			}
		}
	}


	bool CardValueWasUsed(Card cardToUse)
	{
		for (int i = 0; i < usedInTurnCards.Count; i++)
		{
			if (cardToUse.value == usedInTurnCards[i].value)
			{
				return true;
			}
		}

		return false;
	}


	bool PlayerIsAttacker(Player player)
	{
		int playerIndex = (players[0] == player) ? 0 : 1;
		return (playerIndex == currentAttackingPlayer);
	}


	void CheckForGameEnd()
	{
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].NumberOfCards <= 0)
			{
				EndGame();
			}
		}
	}


	void EndGame()
	{
		UsedCardsHolder.Instance.ClearUsedCards();
		isGameOver = true;
		SetInteractivity(true);

		gameOverUI.SetActive(true);
		if ((players[0].NumberOfCards == 0) && (players[1].NumberOfCards == 0))
		{
			winnerText.text = "Draw";
		}
		else
		{
			string winnerName = "";
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].NumberOfCards == 0)
				{
					winnerName = players[i].name;
					break;
				}
			}
			winnerText.text = winnerName + " won!";
		}
	}


	void ChoosePlayerToAttack()
	{
		Card player1Trump = players[0].GetLowestTrump();
		Card player2Trump = players[1].GetLowestTrump();

		if (player1Trump != null && player2Trump == null) //1 player has a trump. 2 doesn't
		{
			currentAttackingPlayer = 0;
		}
		else if (player1Trump == null && player2Trump != null) //1 player no trumps, 2 has one
		{
			currentAttackingPlayer = 1;
		}
		else if (player1Trump == null && player2Trump == null) //players don't have trumps
		{
			Card player1LowestCard = players[0].GetLowestNotTrumpCard();
			Card player2LowestCard = players[1].GetLowestNotTrumpCard();

			if (player1LowestCard.value < player2LowestCard.value)
			{
				currentAttackingPlayer = 0;
			}
			else if (player2LowestCard.value < player1LowestCard.value)
			{
				currentAttackingPlayer = 1;
			}
			else
			{
				currentAttackingPlayer = Random.Range(0, 2);
			}
		}
		//if both players have trumps
		else if (player1Trump.value < player2Trump.value)
		{
			currentAttackingPlayer = 0;
		}
		else if (player2Trump.value < player1Trump.value)
		{
			currentAttackingPlayer = 1;
		}
			
		currentActivePlayer = currentAttackingPlayer;
		SetInteractivity();
	}


	void OnDestroy()
	{
		UsedCardsHolder.Instance.onCardMoved -= OnCardUsed;
	}
}
