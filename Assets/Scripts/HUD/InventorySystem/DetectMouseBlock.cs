using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class DetectMouseBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameData.MouseBlocked = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameData.MouseBlocked = false;
    }
}
