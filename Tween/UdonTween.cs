
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class UdonTween : UdonSharpBehaviour
{
    [SerializeField]
    AnimationCurve m_moveCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField]
    private Toggle stateToggle;
    [SerializeField]
    UdonBehaviour TweenClient;
    [SerializeField]
    string clientVariableName = "tweenVariable";
    [SerializeField]
    float damping = 0.1f;

    public float tweenValue;
    private float reportedValue = 0.0f;
    [SerializeField]
    private bool currentState = false;
    float animationTime = 0;
    public float TweenValue
    {
        get => tweenValue;
        set
        {
            tweenValue = value;
            if (reportedValue != tweenValue)
            {
                reportedValue = tweenValue;
                if ((TweenClient != null) && (!string.IsNullOrEmpty(clientVariableName)))
                    TweenClient.SetProgramVariable<Single>(clientVariableName, tweenValue);
            }
        }
    }

    private bool CurrentState 
    { 
        get => currentState;
        set
        {
            if (value != currentState)
            {
                currentState = value;
                if (currentState)
                    animationTime = Mathf.Clamp(tweenValue, 0, 1);
                else
                    animationTime = Mathf.Clamp(1 - tweenValue, 0, 1);
            }
        }
    }
    public void onToggleChanged()
    {
        bool togVal = CurrentState;
        if (stateToggle != null)
        {
            togVal = stateToggle.isOn;
        }
        else
            togVal = !togVal;
        CurrentState = togVal;

    }
    public void setOffState()
    {
        CurrentState = false;
    }

    public void setOnState()
    {
        CurrentState = true;
    }
    private void Update()
    {
        if (animationTime < 1)
        {
            animationTime += Time.deltaTime * damping;
        }
        if (currentState)
        {
            if (tweenValue < 1)
                TweenValue = Mathf.Lerp(0, 1, m_moveCurve.Evaluate(animationTime));
        }
        else
        {
            if (tweenValue > 0)
                TweenValue = Mathf.Lerp(1, 0, m_moveCurve.Evaluate(animationTime));
        }
    }
    /*
    public void SetValues(float value, float min, float max)
    {
        tweenValue = value;
        reportedValue = value;
    }*/
    private void Start()
    {
        onToggleChanged();
    }
}
