using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class BringToFront : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Use this for initialization
    void Start()
    {
        transform.SetAsLastSibling();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameData.MouseBlocked = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameData.MouseBlocked = false;
    }

    


}
