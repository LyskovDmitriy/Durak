using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour 
{

	public static GameController Instance { get; private set; }


	public Suit Trump { get; private set; }


	public Player[] players;
	public Image TrumpCardImage; //to display trump
	public GameObject DeckImage; //represents a deck on the table
	public Text cardsLeftNumberText; //number of cards that are still in the deck
	public GameObject gameOverUI;
	public Text winnerText;
	public Toggle deckSizeToggle; //is responsoble for choosing between small and large decks
	public GameObject deckChoiceScreen;


	private Dictionary<string, Sprite> cardToSprite;
	private List<Card> usedInTurnCards; //all the cards that were used during current turn
	private Deck currentDeck;
	private int defaultCardsNumber = 6;
	private int currentAttackingPlayer;
	private int currentActivePlayer;
	private bool isGameOver = false;


	public Sprite GetCardSprite(Card card)
	{
		string spriteName = string.Format("{0}_{1}", card.value, card.suit); //sprite name looks like "Ace_Hearts"

		if (cardToSprite.ContainsKey(spriteName))
		{
			return cardToSprite[spriteName];
		}
		else
		{
			return null;
		}
	}

	//is used to hide AI Player's cards from user
	public Sprite GetCardBack()
	{
		string backName = "Back_Red"; //standart card back
		if (cardToSprite.ContainsKey(backName))
		{
			return cardToSprite[backName];
		}
		else
		{
			return null;
		}
	}

	//decides whether the card can be used by particular player according to game rules 
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
			//if player is defending, the previous card (that he is defending from) sholud be checked 
			Card previousCard = usedInTurnCards.Last();
			bool cardToUseIsTrump = (cardToUse.suit == Trump);
			bool previousCardIsTrump = (previousCard.suit == Trump);

			//active card is trump but previous one wasn't
			if (cardToUseIsTrump && !previousCardIsTrump)
			{
				canBeUsed = true;
			}
			//active card isn't trump but previous one was
			else if (!cardToUseIsTrump && previousCardIsTrump)
			{
				canBeUsed = false;
			}
			else
			{
				//if both cards have the same suit their values are compared
				if (cardToUse.suit == previousCard.suit)
				{
					canBeUsed = previousCard.value < cardToUse.value;
				}
				else
				{
					canBeUsed = false;
				}
			}
		}
		//if the card can be used it is added to the list of used cards
		if (canBeUsed)
		{
			usedInTurnCards.Add(cardToUse);
		}
		return canBeUsed;
	}


	public void EndTurn(Player playerThatEndedTurn)
	{
		//player can't end the turn if he hasn't done anything
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
		//if attacker ended the turn then the defence was successful, both players draw cards and attacker changes
		if (PlayerIsAttacker(playerThatEndedTurn))
		{
			playerThatEndedTurn.AddCards(DrawCards(playerThatEndedTurn));
			currentAttackingPlayer = (currentAttackingPlayer + 1) % players.Length;
		}
		else
		{
			//if defender ended the turn then he draws all used in turn cards
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
		cardsLeftNumberText.gameObject.SetActive(true);
		for (int i = 0; i < players.Length; i++)
		{
			players[i].ClearHand();
		}
		deckChoiceScreen.SetActive(true);
	}

	//Is called in deck choice screen
	public void StartGame() 
	{
		deckChoiceScreen.SetActive(false);
		isGameOver = false;
		CreateDeck();

		for (int i = 0; i < players.Length; i++)
		{
			players[i].AddCards(DrawCards(players[i]));
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
		//buld deck of particular size that depends on player choice
		currentDeck = new Deck(deckSizeToggle.isOn);
		currentDeck.RandomizeCards();

		Card lastCard = currentDeck.GetLastCard();
		Trump = lastCard.suit;
		TrumpCardImage.sprite = GetCardSprite(lastCard);
	}

	//should be called only once when the game starts
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
		if (currentDeck.isEmpty)
		{
			return null;
		}
		
		int cardsToDraw = defaultCardsNumber - player.NumberOfCards;

		if (cardsToDraw < 0) //return null if player doesn't need any cards
		{
			return null;
		}

		Card[] cards = currentDeck.GetCards(cardsToDraw);

		//update visual representation of the cards left in the deck
		int deckSize = currentDeck.CurrentSize;
		cardsLeftNumberText.text = deckSize.ToString();
		if (deckSize <= 1)
		{
			//disable deck image if the deck has only trump left
			cardsLeftNumberText.gameObject.SetActive(false);
			DeckImage.SetActive(false);
			if (deckSize < 1)
			{
				TrumpCardImage.gameObject.SetActive(false);
			}
		}

		return cards;
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
			//checks if any of the players has 0 cards
			if (players[i].NumberOfCards <= 0)
			{
				//if defender runs out of cards either turn or game ends
				if (!PlayerIsAttacker(players[i]))
				{
					if (currentDeck.isEmpty)
					{
						EndTurn(GetTheOtherPlayer(players[i]));
					}
					else
					{
						EndGame();
					}
				}
				//if attacker runs out of cards nothing is done
				//if the game is won will be checked in EndTurn after the defender makes any actions
			}
		}

		if (isGameOver)
		{
			return;
		}

		SetInteractivity();
	}


	void SetInteractivity(bool disableAll = false) //disableAll is used when the game is over
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

	//checks if any card with the same value was used during the turn
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
		if (currentDeck.isEmpty)
		{
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].NumberOfCards <= 0)
				{
					EndGame();
				}
			}
		}
	}


	void EndGame()
	{
		UsedCardsHolder.Instance.ClearUsedCards();
		isGameOver = true;
		SetInteractivity(true);

		gameOverUI.SetActive(true);
		if ((players[0].NumberOfCards == 0) && (players[1].NumberOfCards == 0)) //if both players have 0 cards
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
