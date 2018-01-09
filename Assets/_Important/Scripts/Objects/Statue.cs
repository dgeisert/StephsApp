using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour {

	public int statueCount;
	public Transform pedestal;
	public List<Material> mats;

	public void Init(CreateIsland island){
		GameObject go = dgUtil.Instantiate (
			GameManager.instance.GetResourcePrefab ("statue" + Mathf.FloorToInt (Random.value * statueCount)),
			Vector3.zero,
			Quaternion.identity,
			true,
			pedestal);
		Material mat = mats[Mathf.FloorToInt (Random.value * mats.Count)];
		foreach (Hand hand in go.GetComponentsInChildren<Hand>()) {
			hand.seed = Mathf.FloorToInt (Random.value * 10000);
			hand.Init ();
			dgUtil.Disable (hand.gameObject);
		}
		foreach (Head head in go.GetComponentsInChildren<Head>()) {
			head.seed = Mathf.FloorToInt (Random.value * 10000);
			head.Init ();
			dgUtil.Disable (head.gameObject);
		}
		foreach (Body body in go.GetComponentsInChildren<Body>()) {
			body.seed = Mathf.FloorToInt (Random.value * 10000);
			body.Init ();
			dgUtil.Disable (body.gameObject);
		}
		foreach (BaseWeapon bw in go.GetComponentsInChildren<BaseWeapon>()) {
			bw.seed = Mathf.FloorToInt (Random.value * 10000);
			bw.Init ();
			dgUtil.Disable (bw.gameObject);
		}
		foreach(MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()){
			List<Material> setMats = new List<Material> ();
			for(int i = 0; i < mr.materials.Length; i++) {
				setMats.Add (mat);
			}
			mr.materials = setMats.ToArray ();
		}
		foreach (ProceduralSplat ps in GetComponentsInChildren<ProceduralSplat>()) {
			ps.baseColorList = new List<Color>{ Color.grey };
			ps.leavesColorList = new List<Color>{ Color.grey };
			ps.Init (Mathf.FloorToInt (Random.value * 10000), island, transform.localPosition + Vector3.up / 2, 0.05f, true);
		}
	}
}
