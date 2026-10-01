
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
    [SerializeField,UdonSynced,FieldChangeCallback(nameof(ActiveIndex))]
    public int activeIndex = -1;
    [SerializeField]
    public string clientVariable = "activeToggle";

    [Header("Just here to see in inspector")]
    // No Fusion
    [SerializeField, FieldChangeCallback(nameof(ActiveValue))]
    public int activeValue = -1;
    [SerializeField]
    private bool interactable = true;
    [SerializeField]
    private bool showDebug = false;
    public bool ShowDebug
    {
        get => showDebug;
        set
        {
            foreach (UdonToggle tog in toggles)
            {
                if (tog != null)
                    tog.ShowDebug = value;
            }
        } 
    }
    /* 
    * Udon Sync Stuff
    */
    private bool iamOwner = false;
    public bool IsOwner
    {
        get => iamOwner;
    }

    private void ReviewOwnerShip()
    {
        iamOwner = Networking.IsOwner(this.gameObject);
    }
    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        ReviewOwnerShip();
    }
    public void onPointer()
    {
        if (showDebug)
            Debug.Log($"onPointer: iamOwner={iamOwner}");
        if (!iamOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void TogSet()
    {
        if (!iamOwner)
        { 
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (showDebug)
                Debug.Log($"Toggle set: Grabbed Ownership");
        }
        else
            if (showDebug)
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
                tog.SetState(false);
            }
        }
        for (int i = 0; i < numToggles; i++)
        {
            tog = toggles[i];
            if (tog == null)
                continue;
            if (toggleValues[i] == toggleValue && !tog.TogState)
            {
                tog.SetState(true);
            }
        }
    }

    public void SetActiveValue(int value)
    {
        ActiveValue = value;
        refreshToggles(value);
    }

    public int ActiveValue
    {
        get => activeValue;
        set
        {
            activeValue = value;
            if (toggleClient != null && !string.IsNullOrEmpty(clientVariable))
                toggleClient.SetProgramVariable(clientVariable, value);
            refreshToggles(value);
        }
    }
    public int ActiveIndex
    {
        get => activeIndex;
        set
        {
            if (value < -1 || value >= numToggles)
            {
                if (showDebug)
                    Debug.LogError($"ActiveIndex value {value} is out of range for toggle group with {numToggles} toggles.");
                return;
            }
            activeIndex = value;
            int togValue = toggleValues[activeIndex];
            if (!iamOwner)
                Networking.SetOwner(Networking.LocalPlayer,gameObject);
            if (showDebug)
                Debug.Log($"ActiveIndex set to {value}, which corresponds to toggle value {togValue}. ActiveValue {activeValue}.");
            ActiveValue = togValue;
            refreshToggles(togValue);
            RequestSerialization();
        }
    }

    private void OnEnable()
    {
        numToggles = toggles != null && toggles.Length > 0 ? toggles.Length : 0;
        if (numToggles > 0)
        {
            for (int i = 0; i < numToggles; i++)
            {
                UdonToggle Tog = toggles[i];
                if (Tog == null) continue;
                Tog.ToggleIndex = i;
                //Tog.ClientVariable = "activeIndex";
                //toggleValues[i] = Tog.ToggleValue;
                if (i==activeIndex && !Tog.TogState) 
                    Tog.SetState(true);
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
        if (toggleValues == null || toggleValues.Length != numToggles)
            toggleValues = new int[numToggles];
        ShowDebug = showDebug;
        OnEnable();
    }
#endif

    public void Start()
    {
        //player = Networking.LocalPlayer;
        ReviewOwnerShip();
        if (iamOwner)
        {
            ActiveValue = activeValue;
        }
    }

        // Update is called once per frame
}

