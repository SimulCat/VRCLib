
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Toggle))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedToggle : UdonSharpBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private UdonBehaviour toggleClient;
    [SerializeField]
    private string clientVariable = "toggleIndex";
    [SerializeField]
    private int toggleIndex = -1;
    [SerializeField,UdonSynced,FieldChangeCallback(nameof(SyncedState))]
    private bool syncedState = false;
    [SerializeField]
    private bool reportedState = false;
    private VRCPlayerApi player;
    private bool locallyOwned = false;

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        locallyOwned = Networking.IsOwner(this.gameObject);
    }

    public bool SyncedState
    {
        get 
        { 
            return syncedState; 
        }
        set 
        {
            syncedState = value;
            if (toggle != null && toggle.isOn != value)
                toggle.SetIsOnWithoutNotify(value);
            if (toggleClient != null && !string.IsNullOrEmpty(clientVariable))
            {
                if (reportedState != syncedState)
                {
                    if (toggleIndex < 0)
                        toggleClient.SetProgramVariable<bool>(clientVariable, syncedState);
                    else
                    {
                        if (syncedState)
                            toggleClient.SetProgramVariable<int>(clientVariable, toggleIndex);
                    }
                }

            }
            reportedState = value;
            RequestSerialization();
        }
    }
    public void setState(bool state = false)
    {
        syncedState = state;
        reportedState = state;
        if (toggle != null)
        {
            if (toggle.isOn != state)
                toggle.SetIsOnWithoutNotify(state);
        }
    }
    public void onToggle()
    {
        if (!locallyOwned)
            Networking.SetOwner(player, gameObject);
        SyncedState = toggle.isOn;
    }
    void Start()
    {
        player = Networking.LocalPlayer;
        locallyOwned = Networking.IsOwner(gameObject);

        if (toggle == null)
            toggle = GetComponent<Toggle>();
        reportedState = !toggle.isOn;
        SyncedState = !reportedState;
    }
}
