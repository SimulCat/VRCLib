
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Toggle))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class UdonToggle : UdonSharpBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    public UdonBehaviour toggleClient;
    [SerializeField]
    public string clientVariable = "toggleIndex";
    [SerializeField]
    private int toggleIndex = -1;
    [SerializeField]
    private int toggleValue = -1;
    [SerializeField, FieldChangeCallback(nameof(TogState))]
    private bool togState = false;
    [SerializeField]
    private bool reportedState = false;
    private bool enabled = false;
    public int ToggleIndex
    {
        get => toggleIndex;
        set => toggleIndex = value;
    }

    public int ToggleValue
    {
        get => toggleValue;
        set => toggleValue = value;
    }

    public bool TogState
    {
        get
        {
            return togState;
        }
        set
        {
            togState = value;
            if (toggleClient != null && !string.IsNullOrEmpty(clientVariable))
            {
                if (reportedState != togState)
                {
                    if (toggleIndex < 0)
                        toggleClient.SetProgramVariable<bool>(clientVariable, togState);
                    else
                    {
                        if (togState)
                            toggleClient.SetProgramVariable<int>(clientVariable, ToggleIndex);
                    }
                }

            }
            reportedState = value;
        }
    }

    public bool Interactable
    {
        get => toggle.interactable;
        set => toggle.interactable = value;
    }
    public void setState(bool state)
    {
        togState = state;
        reportedState = state;
        if (enabled && toggle != null)
        {
            if (toggle.isOn != state)
                toggle.SetIsOnWithoutNotify(state);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if ( toggle == null)
        {
            toggle = GetComponent<Toggle>();
        }
    }
#endif
    public void OnEnable() 
    { 
        if ( toggle == null)   
        {
            toggle = GetComponent<Toggle>();
        }
        toggle.isOn = togState;
        reportedState = togState;
        enabled = true;
    }

    public void onToggle()
    {
        TogState = toggle.isOn;
        if (togState && toggleClient != null)
        { 
            toggleClient.SendCustomEvent("TogSet");
        }
    }

}