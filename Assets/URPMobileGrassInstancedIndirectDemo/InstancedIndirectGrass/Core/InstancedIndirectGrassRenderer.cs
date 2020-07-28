//see this for ref: https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using System;
using UnityEngine;

[ExecuteAlways]
public class InstancedIndirectGrassRenderer : MonoBehaviour
{
    [Range(1,1000000)]
    public int instanceCount = 50000;
    public float drawDistance = 125;
    public Material instanceMaterial;
    public ComputeShader cullingComputeShader;

    //global ref to this script
    [HideInInspector]
    public static InstancedIndirectGrassRenderer instance;

    private int cachedInstanceCount = -1;
    private Mesh cachedGrassMesh;

    private ComputeBuffer allInstanceTransformBuffer;
    private ComputeBuffer visibleInstanceOnlyTransformBuffer;
    private ComputeBuffer argsBuffer;

    private void Awake()
    {
        instance = this; // assign global ref using this script
    }

    void LateUpdate()
    {
        // recreate all buffers in grass shader if needed
        UpdateAllInstanceTransformBufferIfNeeded();

        //dispatch culling compute, fill visible instance into visibleInstanceOnlyTransformBuffer
        visibleInstanceOnlyTransformBuffer.SetCounterValue(0);
        Matrix4x4 v = Camera.main.worldToCameraMatrix;
        Matrix4x4 p = Camera.main.projectionMatrix;
        Matrix4x4 vp = p * v;
        cullingComputeShader.SetMatrix("_VPMatrix", vp);
        cullingComputeShader.SetFloat("_MaxDrawDistance", drawDistance);
        cullingComputeShader.SetBuffer(0, "_AllInstancesTransformBuffer", allInstanceTransformBuffer);
        cullingComputeShader.SetBuffer(0, "_VisibleInstanceOnlyTransformIDBuffer", visibleInstanceOnlyTransformBuffer);
        cullingComputeShader.Dispatch(0, Mathf.CeilToInt(instanceCount/1024), 1, 1);
        ComputeBuffer.CopyCount(visibleInstanceOnlyTransformBuffer, argsBuffer, 4);

        // Render     
        Graphics.DrawMeshInstancedIndirect(GetGrassMeshCache(), 0, instanceMaterial, new Bounds(transform.position, transform.localScale * 2), argsBuffer);
    }

    void OnDisable()
    {
        //release all compute buffers
        if (allInstanceTransformBuffer != null)
            allInstanceTransformBuffer.Release();
        allInstanceTransformBuffer = null;

        if (visibleInstanceOnlyTransformBuffer != null)
            visibleInstanceOnlyTransformBuffer.Release();
        visibleInstanceOnlyTransformBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(300, 50, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = Mathf.Max(1,(int)(GUI.HorizontalSlider(new Rect(300, 100, 200, 30), instanceCount / 10000f, 0, 100)) *10000);

        float scale = Mathf.Sqrt((instanceCount / 4)) / 2f;
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);

        GUI.Label(new Rect(300, 150, 200, 30), "Draw Distance: " + drawDistance);
        drawDistance = Mathf.Max(1, (int)(GUI.HorizontalSlider(new Rect(300, 200, 200, 30), drawDistance/ 25f, 1, 8))*25);
    }

    Mesh GetGrassMeshCache()
    {
        if (!cachedGrassMesh)
        {
            //if not exist, create a 3 vertices hardcode triangle grass mesh
            cachedGrassMesh = new Mesh();

            //first grass (vertices)
            Vector3[] verts = new Vector3[3];
            verts[0] = new Vector3(-0.25f, 0);
            verts[1] = new Vector3(+0.25f, 0);
            verts[2] = new Vector3(-0.0f, 1);
            //first grass (Triangles index)
            int[] trinagles = new int[3] { 2, 1, 0, }; //order to fit Cull Back in grass shader

            cachedGrassMesh.SetVertices(verts);
            cachedGrassMesh.SetTriangles(trinagles, 0);
        }

        return cachedGrassMesh;
    }

    void UpdateAllInstanceTransformBufferIfNeeded()
    {
        //always update
        instanceMaterial.SetVector("_PivotPosWS", transform.position);
        instanceMaterial.SetVector("_BoundSize", new Vector2(transform.localScale.x, transform.localScale.z));

        //early exit if no need update buffer
        if (cachedInstanceCount == instanceCount &&
            argsBuffer != null &&
            allInstanceTransformBuffer != null &&
            visibleInstanceOnlyTransformBuffer != null)
            {
                return;
            }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////
        // Transform buffer
        ///////////////////////////
        if (allInstanceTransformBuffer != null)
            allInstanceTransformBuffer.Release();
        allInstanceTransformBuffer = new ComputeBuffer(instanceCount, sizeof(float)*3); //float3 posWS only, per grass

        if (visibleInstanceOnlyTransformBuffer != null)
            visibleInstanceOnlyTransformBuffer.Release();
        visibleInstanceOnlyTransformBuffer = new ComputeBuffer(instanceCount, sizeof(uint), ComputeBufferType.Append); //uint only, per visible grass

        //keep grass visual the same
        UnityEngine.Random.InitState(123);

        //spawn grass inside gizmo cube 
        Vector3[] positions = new Vector3[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Vector3.zero;

            //can define any posWS in this section, random is just an example
            //TODO: allow API call to set posWS
            //===================================================
            //local pos
            pos.x = UnityEngine.Random.Range(-1f, 1f);
            pos.y = 0;
            pos.z = UnityEngine.Random.Range(-1f, 1f);

            //transform to posWS in C#
            pos.x *= transform.lossyScale.x;
            pos.z *= transform.lossyScale.z;
            pos += transform.position;
            //===================================================

            positions[i] = new Vector3(pos.x,pos.y,pos.z);
        }

        allInstanceTransformBuffer.SetData(positions);
        instanceMaterial.SetBuffer("_AllInstancesTransformBuffer", allInstanceTransformBuffer);
        instanceMaterial.SetBuffer("_VisibleInstanceOnlyTransformIDBuffer", visibleInstanceOnlyTransformBuffer);

        ///////////////////////////
        // Indirect args buffer
        ///////////////////////////
        if (argsBuffer != null)
            argsBuffer.Release();
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        args[0] = (uint)GetGrassMeshCache().GetIndexCount(0);
        args[1] = (uint)instanceCount;
        args[2] = (uint)GetGrassMeshCache().GetIndexStart(0);
        args[3] = (uint)GetGrassMeshCache().GetBaseVertex(0);
        args[4] = 0;

        argsBuffer.SetData(args);

        //update cache to prevent future no-op buffer update, which waste performance
        cachedInstanceCount = instanceCount;
    }
}