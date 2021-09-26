using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UDragEnhanceViewCopy : EventTrigger
{
    private EnhanceScrollViewCopy enhanceScrollView;
    public void SetScrollView(EnhanceScrollViewCopy view)
    {
        enhanceScrollView = view;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewMove(eventData.delta);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewEnd();
    }
}
