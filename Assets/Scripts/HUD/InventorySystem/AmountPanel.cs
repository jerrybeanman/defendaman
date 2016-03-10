using UnityEngine;
using System.Collections;

public class AmountPanel : MonoBehaviour {

    private Item _item;
    private GameObject _amount_panel;

    void Start () {
        _amount_panel = GameObject.Find("Amount Panel");
        _amount_panel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Activate(Item item)
    {
        this._item = item;
        _amount_panel.SetActive(true);
    }

    public void Deactivate()
    {
        _amount_panel.SetActive(false);
    }
}
