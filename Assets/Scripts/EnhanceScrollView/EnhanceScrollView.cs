using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class EnhanceScrollView : MonoBehaviour
{
    // Control the item's scale curve
    public AnimationCurve scaleCurve;
    // Control the position curve
    public AnimationCurve positionCurve;
    // Control the "depth"'s curve(In 3d version just the Z value, in 2D UI you can use the depth(NGUI))
    // NOTE:
    // 1. In NGUI set the widget's depth may cause performance problem
    // 2. If you use 3D UI just set the Item's Z position
    public AnimationCurve depthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    // The start center index
    [Tooltip("The Start center index")]
    public int startCenterIndex = 0;
    // Offset width between item
    public float cellWidth = 10f;
    private float totalVerticalHeight = 500.0f;
    // vertical fixed position value 
    public float xFixedPositionValue = 46.0f;

    // Lerp duration
    public float lerpDuration = 0.2f;
    private float mCurrentDuration = 0.0f;
    private int mCenterIndex = 0;
    public bool enableLerpTween = true;
    // center and preCentered item
    private EnhanceItem curCenterItem;
    private EnhanceItem preCenterItem;

    // if we can change the target item
    private bool canChangeItem = true;
    private float dFactor = 0.2f;

    // originHorizontalValue Lerp to horizontalTargetValue
    private float originVerticalValue = 0.1f;
    public float curVerticalValue = 0.5f;

    // "depth" factor (2d widget depth or 3d Z value)
    private int depthFactor = 5;

    public int maxNumber = 9;
    private float lastHorizontallValue = 0;
    private EnhanceScrollViewDragController dragController;

    public void EnableDrag(bool isEnabled)
    {
        if (dragController != null)
            dragController.enabled = false;
    }

    // targets enhance item in scroll view
    public List<EnhanceItem> listEnhanceItems;
    // sort to get right index
    private List<EnhanceItem> listSortedItems = new List<EnhanceItem>();

    public float maxVerticalValue = 0;
    public float minVerticalValue = 0;
    public int currIndex = 0;

    void Start()
    {
        canChangeItem = false;
        int count = listEnhanceItems.Count;
        dFactor = (Mathf.RoundToInt((1f / count) * 10000f)) * 0.0001f;
        mCenterIndex = count / 2;
        if (count % 2 == 0)
            mCenterIndex = count / 2 - 1;
        int index = 0;
        for (int i = count - 1; i >= 0; i--)
        {
            listEnhanceItems[i].CurveOffSetIndex = i;
            listEnhanceItems[i].CenterOffSet = dFactor * (mCenterIndex - index);
            listEnhanceItems[i].SetSelectState(false);
            GameObject obj = listEnhanceItems[i].gameObject;
            UDragEnhanceView script = obj.GetComponent<UDragEnhanceView>();
            if (script != null)
                script.SetScrollView(this);
            index++;
        }

        // set the center item with startCenterIndex
        if (startCenterIndex < 0 || startCenterIndex >= count)
        {
            Debug.LogError("## startCenterIndex < 0 || startCenterIndex >= listEnhanceItems.Count  out of index ##");
            startCenterIndex = mCenterIndex;
        }

        // sorted items
        listSortedItems = new List<EnhanceItem>(listEnhanceItems.ToArray());
        totalVerticalHeight = cellWidth * count;
        curCenterItem = listEnhanceItems[startCenterIndex];
        curVerticalValue = 0.5f - curCenterItem.CenterOffSet;
        minVerticalValue = curVerticalValue;
        maxVerticalValue = curVerticalValue + (maxNumber - 1) * dFactor;
        currIndex = startCenterIndex;
        LerpTweenToTarget(0f, curVerticalValue, false);
        // 
        // enable the drag actions
        // 
        EnableDrag(true);
    }

    private void LerpTweenToTarget(float originValue, float targetValue, bool needTween = false)
    {
        if (!needTween)
        {
            SortEnhanceItem();
            originVerticalValue = targetValue;
            UpdateEnhanceScrollView(targetValue);
            this.OnTweenOver();
        }
        else
        {
            originVerticalValue = originValue;
            curVerticalValue = targetValue;
            mCurrentDuration = 0.0f;
        }
        enableLerpTween = needTween;
        
    }

    public void DisableLerpTween()
    {
        this.enableLerpTween = false;
    }

    /// 
    /// Update EnhanceItem state with curve fTime value
    /// 
    public void UpdateEnhanceScrollView(float fValue)
    {
        for (int i = 0; i < listEnhanceItems.Count; i++)
        {
            EnhanceItem itemScript = listEnhanceItems[i];
            float yValue = GetYPosValue(fValue, itemScript.CenterOffSet);
            float scaleValue = GetScaleValue(fValue, itemScript.CenterOffSet);
            float depthCurveValue = depthCurve.Evaluate(fValue + itemScript.CenterOffSet);
            itemScript.UpdateScrollViewItems(xFixedPositionValue, depthCurveValue, depthFactor, listEnhanceItems.Count, yValue, scaleValue, scaleValue);
        }
        int currIndex = (int) ((fValue - minVerticalValue) / dFactor + 0.5f);
        int index = 0;
        foreach (var item in listSortedItems)
        {
            if (index == 0)
            {
                item.dataNumber = currIndex + 1;
            }else if (index == 1)
            {
                item.dataNumber = currIndex;
            }
            else
            {
                item.dataNumber = currIndex - 1;
            }
            index++;
        }
    }

    void Update()
    {
        if (enableLerpTween)
            TweenViewToTarget();
    }

    private void TweenViewToTarget()
    {
        mCurrentDuration += Time.deltaTime;
        if (mCurrentDuration > lerpDuration)
            mCurrentDuration = lerpDuration;

        float percent = mCurrentDuration / lerpDuration;
        float value = Mathf.Lerp(originVerticalValue, curVerticalValue, percent);
        UpdateEnhanceScrollView(value);
        if (mCurrentDuration >= lerpDuration)
        {
            canChangeItem = true;
            enableLerpTween = false;
            
            OnTweenOver();
        }
    }

    private void OnTweenOver()
    {
        if (preCenterItem != null)
            preCenterItem.SetSelectState(false);
        if (curCenterItem != null)
            curCenterItem.SetSelectState(true);
        
        
    }

    // Get the evaluate value to set item's scale
    private float GetScaleValue(float sliderValue, float added)
    {
        float scaleValue = scaleCurve.Evaluate(sliderValue + added);
        return scaleValue;
    }

    // Get the X value set the Item's position
    private float GetYPosValue(float sliderValue, float added)
    {
        float evaluateValue = positionCurve.Evaluate(sliderValue + added) * totalVerticalHeight;
        return evaluateValue;
    }

    private int GetMoveCurveFactorCount(EnhanceItem preCenterItem, EnhanceItem newCenterItem)
    {
        SortEnhanceItem();
        int factorCount = Mathf.Abs(newCenterItem.RealIndex) - Mathf.Abs(preCenterItem.RealIndex);
        return Mathf.Abs(factorCount);
    }

    // sort item with X so we can know how much distance we need to move the timeLine(curve time line)
    static public int SortPosition(EnhanceItem a, EnhanceItem b) { return a.transform.localPosition.y.CompareTo(b.transform.localPosition.y); }
    private void SortEnhanceItem()
    {
        listSortedItems.Sort(SortPosition);
        for (int i = listSortedItems.Count - 1; i >= 0; i--)
            listSortedItems[i].RealIndex = i;
    }
    
    public float factor = 0.001f;
    // On Drag Move
    public void OnDragEnhanceViewMove(Vector2 delta)
    {
        
        if (maxVerticalValue - lastHorizontallValue < 0.1f && delta.y > 0)
        {
            return;
        }else if (lastHorizontallValue - minVerticalValue < 0.1f && delta.y < 0)
        {
            return;
        }
      
        
        
        // In developing
        if (Mathf.Abs(delta.y) > 0.0f)
        {
            curVerticalValue += delta.y * factor;
            curVerticalValue = Mathf.Min(maxVerticalValue, curVerticalValue);
            curVerticalValue = Mathf.Max(curVerticalValue, minVerticalValue);
           
           
            LerpTweenToTarget(0.0f, curVerticalValue, false);
        }
    }

    // On Drag End
    public void OnDragEnhanceViewEnd(Vector2 delta)
    {
        // find closed item to be centered
        int closestIndex = 0;
        float value = (curVerticalValue - (int)curVerticalValue);
        float min = float.MaxValue;
        float tmp = 0.5f * (curVerticalValue < 0 ? -1 : 1);
        for (int i = 0; i < listEnhanceItems.Count; i++)
        {
            float dis = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs((tmp - listEnhanceItems[i].CenterOffSet)));
            if (dis < min)
            {
                closestIndex = i;
                min = dis;
            }
        }
        
        originVerticalValue = curVerticalValue;
        float target = ((int)curVerticalValue + (tmp - listEnhanceItems[closestIndex].CenterOffSet));
        preCenterItem = curCenterItem;
        curCenterItem = listEnhanceItems[closestIndex];
        LerpTweenToTarget(originVerticalValue, target, true);
        canChangeItem = false;
    }

    public void OnDragEnhanceViewBegin()
    {
        
        lastHorizontallValue = curVerticalValue;
    }
}