
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Toggle))]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class UdonToggle : UdonSharpBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    public UdonBehaviour toggleClient;
    [SerializeField]
    private string clientVariable = "toggleIndex";
    public string ClientVariable
    {
        get => clientVariable;
        set => clientVariable = value;
    }
    [SerializeField]
    private int toggleIndex = -1;
    [SerializeField, FieldChangeCallback(nameof(TogState))]
    private bool togState = false;
    [SerializeField]
    private bool showDebug = false;
    public bool ShowDebug
    {
        get => showDebug;
        set => showDebug = value;
    }
    private bool isEnabled = false;
    public int ToggleIndex
    {
        get => toggleIndex;
        set => toggleIndex = value;
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
                if (toggleIndex < 0)
                    toggleClient.SetProgramVariable<bool>(clientVariable, togState);
                else
                {
                    if (togState)
                        toggleClient.SetProgramVariable<int>(clientVariable, ToggleIndex);
                }

            }
        }
    }

    public bool Interactable
    {
        get => toggle.interactable;
        set => toggle.interactable = value;
    }
    public void SetState(bool state)
    {
        togState = state;
        if (isEnabled && toggle != null)
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
            toggle = GetComponent<Toggle>();
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(togState);
        isEnabled = true;
    }

    public void onToggle()
    {
        TogState = toggle.isOn;
        if (showDebug)
            Debug.Log($"{gameObject.name}: onToggle: TogState={TogState}, toggleIndex={toggleIndex}");
        if (togState && toggleClient != null)
        { 
            toggleClient.SendCustomEvent("TogSet");
        }
    }

}