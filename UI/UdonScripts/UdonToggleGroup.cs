
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonToggleGroup : UdonSharpBehaviour
{
    [SerializeField] UdonToggle[] toggles;
    [SerializeField] int[] toggleValues;
    [SerializeField] private int numToggles = 0;
    [SerializeField]
    private UdonBehaviour toggleClient;
    [SerializeField,FieldChangeCallback(nameof(ActiveIndex))]
    public int activeIndex = -1;
    [SerializeField]
    private string clientVariable = "activeToggle";

    [Header("Just here to see in inspector")]
    // No Fusion
    [SerializeField, UdonSynced, FieldChangeCallback(nameof(ActiveValue))]
    public int activeValue = -1;
    //[SerializeField]
    //private bool debug = false;
    [SerializeField]
    private bool interactable = true;
    private bool iamOwner = false;

    /* 
* Udon Sync Stuff
*/
    private void ReviewOwnerShip()
    {
        iamOwner = Networking.IsOwner(this.gameObject);
    }
    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        ReviewOwnerShip();
    }

    public void TogSet()
    {
        if (!iamOwner)
        { 
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Debug.Log($"Toggle set: Grabbed Ownership");
        }
        else
            Debug.Log($"Toggle set: Already Owner");
    }
    public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                for (int i = 0; i < numToggles; i++)
                {
                    if (toggles[i] == null)
                        continue;
                    toggles[i].Interactable = interactable;
                }
            }
        }

    private void refreshToggles(int toggleValue)
    {
        if (numToggles <= 0)
            return;
        UdonToggle tog;
        for (int i = 0; i < numToggles; i++)
        {
            tog = toggles[i];
            if (tog == null)
                continue;
            if (toggleValues[i] != toggleValue && tog.TogState)
            {
                tog.setState(false);
            }
        }
        for (int i = 0; i < numToggles; i++)
        {
            tog = toggles[i];
            if (tog == null)
                continue;
            if (toggleValues[i] == toggleValue && !tog.TogState)
            {
                tog.setState(true);
            }
        }
    }

    public void SetActiveValue(int value)
    {
        ActiveValue = value;
        refreshToggles(value);
    }

    private bool valueChanged = false;
    public int ActiveValue
    {
        get => activeValue;
        set
        {
            valueChanged |= value != activeValue;
            activeValue = value;
            if (valueChanged)
            {
                if (toggleClient != null && !string.IsNullOrEmpty(clientVariable))
                    toggleClient.SetProgramVariable(clientVariable, value);
                refreshToggles(value);
            }
            RequestSerialization();
        }
    }
    public int ActiveIndex
    {
        get => activeIndex;
        set
        {
            if (value < -1 || value >= numToggles)
            {
                //if (debug)
                //    Debug.LogError($"ActiveIndex value {value} is out of range for toggle group with {numToggles} toggles.");
                return;
            }
            int togValue = toggleValues[value];
            bool valueChanged = togValue != activeValue;
            if (!iamOwner)
                Networking.SetOwner(Networking.LocalPlayer,gameObject);
            activeIndex = value;
            //if (debug)
            //    Debug.Log($"ActiveIndex set to {value}, which corresponds to toggle value {togValue}. ActiveValue {activeValue}.");
            ActiveValue = togValue;
            refreshToggles(togValue);
        }
    }

    private void OnEnable()
    {
        numToggles = toggles != null && toggles.Length > 0 ? toggles.Length : 0;
        if (numToggles > 0)
        {
            toggleValues = new int[numToggles];
            for (int i = 0; i < numToggles; i++)
            {
                UdonToggle Tog = toggles[i];
                if (Tog == null) continue;
                Tog.ToggleIndex = i;
                Tog.clientVariable = "activeIndex";
                toggleValues[i] = Tog.ToggleValue;
                if (i==activeIndex && !Tog.TogState) 
                    Tog.setState(true);
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (toggles == null || toggles.Length == 0)
            toggles = GetComponentsInChildren<UdonToggle>();
        numToggles = 0;
        if ((toggles != null) &&  (toggles.Length > 0))
            numToggles = toggles.Length;
        OnEnable();
    }
#endif

    public void Start()
    {
        //player = Networking.LocalPlayer;
        ReviewOwnerShip();
        if (iamOwner)
        {
            valueChanged = true;
            ActiveValue = activeValue;
        }
    }

        // Update is called once per frame
}

