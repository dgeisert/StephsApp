using UnityEngine;
using System.Collections;


public class PolyWater : MonoBehaviour
{

    Vector3 _waveSource1 = new Vector3(2.0f, 0.0f, 2.0f);
    public float WaveSpeed = -0.3f;
    public float WaveHeight = 0.48f;
    public float WavePattern = 0.62f;
    public bool EdgeBlend = true;
    public bool ForceFlatShading = true;
    Mesh _mesh;
    Vector3[] _verts;

    void Start()
    {

    }

    int frameDelay = 2, thisFrame = 0;
    bool meshAssigned = false;
    void Update()
    {
        if (_mesh == null)
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                return;
            }
            if(mf.mesh == null)
            {
                return;
            }
            MakeMeshLowPoly(mf);
            return;
        }
        thisFrame++;
        if (thisFrame > frameDelay)
        {
            CalcWave();
            SetEdgeBlend();
            thisFrame = 0;
        }
    }

    MeshFilter MakeMeshLowPoly(MeshFilter mf)
    {
        _mesh = mf.sharedMesh;
        var oldVerts = _mesh.vertices;
        var triangles = _mesh.triangles;
        var vertices = new Vector3[triangles.Length];
        for (var i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;
        }
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _verts = _mesh.vertices;
        return mf;
    }

    void SetEdgeBlend()
    {
        if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
        {
            EdgeBlend = false;
        }
        if (EdgeBlend)
        {
            Shader.EnableKeyword("WATER_EDGEBLEND_ON");
            if (Camera.main)
            {
                Camera.main.depthTextureMode |= DepthTextureMode.Depth;
            }
        }
        else
        {
            Shader.DisableKeyword("WATER_EDGEBLEND_ON");
        }
    }

    void CalcWave()
    {
        for (var i = 0; i < _verts.Length; i++)
        {
            var v = _verts[i];
            v.y = 0.0f;
            var dist = Vector3.Distance(v, _waveSource1);
            dist = (dist % WavePattern) / WavePattern;
            v.y = 0.5f + WaveHeight * Mathf.Sin(Time.time * Mathf.PI * 2.0f * WaveSpeed
            + (Mathf.PI * 2.0f * dist));
            _verts[i] = v;
        }
        _mesh.vertices = _verts;
        _mesh.RecalculateNormals();
        _mesh.MarkDynamic();

        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }
}