
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
    private UdonBehaviour toggleClient;
    [SerializeField]
    private string clientVariable = "toggleIndex";
    [SerializeField]
    private int toggleIndex = -1;
    [SerializeField, FieldChangeCallback(nameof(TogState))]
    private bool togState = false;
    [SerializeField]
    private bool reportedState = false;

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