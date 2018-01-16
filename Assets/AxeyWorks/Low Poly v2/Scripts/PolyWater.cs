using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class PolyWater : MonoBehaviour
{

    Vector3 _waveSource1 = new Vector3(-200f, 0.0f, 200f);
    public float WaveSpeed = -0.3f;
    public float WaveHeight = 0.48f;
    public bool EdgeBlend = true;
    public bool ForceFlatShading = true;
	public float waveShape;
    Mesh _mesh;
    Vector3[] _verts;
	Vector2[] _uvs;

	public void Init(Camera camera)
    {
		camera.depthTextureMode |= DepthTextureMode.Depth;
        var mf = GetComponent<MeshFilter>();
		MakeMeshLowPoly(mf);
		Shader.DisableKeyword("WATER_EDGEBLEND_ON");
    }

    MeshFilter MakeMeshLowPoly(MeshFilter mf)
    {
        _mesh = mf.sharedMesh;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _verts = _mesh.vertices;
		_uvs = _mesh.uv;
        return mf;
    }

    void CalcWave()
	{
		float flowZ = Mathf.Sin (Time.time * WaveSpeed) * WaveHeight;
		float flowX = Mathf.Cos (Time.time * WaveSpeed) * WaveHeight;
        for (var i = 0; i < _verts.Length; i++)
        {
			var uv = _uvs [i];
			uv.y += flowZ;
			uv.x += flowX;
			/*
			var v = _verts[i];
			v.y = 0;
			v.y = WaveHeight * Mathf.PerlinNoise(WaveSpeed * (v.x + transform.position.x + Time.time),
				WaveSpeed * (v.z + transform.position.z + Time.time));
            _verts[i] = v;
            */
			_uvs [i] = uv;
        }
		_mesh.uv = _uvs;
        //_mesh.vertices = _verts;
        //_mesh.RecalculateNormals();
        //_mesh.MarkDynamic();

        //GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

	void OnDisable(){
		CancelInvoke ("CalcWave");
	}
	void OnEnable(){
		InvokeRepeating ("CalcWave", 0.1f, 0.1f);
	}
}