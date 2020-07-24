using UnityEngine;
[ExecuteAlways]
public class InstancedIndirectGrassRenderer : MonoBehaviour
{
    [Range(1,100000)]
    public int instanceCount = 20000;
    public Material instanceMaterial;

    private int cachedInstanceCount = -1;
    private Vector3 cachedPivotPos = Vector3.negativeInfinity;
    private Vector3 cachedLocalScale = Vector3.negativeInfinity;
    private Mesh cachedGrassMesh;

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

    void LateUpdate()
    {
        // Update _TransformBuffer in grass shader
        UpdateBuffers();

        // Render     
        Graphics.DrawMeshInstancedIndirect(GetGrassMesh(), 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f)), argsBuffer);
    }
    void OnDisable()
    {
        //release all compute buffers
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(265+500, 25, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(500, 20, 200, 30), (float)instanceCount, 1.0f, 100000.0f);
    }

    Mesh GetGrassMesh()
    {
        if (!cachedGrassMesh)
        {
            //if not exist, return a 5 vertices hardcode grass mesh
            cachedGrassMesh = new Mesh();
            Vector3[] verts = new Vector3[3];

            //first grass
            verts[0] = new Vector3(-0.25f, 0);
            verts[1] = new Vector3(+0.25f, 0);
            verts[2] = new Vector3(-0.0f, 1);

            cachedGrassMesh.SetVertices(verts);
            int[] trinagles = new int[3] { 0, 1, 2,};
            cachedGrassMesh.SetTriangles(trinagles, 0);
        }

        return cachedGrassMesh;
    }

    void UpdateBuffers()
    {
        //early exit if no need update buffer
        if (cachedInstanceCount == instanceCount &&
            cachedPivotPos == transform.position &&
            cachedLocalScale == transform.localScale &&
            argsBuffer != null &&
            positionBuffer != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    return;
            }
        //=============================================
        if (argsBuffer != null)
            argsBuffer.Release();
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        if (positionBuffer != null)
            positionBuffer.Release();
        Vector4[] positions = new Vector4[instanceCount];
        positionBuffer = new ComputeBuffer(positions.Length, sizeof(float)*4); //float4

        //spawn grass inside gizmo cube 
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = transform.position;
            pos.x += Random.Range(-1f, 1f) * transform.localScale.x;
            pos.z += Random.Range(-1f, 1f) * transform.localScale.z;
            float size = Random.Range(2f, 5f);
            positions[i] = new Vector4(pos.x,pos.y,pos.z, size);
        }
        positionBuffer.SetData(positions);
        instanceMaterial.SetBuffer("_TransformBuffer", positionBuffer);
        instanceMaterial.SetVector("_PivotPosWS", transform.position);
        instanceMaterial.SetFloat("_BoundSize", transform.localScale.x);
        // Indirect args
        args[0] = (uint)GetGrassMesh().GetIndexCount(0);
        args[1] = (uint)instanceCount;
        args[2] = (uint)GetGrassMesh().GetIndexStart(0);
        args[3] = (uint)GetGrassMesh().GetBaseVertex(0);

        argsBuffer.SetData(args);

        //update cache to prevent future no-op update
        cachedInstanceCount = instanceCount;
        cachedPivotPos = transform.position;
        cachedLocalScale = transform.localScale;
    }
}