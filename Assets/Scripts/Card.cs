[System.Serializable]
public class Card
{

	public Suit suit { get; private set; }
	public Value value { get; private set; }


	public Card (Suit suit, Value value)
	{
		this.suit = suit;
		this.value = value;
	}
}
	
public enum Suit { Clubs = 0, Hearts = 1, Spades = 2, Diamonds = 3 }

public enum Value { Two = 0, Three = 1, Four = 2, Five = 3, Six = 4, Seven = 5, Eight = 6, Nine = 7, Ten = 8, 
	Jack = 9, Queen = 10, King = 11, Ace = 12 }