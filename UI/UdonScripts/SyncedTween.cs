
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedTween : UdonSharpBehaviour
{
    [Header("Assignment during implementation")]
    [SerializeField] UdonBehaviour[] linkedBehaviors;
    [SerializeField] UdonBehaviour[] inverseBehaviors;
    [SerializeField] string linkedVariableName;

    [SerializeField, Tooltip("Animation Curve")]
    AnimationCurve tweenCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField]
    private Toggle stateToggle;
    [SerializeField]
    private Toggle offToggle;
    [SerializeField]
    float easing = 0.75f;

    
    [SerializeField, UdonSynced, FieldChangeCallback(nameof(SyncedState))]
    private bool syncedState = false;
    private bool isPlaying = false;
    [SerializeField]
    bool locallyOwned = true;
    private VRCPlayerApi localPlayer;

    private bool prevState;

    private void UpdateToggle()
    {
        if (offToggle == null && stateToggle != null)
        {
            stateToggle.SetIsOnWithoutNotify(syncedState);
            return;
        }
        if (syncedState)
        {
            if (stateToggle != null)
                stateToggle.SetIsOnWithoutNotify(true);
            return;
        }
        else
        {
            if (offToggle != null)
                offToggle.SetIsOnWithoutNotify(true);
        }
    }

    private bool SyncedState
    {
        get => syncedState;
        set
        {
            isPlaying |=  value != prevState;
            syncedState = value;
            prevState = value;
            UpdateToggle();
            RequestSerialization();
        }
    }

    public void setState(bool state)
    {
        SyncedState = state;
        UpdateToggle();
        animationTime = state ? 1 : 0;
        isPlaying = true;
    }

    void SendOutValues(float value)
    {
       
        foreach (UdonBehaviour receiver in linkedBehaviors)
        {
            receiver.SetProgramVariable<float>(linkedVariableName, value);
        }
        float inv = Mathf.Clamp01(1f-value);
        foreach (UdonBehaviour receiver in inverseBehaviors)
        {
            receiver.SetProgramVariable<float>(linkedVariableName, inv);
        }
    }

    float animationTime = 0;

    public void onToggle()
    {
        if (stateToggle == null)
            return;
        bool togVal = stateToggle.isOn;
        if (togVal != syncedState)
        {
            if (!locallyOwned)
                Networking.SetOwner(localPlayer, gameObject);
            SyncedState = togVal;
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        base.OnOwnershipTransferred(player);
        locallyOwned = player.isLocal;
    }

    private void Update()
    {
        if (!isPlaying)
            return;
        animationTime = Mathf.Clamp01(animationTime + (Time.deltaTime * (syncedState ? easing : -easing)));
        isPlaying = animationTime > 0 && animationTime < 1;
        SendOutValues(tweenCurve.Evaluate(animationTime));
    }

    private void Start()
    {
        if (stateToggle == null)
            stateToggle = GetComponent<Toggle>();
        if (string.IsNullOrEmpty(linkedVariableName))
            linkedVariableName = gameObject.name;
        localPlayer = Networking.LocalPlayer;
        locallyOwned = Networking.IsOwner(gameObject);
        if (stateToggle != null)
            SyncedState = stateToggle.isOn;
        else
            SyncedState = syncedState;
        animationTime = syncedState ? 1f : 0f;
    }
}
