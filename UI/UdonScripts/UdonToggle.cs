
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
    private string clientVariable = "toggleIndex";
    [SerializeField]
    private int toggleIndex = -1;
    [SerializeField, FieldChangeCallback(nameof(TogState))]
    private bool togState = false;
    [SerializeField]
    private bool reportedState = false;

    public int ToggleIndex
    {
        get => toggleIndex;
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
                            toggleClient.SetProgramVariable<int>(clientVariable, toggleIndex);
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
    public void setState(bool state = false)
    {
        togState = state;
        reportedState = state;
        if (toggle != null)
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
    public void onToggle()
    {
        TogState = toggle.isOn;
    }
    void Start()
    {

        if (toggle == null)
            toggle = GetComponent<Toggle>();
        reportedState = !toggle.isOn;
        TogState = !reportedState;
    }
}