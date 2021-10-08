using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UDragEnhanceView : MonoBehaviour, IBeginDragHandler, IEndDragHandler,IDragHandler
{
    private EnhanceScrollView enhanceScrollView;
    public void SetScrollView(EnhanceScrollView view)
    {
        enhanceScrollView = view;
    }


    public  void OnBeginDrag(PointerEventData eventData)
    {
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewBegin();
    }

    public  void OnDrag(PointerEventData eventData)
    {
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewMove(eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewEnd(eventData.delta);
    }
}
