﻿using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)] // Keeps performance up
public class SyncedSlider : UdonSharpBehaviour
{
    [SerializeField]    
    private Slider slider;
    [SerializeField]
    private TextMeshProUGUI sliderLabel;
    [SerializeField]
    private TextMeshProUGUI sliderTitle;
    [SerializeField]
    private bool hideLabel = false;
    [SerializeField]
    private bool unitsInteger = false;
    [SerializeField]
    private bool displayInteger = false;
    [SerializeField]
    private string sliderUnit;
    [SerializeField]
    private float unitDisplayScale = 1;
    [SerializeField]
    private float sliderScale = 1.0f;
    [SerializeField]
    UdonBehaviour SliderClient;
    [SerializeField]
    string clientVariableName = "SliderValueVar";
    [SerializeField]
    string clientPointerStateVar = "PointerStateVar";
    [SerializeField]
    private float currentValue;
    [SerializeField]
    private float maxValue = 1;
    [SerializeField]
    private float minValue = 0;

    private float reportedValue = 0.0f;
    private bool isInteractible;
    private bool isInitialized = false;
    public bool IsInteractible
    {
        get {
            if (slider != null) 
                isInteractible = slider.interactable;
            return isInteractible;
            } 
        set 
        { 
            isInteractible = value;
            if (slider!= null ) 
                slider.interactable = value; 
        }
    }

    public float UnitDisplayScale 
    { 
        get => unitDisplayScale; 
        set 
        {
            if (unitDisplayScale != value)
            {
                unitDisplayScale = value;
            }
        } 
    }
    public void SetValues(float value, float min, float max)
    {
        minValue= min;
        maxValue= max;
        reportedValue = value;
        if (slider != null)
        {
            slider.minValue= minValue/sliderScale;
            slider.maxValue= maxValue/sliderScale;
            slider.value = value / sliderScale;
        }
        currentValue = value;
    }
    public string TitleText
    {
        get 
        { 
            if (sliderTitle == null)
                return "";
            return sliderTitle.text;
        }
        set 
        { 
            sliderTitle.text = value; 
        }
    }
    public string SliderUnit
    {
        get => sliderUnit;
        set
        {
            sliderUnit = value;
            CurrentValue = currentValue;
        }
    }
    public float CurrentValue { 
        get => currentValue;
        set
        {
            currentValue = value;
            if (slider != null)
            {
                float sliderValue = currentValue / sliderScale;
                if (slider.value != sliderValue)
                    slider.value = sliderValue;
                if (sliderLabel != null)
                {

                    if (!hideLabel)
                    {
                        float displayValue = currentValue * unitDisplayScale;
                        if (displayInteger)
                            displayValue = Mathf.RoundToInt(displayValue);
                        if (unitsInteger || displayInteger)
                            sliderLabel.text = string.Format("{0}{1}", (int)displayValue, sliderUnit);
                        else
                            sliderLabel.text = string.Format("{0:0.0}{1}", displayValue, sliderUnit);
                    }
                    else
                    {
                        sliderLabel.text = "";
                    }
                }
            }
            if (reportedValue != currentValue)
            {
                reportedValue = currentValue;
                if (iHaveClientVar)
                {
                    if (unitsInteger)
                        SliderClient.SetProgramVariable<int>(clientVariableName, Mathf.RoundToInt(currentValue));
                    else
                        SliderClient.SetProgramVariable<Single>(clientVariableName, currentValue);
                }
            }
        }
    }
    public float MaxValue
    {
        get => maxValue;
        set 
        { 
            if (maxValue != value) 
            { 
                maxValue = value;
                if (slider != null)
                    slider.maxValue= maxValue/sliderScale;
            }
        }
    }

    public float MinValue
    {
        get => minValue;
        set
        {
            if (minValue != value)
            {
                minValue = value;
                if (slider != null)
                    slider.minValue = minValue/sliderScale;
            }
        }
    }

    public void SliderValueChange()
    {
        if (slider != null)
        {
            CurrentValue = slider.value * sliderScale;
        }
    }

    public void OnPointerDown()
    {
        if (iHaveClientPtr)
            SliderClient.SetProgramVariable<bool>(clientPointerStateVar, true);
    }
    public void OnPointerUp()
    {
        if (iHaveClientPtr)
            SliderClient.SetProgramVariable<bool>(clientPointerStateVar, false);
    }
    private bool iHaveClientVar = false;
    private bool iHaveClientPtr = false;
    public void Start()
    {
        iHaveClientVar = (SliderClient != null) && (!string.IsNullOrEmpty(clientVariableName));
        iHaveClientPtr = (SliderClient != null) && (!string.IsNullOrEmpty(clientPointerStateVar));
        if (sliderLabel == null)
            hideLabel = true;
        if (slider != null)
        {
            isInteractible = slider.interactable;
            if (!isInitialized)
            {
                isInitialized = true;
            }
        }
        //CurrentValue = currentValue;
    }
}
