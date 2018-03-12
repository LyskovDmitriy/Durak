using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIPlayer : Player 
{

	public float timeToMakeMove;

	//AI player makes his moves here
	public override void SetInteractivity(bool isInteractable)
	{
		if (isInteractable)
		{
			StartCoroutine(MakeMove());
		}
	}


	IEnumerator MakeMove()
	{
		yield return new WaitForSeconds(timeToMakeMove);

		for (int i = hand.Count - 1; i >= 0; i--)
		{
			if (ChooseCard(i, cardObjects[i].GetComponent<CardInteraction>())) //if card was successfully used
			{
				yield break;
			}
		}
		//if no card can be used then end turn
		EndTurn();
	}


	protected override void SetCardsSprites()
	{
		for (int i = 0; i < cardObjects.Count; i++)
		{
			Image cardImage = cardObjects[i].GetComponent<Image>();
			Button cardButton = cardObjects[i].GetComponent<Button>();
			cardButton.interactable = false;
			cardImage.sprite = GameController.Instance.GetCardSprite(hand[i]);
			cardImage.GetComponent<CardInteraction>().Hide();
		}
	}
}
