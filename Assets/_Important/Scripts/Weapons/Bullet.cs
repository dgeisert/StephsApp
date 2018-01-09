using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class Bullet : Photon.MonoBehaviour
{

	private float bulletLife = 5f;
	public GameObject bulletState, hitState;
	public float damage = 1;
	public bool isMine = false;
    public bool isEnemy = false;

	void Start () {
		Destroy (gameObject, bulletLife);
		bulletState.SetActive (true);
		hitState.SetActive (false);
	}

	public void SetMaterial(Quaternion mat){
		Color col = new Color (mat.x, mat.y, mat.z, mat.w);
		foreach(MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()){
			mr.material.color = col;
		}
		foreach(ParticleSystem ps in GetComponentsInChildren<ParticleSystem>()){
			ParticleSystem.MainModule psm = ps.main;
			psm.startColor = col;
		}
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.layer == 2 && !(isEnemy && col.collider.GetComponentInParent<Shield> () != null)) {
			return; // ignore the layer ignore raycast, should be refined to something else, probably a tag.
		}
        if(isEnemy && col.collider.GetComponentInParent<BaseEnemy>() != null)
        {
            return; // enemy shots don't hit enemies, should be refined to something else, probably a tag.
        }
		transform.SetParent (col.transform);
		bulletState.SetActive (false);
		hitState.SetActive (true);
		GetComponent<Rigidbody> ().isKinematic = true;
		transform.position = col.contacts[0].point;
        foreach (MeshCollider component in GetComponentsInChildren<MeshCollider>())
        {
            Destroy(component);
        }
        foreach (BoxCollider component in GetComponentsInChildren<BoxCollider>())
        {
            Destroy(component);
        }
        foreach (SphereCollider component in GetComponentsInChildren<SphereCollider>())
        {
            Destroy(component);
        }
		if (!isEnemy && isMine && col.collider.GetComponentInParent<BaseEnemy> () != null) {
			col.transform.GetComponentInParent<BaseEnemy> ().Hit (damage, col.collider);
		} else if (isEnemy && col.collider.GetComponentInParent<Shield> () != null) {
			//don't look for player if hit shield
		}
        else if (isEnemy && col.collider.GetComponentInParent<PlayerManager>() != null)
        {
			if (Random.value > PlayerManager.instance.dodgeChance / 100) {
				PlayerManager.instance.otherPlayerObject.TakeDamage (damage);
			}
        }
        else if (!isEnemy && isMine && col.collider.GetComponentInParent<TreasureChest>() != null)
        {
            col.transform.GetComponentInParent<TreasureChest>().AttemptOpen();
        }
    }

}
