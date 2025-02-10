
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonTransfer : UdonSharpBehaviour
{ 
    [SerializeField]
    Transform[] objects;
    [SerializeField]
    Vector3[] targetOffsets;

    [SerializeField, FieldChangeCallback(nameof(Tween))]
    float tween;

    private float updatedValue = -1;
    private int nTransforms = -1;
    //[SerializeField]
    private Vector3[] startPoints;
    //[SerializeField]
    private Vector3[] endPoints;

    private void init()
    {
        nTransforms = objects != null ? objects.Length : 0;
        if (nTransforms > 0)
        {
            startPoints = new Vector3[nTransforms];
            for (int i = 0; i < nTransforms; i++)
                startPoints[i] = objects[i] != null ? objects[i].position : transform.position;
            endPoints = new Vector3[nTransforms];
            int nOthers = targetOffsets == null ? 0 : targetOffsets.Length;
            for (int i = 0; i < nTransforms; i++)
            {
                if (i < nOthers && targetOffsets[i] != null)
                    endPoints[i] = startPoints[i] + targetOffsets[i];
                else
                    endPoints[i] = startPoints[i];
            }
            updatedValue = -1;
            Tween = tween;
        }
    }
    public float Tween 
    {   
        get => tween;
        set 
        {
            tween = Mathf.Clamp01(value);
            if (updatedValue != tween)
            {
                for (int i = 0; i < nTransforms; i++)
                {
                    if (objects[i] != null)
                    {
                        objects[i].position = Vector3.Lerp(startPoints[i], endPoints[i], tween);
                    }
                }
                updatedValue = tween;
            }
        }
    }

    void Start()
    {
        init();
    }
}
