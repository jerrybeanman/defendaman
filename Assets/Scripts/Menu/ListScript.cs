using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ListScript : MonoBehaviour {

    public GameObject player_slot;
    [Range(1,2)]
    public float spacing_factor = 1.1f;

    private VerticalLayoutGroup _vLayout;
    private RectTransform _rt;

    private float _new_top;
    private float _old_top;
    private float _new_spacing;
    private float _old_spacing;
    private int _screen_height;


    // Use this for initialization
    void Start () {
        _vLayout = this.GetComponent<VerticalLayoutGroup>();
        _rt = (RectTransform)player_slot.transform;
        _old_top = _vLayout.padding.top;
        _old_spacing = _rt.rect.height;
        _vLayout.spacing = spacing_factor * _old_spacing;
        _screen_height = Screen.height;
    }
	
	// Update is called once per frame
	void Update () {
        _rt = (RectTransform)player_slot.transform;
        _new_spacing = _rt.rect.height;

        if (_new_spacing != _old_spacing)
        {
            _vLayout.spacing = spacing_factor * _new_spacing;
            _old_spacing = _new_spacing;
        }

        if (Screen.height != _screen_height)
        {
            _vLayout.padding.top = Screen.height / 9;
        }
	}
}
