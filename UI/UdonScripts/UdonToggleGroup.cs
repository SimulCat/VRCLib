
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(ToggleGroup))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonToggleGroup : UdonSharpBehaviour
{
    [SerializeField] ToggleGroup togGroup;
    [SerializeField] UdonToggle[] toggles;
    private int numToggles = 0;
    [SerializeField]
    private UdonBehaviour toggleClient;
    [SerializeField]
    private string clientVariable = "activeToggle";

    [Header("Just here to see in inspector")]
    // No Fusion
    [SerializeField, FieldChangeCallback(nameof(ActiveToggle))]
    public int activeToggle = -1;
    [SerializeField]
    private bool debug = false;
    [SerializeField]
    private bool interactable = true;
        
    public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                for (int i = 0; i < toggles.Length; i++)
                {
                    if (toggles[i] == null)
                        continue;
                    toggles[i].Interactable = interactable;
                }
            }
        }
        public void SetActiveToggle(int toggleIndex = -1)
        {
            if (debug)
                Debug.Log($"SetActiveToggle called with index {toggleIndex}");
            foreach (UdonToggle tog in toggles)
            {
                if (tog == null)
                    continue;
                if (tog.ToggleIndex == toggleIndex && tog.TogState == false)
                    tog.setState(true);
            }
            ActiveToggle = toggleIndex;
        }

        public int ActiveToggle
        {
            get => activeToggle;
            set
            {
            if (debug)
                Debug.Log($"ActiveToggle set to {value}");
            if (toggleClient != null && !string.IsNullOrEmpty(clientVariable))
                toggleClient.SetProgramVariable(clientVariable, value);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (togGroup == null)
            togGroup = GetComponent<ToggleGroup>();
        if (toggles == null || toggles.Length == 0)
            toggles = GetComponentsInChildren<UdonToggle>();
        numToggles = 0;
        if ((toggles != null) &&  (toggles.Length > 0))
            numToggles = toggles.Length;
        foreach (UdonToggle UdonTog in toggles)
        {
            if (UdonTog != null)
            {
                UdonTog.toggleClient = GetComponent<UdonBehaviour>();
            }
        }
    }
#endif
    void Awake()
        {
            togGroup = GetComponent<ToggleGroup>();
            if (togGroup == null)
                togGroup = GetComponent<ToggleGroup>();
        }

        public void Start()
        {
        }

        // Update is called once per frame
    }

