
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedIncDec : UdonSharpBehaviour
{
    [SerializeField]
    UdonBehaviour UdonClient;
    [SerializeField] public string clientVariableName = "Value";

    [SerializeField] Button IncButton;
    [SerializeField] Button DecButton;
    [SerializeField] TextMeshProUGUI ValueText;
    [SerializeField] int value = 0;
    [SerializeField] int minValue = 0;
    [SerializeField] int maxValue = 10;
    private bool debug = false;
    /* 
    * Udon Sync Stuff
    */
    private bool iamOwner = false;


    private void ReviewOwnerShip()
    {
        iamOwner = Networking.IsOwner(this.gameObject);
    }
    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        ReviewOwnerShip();
    }

    private int reportedValue = -1;
    public int Value
    {
        get => value;
        set
        {
            int newValue = Mathf.Clamp(value, minValue, maxValue);
            if (this.value != newValue)
            {
                this.value = newValue;

                if (debug)
                    Debug.Log($"{gameObject.name}:IncDec set to {newValue}");
            }
            if (ValueText != null)
                ValueText.text = $"{newValue}";
            if (reportedValue != newValue)
            {
                reportedValue = newValue;
                if (UdonClient != null)
                    UdonClient.SetProgramVariable(clientVariableName, newValue);
            }
            RequestSerialization();
        }

    }

    public void SetValueWithoutNotification(int newValue)
    {
        reportedValue = newValue;
        Value = newValue;
    }

    public void SetLimits(int min, int max)
    {
        minValue = min;
        maxValue = max;
        SetValueWithoutNotification(Mathf.Clamp(value, minValue, maxValue));
    }
    public void incValue()
    {
        int incValue = Mathf.Clamp(value + 1, minValue, maxValue);
        if (!iamOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        if (value != incValue)
        {
            Value = incValue;
            if (debug)
                Debug.Log($"{gameObject.name}:IncDec + to {incValue}");
        }
    }

    public void onPointer()
    {
        if (!iamOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void decValue()
    {
        if (!iamOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        int decValue = Mathf.Clamp(value - 1, minValue, maxValue);
        if (value != decValue)
        {
            Value = decValue;
            if (debug)
                Debug.Log($"{gameObject.name}:IncDec - to {decValue}");
        }
    }
    void OnEnable()
    {
    }

    void Start()
    {
        ValueText.text = $"{value}";
    }
}
