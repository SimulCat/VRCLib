using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
[RequireComponent(typeof(MeshFilter))]

public class MeshCombine : UdonSharpBehaviour
{
    [SerializeField]
    private MeshFilter parentMeshFilter;
    private void Start()
    {
        if (parentMeshFilter == null)
            parentMeshFilter = GetComponent<MeshFilter>();
    }
    
    private void OnValidate()
    {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        Matrix4x4 mytranspose = transform.worldToLocalMatrix;
        int combined = 0; int childCount = meshFilters != null ? meshFilters.Length - 1 : -1;
        CombineInstance combineInstance;
        if (childCount > 0)
        {
            CombineInstance[] combine;
            combine = new CombineInstance[childCount];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].transform == transform)
                    continue;
                combineInstance = new CombineInstance();
                combineInstance.mesh = meshFilters[i].sharedMesh;
                combineInstance.transform = mytranspose * meshFilters[i].transform.localToWorldMatrix;
                combine[combined] = combineInstance;
                meshFilters[i].gameObject.SetActive(false);
                combined++;
            }
            if (combined > 0)
            {
                parentMeshFilter = GetComponent<MeshFilter>();
                parentMeshFilter.mesh = new Mesh();
                parentMeshFilter.mesh.CombineMeshes(combine, true, true);
                // 5. Update bounds and topology info
                parentMeshFilter.mesh.RecalculateBounds();
                parentMeshFilter.mesh.RecalculateNormals();
            }
        }
    } 
}
