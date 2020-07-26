//see this for ref: https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using UnityEngine;

[ExecuteAlways]
public class InstancedIndirectGrassRenderer : MonoBehaviour
{
    [Range(1,100000)]
    public int instanceCount = 20000;
    public float drawDistance = 125;
    public Material instanceMaterial;

    //global ref to this script
    [HideInInspector]
    public static InstancedIndirectGrassRenderer instance;

    private int cachedInstanceCount = -1;
    private Mesh cachedGrassMesh;

    private ComputeBuffer transformBigBuffer;
    private ComputeBuffer argsBuffer;

    private void Awake()
    {
        instance = this; // assign global ref using this script
    }
    void LateUpdate()
    {

        // Update _TransformBuffer in grass shader if needed
        UpdateBuffersIfNeeded();

        // Render     
        Graphics.DrawMeshInstancedIndirect(GetGrassMeshCache(), 0, instanceMaterial, new Bounds(transform.position, transform.localScale * 2), argsBuffer);
    }
    void OnDisable()
    {
        //release all compute buffers
        if (transformBigBuffer != null)
            transformBigBuffer.Release();
        transformBigBuffer = null;

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

    void UpdateBuffersIfNeeded()
    {
        //always update
        instanceMaterial.SetVector("_PivotPosWS", transform.position);
        instanceMaterial.SetVector("_BoundSize", new Vector2(transform.localScale.x, transform.localScale.z));
        instanceMaterial.SetFloat("_DrawDistance", drawDistance);

        //early exit if no need update buffer
        if (cachedInstanceCount == instanceCount &&
            argsBuffer != null &&
            transformBigBuffer != null)
            {
                return;
            }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////
        // Transform buffer
        ///////////////////////////
        if (transformBigBuffer != null)
            transformBigBuffer.Release();
        Vector4[] positions = new Vector4[instanceCount];
        transformBigBuffer = new ComputeBuffer(positions.Length, sizeof(float)*4); //float4 per grass

        //keep grass visual the same
        Random.InitState(123);

        //spawn grass inside gizmo cube 
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Vector3.zero;

            //local pos (can define any local pos here, random is just an example)
            pos.x = Random.Range(-1f, 1f);
            pos.z = Random.Range(-1f, 1f);

            //local rotate
            //TODO: allow this gameobject's rotation affect grass, make sure to update bending grass's imaginary camera rotation also

            //world scale
            float size = Random.Range(2f, 5f);

            positions[i] = new Vector4(pos.x,pos.y,pos.z, size);
        }

        transformBigBuffer.SetData(positions);
        instanceMaterial.SetBuffer("_TransformBuffer", transformBigBuffer);

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