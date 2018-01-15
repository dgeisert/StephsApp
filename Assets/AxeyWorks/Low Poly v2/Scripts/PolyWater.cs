using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
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
	Vector2[] _uvs;

	public void Init(Camera camera)
    {
		camera.depthTextureMode |= DepthTextureMode.Depth;
        var mf = GetComponent<MeshFilter>();
		MakeMeshLowPoly(mf);
		Shader.DisableKeyword("WATER_EDGEBLEND_ON");
		InvokeRepeating ("CalcWave", 0.1f, 0.1f);
    }

    MeshFilter MakeMeshLowPoly(MeshFilter mf)
    {
        _mesh = mf.sharedMesh;
        var oldVerts = _mesh.vertices;
        var triangles = _mesh.triangles;
		var vertices = oldVerts;/*new Vector3[triangles.Length];
        for (var i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;
        }*/
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _verts = _mesh.vertices;
		_uvs = _mesh.uv;
        return mf;
    }

	void Update(){
		for (var i = 0; i < _uvs.Length; i++)
		{
			_uvs [i] = new Vector2 (transform.position.x + _verts[i].x, transform.position.z + _verts[i].z);
		}
		_mesh.uv = _uvs;

		GetComponent<MeshFilter>().sharedMesh = _mesh;
	}

    void CalcWave()
    {
        for (var i = 0; i < _verts.Length; i++)
        {
            var v = _verts[i];
            v.y = 0.0f;
			var dist = Vector3.Distance(v, _waveSource1);
            dist = (dist % WavePattern) / WavePattern;
            v.y = WaveHeight * Mathf.Sin(Time.time * Mathf.PI * 2.0f * WaveSpeed
            + (Mathf.PI * 2.0f * dist));
            _verts[i] = v;
			_uvs [i] = new Vector2 (transform.position.x + v.x, transform.position.z + v.z);
        }
        _mesh.vertices = _verts;
		_mesh.uv = _uvs;
        _mesh.RecalculateNormals();
        _mesh.MarkDynamic();

        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }
}