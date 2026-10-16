
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BoingBoing : UdonSharpBehaviour
{
    [SerializeField, Range(1, 12)]
    int maxJumps = 5;
    [SerializeField, Range(1f, 10f)]
    float midAirJumpVelocity = 5f;
    [SerializeField]
    private int _jumpCount = 0;
    [SerializeField]
    bool showDebug = false;
    bool iAmOwner = false;
    private VRCPlayerApi _localPlayer;

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        // Only trigger on the initial button press down
        if (!value || _localPlayer == null) return;

        if (_localPlayer.IsPlayerGrounded())
        {
            // Reset counter when jumping normally from the ground
            _jumpCount = 1;
        }
        else
        {
            // Player is in mid-air and wants to jump again
            if (_jumpCount < maxJumps)
            {
                _jumpCount++;

                // Get current velocity to maintain horizontal momentum
                Vector3 currentVelocity = _localPlayer.GetVelocity();

                // Apply a fresh vertical boost while keeping X and Z velocity intact
                _localPlayer.SetVelocity(new Vector3(currentVelocity.x, midAirJumpVelocity, currentVelocity.z));
            }
        }
    }

    void Start()
    {
        _localPlayer = Networking.LocalPlayer;

    }
}
