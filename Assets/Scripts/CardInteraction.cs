using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardInteraction : MonoBehaviour 
{

	public Player cardHolder;
	public int indexInPlayerDeck;


	private Sprite originalSprite;
	private Image cardImage;
	private bool isHidden = false;


	public void OnClick () 
	{
		if (cardHolder != null)
		{
			cardHolder.ChooseCard(indexInPlayerDeck, this);
		}
	}


	public void Hide()
	{
		isHidden = true;
		originalSprite = cardImage.sprite;
		cardImage.sprite = GameController.Instance.GetCardBack();
	}


	public void MoveToUsedCards()
	{
		if (isHidden)
		{
			cardImage.sprite = originalSprite;
			isHidden = false;
		}

		GetComponent<Button>().interactable = false;
		UsedCardsHolder.Instance.AddCard(GetComponent<RectTransform>());
	}


	void Awake()
	{
		cardImage = GetComponent<Image>();
	}


	void OnDisable()
	{
		isHidden = false;
		originalSprite = null;
	}
}
