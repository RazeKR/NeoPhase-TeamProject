using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text text;

    private void Start()
    {
        if (text != null)
            text.canvasRenderer.SetAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text != null)
            text.CrossFadeAlpha(1.0f, 0.1f, false);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
            text.CrossFadeAlpha(0.0f, 0.1f, false);
    }
}