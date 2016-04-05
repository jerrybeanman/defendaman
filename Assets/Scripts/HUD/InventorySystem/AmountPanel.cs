using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class AmountPanel : MonoBehaviour {

    private Item _item;
    private int _amt;
    private int _inv_pos;
    private int _drop_amt;
    private GameObject _amt_input_field;
    private GameObject _amount_panel;
    private Text _error_text;
    private WorldItemManager _world_item_manager;

    void Start () {
        _world_item_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<WorldItemManager>();
        _amount_panel = GameObject.Find("Amount Panel");
        _amt_input_field = GameObject.Find("Amount InputField");
        _error_text = _amount_panel.transform.Find("Error Text").GetComponent<Text>();
        _amount_panel.SetActive(false);
	}

    /*
     * Deactivate amount panel when there is mouse click outside the panel
     */
    void Update()
    {
        if (!GameData.MouseBlocked && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0)))
        {
            Deactivate();
        }
    }

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

    public void Deactivate()
    {
        _amount_panel.SetActive(false);
    }

   public void OkDropButtonOnClick()
    {
        Debug.Log("ok drop clicked");
        _drop_amt = int.Parse(_amt_input_field.GetComponent<InputField>().text);
        if (_drop_amt > 0 && _drop_amt <= _amt)
        {
            // Send Network message
            List<Pair<string, string>> msg = 
                _world_item_manager.CreateDropItemNetworkMessage(_item.id, _drop_amt, _inv_pos);
            NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Drop, msg, Protocol.UDP);

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

    public void CancelDropButtonOnClick()
    {
        GameData.MouseBlocked = false;
        Deactivate();
    }
}
