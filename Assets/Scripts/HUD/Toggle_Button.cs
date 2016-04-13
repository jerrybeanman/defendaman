using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class Toggle_Button : MonoBehaviour {
	
	public bool ButtonOn = false;
	private Button mybutton;

	void Start()
	{
		mybutton = gameObject.GetComponent<Button>();
	}

	/*----------------------------------------------------------------------------
    --	Change button color when pressed
    --
    --	Interface:  void CheckChatAction()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void BeenClicked()
	{
		ButtonOn = !ButtonOn;
		if(ButtonOn)
		{
			mybutton.image.color = Color.green;
		}
		else
		{
			mybutton.image.color = Color.white;
		}
	}
}