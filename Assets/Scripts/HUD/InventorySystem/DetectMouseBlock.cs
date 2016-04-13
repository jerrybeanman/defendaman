using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------------------
-- WorldItemData.cs - Set the MouseBlocked flag when the mouse pointer hovers over
--                    GameObjects with this script attached. If the flag is set, 
--                    mouse events will be blocked.
--
-- FUNCTIONS:
--      public void OnPointerEnter(PointerEventData eventData)
--      public void OnPointerExit(PointerEventData eventData)
--
-- DATE:        08/03/2016
-- REVISIONS:   
-- DESIGNER:    Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
---------------------------------------------------------------------------------------------*/
public class DetectMouseBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnPointerEnter
    -- DATE: 		08/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnPointerEnter(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Set the MouseBlocked flag to true.
    -------------------------------------------------------------------------------------------------*/
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameData.MouseBlocked = true;
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnPointerExit
    -- DATE: 		08/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnPointerExit(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Set the MouseBlocked flag to false.
    -------------------------------------------------------------------------------------------------*/
    public void OnPointerExit(PointerEventData eventData)
    {
        GameData.MouseBlocked = false;
    }
}
