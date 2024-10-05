
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TweenSelect : UdonSharpBehaviour
{
    [Header("Assignment during implementation")]
    [SerializeField] UdonBehaviour[] stateOneClients;
    [SerializeField] UdonBehaviour[] stateTwoClients;
    [SerializeField] string linkedVariableName;

    [SerializeField, Tooltip("Animation Curve")]
    AnimationCurve tweenCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField]
    private Toggle togBoth;
    [SerializeField]
    private Toggle togOne;
    [SerializeField]
    private Toggle togTwo;
    [SerializeField]
    float easing = 0.75f;


    [SerializeField, UdonSynced, FieldChangeCallback(nameof(SelectState))]
    private int selectState = 0;

    [SerializeField]
    private bool isPlayingOne = false;
    [SerializeField]
    private bool isPlayingTwo = false;

    [SerializeField]
    private bool stateOne = false;
    [SerializeField]
    private bool stateTwo = false;
    [SerializeField]
    private bool bothStates = false;

    bool locallyOwned = true;
    private VRCPlayerApi localPlayer;

    private void updateState()
    {
        bool oneState = (selectState == 1) || (selectState == 3);
        isPlayingOne |= oneState != stateOne;
        stateOne = oneState;
        bool twoState = (selectState == 2) || (selectState == 3);
        isPlayingTwo |= twoState != stateTwo;
        stateTwo = twoState;
        bothStates = selectState == 3;
    }

    private void UpdateButtons()
    {
        if (togBoth != null && bothStates)
        {
            togBoth.SetIsOnWithoutNotify(bothStates);
            return;
        }
        if (togOne != null && stateOne)
            togOne.SetIsOnWithoutNotify(stateOne);
        if (togTwo != null && stateTwo)
            togTwo.SetIsOnWithoutNotify(stateTwo);
    }

    private int SelectState
    {
        get => selectState;
        set
        {
            value = value > 3 ? 3 : value;
            selectState = value;
            updateState();
            if (!locallyOwned)
                UpdateButtons();
            RequestSerialization();
        }
    }

    void SendOutValuesOne(float value)
    {
        if (stateOneClients == null || stateOneClients.Length <= 0)
            return;
        foreach (UdonBehaviour receiver in stateOneClients)
        {
            receiver.SetProgramVariable<float>(linkedVariableName, value);
        }
    }

    void SendOutValuesTwo(float value)
    {
        if (stateTwoClients == null || stateTwoClients.Length <= 0)
            return;
        foreach (UdonBehaviour receiver in stateTwoClients)
        {
            receiver.SetProgramVariable<float>(linkedVariableName, value);
        }
    }

    float animTimeOne = 0;
    float animTimeTwo = 0;

    public void onTogOne()
    {
        if (togOne == null || !togOne.isOn || selectState == 1)
            return;
        if (!locallyOwned)
            Networking.SetOwner(localPlayer, gameObject);
        SelectState = 1;
    }
    public void onTogTwo()
    {
        if (togTwo == null || !togTwo.isOn || selectState == 2)
            return;
        if (!locallyOwned)
            Networking.SetOwner(localPlayer, gameObject);
        SelectState = 2;
    }

    public void onTogBoth()
    {
        if (togBoth == null || !togBoth.isOn || selectState == 3)
            return;
        if (!locallyOwned)
            Networking.SetOwner(localPlayer, gameObject);
        SelectState = 3;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        base.OnOwnershipTransferred(player);
        locallyOwned = player.isLocal;
    }

    private void Update()
    {
        if (isPlayingOne)
        {
            animTimeOne = Mathf.Clamp01(animTimeOne + (Time.deltaTime * (stateOne ? easing : -easing)));
            isPlayingOne = animTimeOne > 0 && animTimeOne < 1;
            SendOutValuesOne(tweenCurve.Evaluate(animTimeOne));
        }
        if (isPlayingTwo)
        {
            animTimeTwo = Mathf.Clamp01(animTimeTwo + (Time.deltaTime * (stateTwo ? easing : -easing)));
            isPlayingTwo = animTimeTwo > 0 && animTimeTwo < 1;
            SendOutValuesTwo(tweenCurve.Evaluate(animTimeTwo));
        }
    }

    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
        locallyOwned = Networking.IsOwner(gameObject);
        UpdateButtons();
        SelectState = selectState;
        animTimeOne = stateOne ? 1f : 0f;
        animTimeTwo = stateTwo ? 1f : 0f;
    }
}
