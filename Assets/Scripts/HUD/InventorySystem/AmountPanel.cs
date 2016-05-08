using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/*----------------------------------------------------------------------------------------
-- AmountPanel.cs - Script attached to AmountPanel GameObject that is enabled when a 
--                  player attempts to drop stackable items with more than one stack. 
--                  Allows players to select how many of the selected item to drop.
--
-- FUNCTIONS:
--      void Start()
--      void Update ()
--      public void Activate(Item item, int amt, int inv_pos)
--      public void Deactivate()
--      public void OkDropButtonOnClick()
--      public void CancelDropButtonOnClick()
--
-- DATE:        10/03/2016
-- REVISIONS:   
-- DESIGNER:    Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-------------------------------------------------------------------------------------------*/
public class AmountPanel : MonoBehaviour {

    private Item _item;
    private int _amt;
    private int _inv_pos;
    private int _drop_amt;
    private GameObject _amt_input_field;
    private GameObject _amount_panel;
    private Text _error_text;
    private WorldItemManager _world_item_manager;

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		10/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Retrieves the WorldItemManager script, the AmountPanel GameObject, the Amount 
    -- InputField GameObject and the Error Text object.
    -- Deactivates the AmountPanel GameObject.
    ----------------------------------------------------------------------------------------*/
    void Start () {
        _world_item_manager = 
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<WorldItemManager>();
        _amount_panel = GameObject.Find("Amount Panel");
        _amt_input_field = GameObject.Find("Amount InputField");
        _error_text = _amount_panel.transform.Find("Error Text").GetComponent<Text>();
        _amount_panel.SetActive(false);
	}

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		10/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Update()
    -- RETURNS: 	void
    -- NOTES:
    -- Deactivates amount panel when there is mouse click outside the panel
    ----------------------------------------------------------------------------------------*/
    void Update()
    {
        if (!GameData.MouseBlocked && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0)))
        {
            Deactivate();
        }
    }

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Activate
    -- DATE: 		10/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void Activate(Item item, int amt, int inv_pos)
    --                  Item item: The item in the inventory slot
    --                  int amt: The amount of the item
    --                  int inv_pos: The inventory position
    -- RETURNS: 	void
    -- NOTES:
    -- Activates the AmountPanel and resets the error message.
    ----------------------------------------------------------------------------------------*/
    public void Activate(Item item, int amt, int inv_pos)
    {
        Debug.Log("Activate");
        _item = item;
        _amt = amt;
        _inv_pos = inv_pos;
        _amount_panel.SetActive(true);
        _error_text.text = "";
        _amt_input_field.GetComponent<InputField>().text = _amt.ToString();
    }

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Deactivate
    -- DATE: 		10/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void Deactivate()
    -- RETURNS: 	void
    -- NOTES:
    -- Deactivates the AmountPanel.
    ----------------------------------------------------------------------------------------*/
    public void Deactivate()
    {
        _amount_panel.SetActive(false);
    }

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	OkDropButtonOnClick
    -- DATE: 		10/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OkDropButtonOnClick()
    -- RETURNS: 	void
    -- NOTES:
    -- Checks that the amount trying to be dropped is allowed. If yes, send a DropItem 
    -- network message, otherwise update the error message.
    ----------------------------------------------------------------------------------------*/
    public void OkDropButtonOnClick()
    {
        Debug.Log("ok drop clicked");
        _drop_amt = int.Parse(_amt_input_field.GetComponent<InputField>().text);
        if (_drop_amt > 0 && _drop_amt <= _amt)
        {
            // Send Network message
            List<Pair<string, string>> msg = 
                _world_item_manager.CreateDropItemNetworkMessage(_item.id, _drop_amt, _inv_pos);
            NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Drop, msg, Protocol.TCP);

            // Pretend that a drop message was received
			if (Application.platform != RuntimePlatform.LinuxPlayer) {
				_world_item_manager.ReceiveItemDropPacket(_world_item_manager.ConvertListToJSONClass(msg));
			}

            GameData.MouseBlocked = false;
            Deactivate();
        }
        else
        {
            _error_text.text = "Invalid amount";
        }
    }

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	CancelDropButtonOnClick
    -- DATE: 		10/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void CancelDropButtonOnClick()
    -- RETURNS: 	void
    -- NOTES:
    -- Deactivates the AmountPanel. 
    ----------------------------------------------------------------------------------------*/
    public void CancelDropButtonOnClick()
    {
        GameData.MouseBlocked = false;
        Deactivate();
    }
}
