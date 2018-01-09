using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class EnemySpawner : Photon.MonoBehaviour {

    public GameObject enemy;
    public float minSize, maxSize;
    public Transform[] spawnPoints;
	public bool repeatSpawn = false;
    float coolDown = 1f, startCooldown = 0f;
    public Transform patrolArea;
	public bool spawnOnStart = false;

	public void Start()
	{
		spawnPoints = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			spawnPoints [i] = transform.GetChild (i).transform;
		}
		if (spawnOnStart) {
			Spawn ();
		}
	}

	public void OnTriggerEnter(Collider col)
    {
		if ((col.GetComponentInParent<OtherPlayerObject>() != null || col.GetComponentInParent<PlayerManager>() != null) 
            && (PhotonNetwork.isMasterClient && gameObject.GetActive()
            || NetworkManager.instance.singlePlayer)
            && coolDown + startCooldown < Time.time)
        {
			Spawn ();
        }
    }

	public void Spawn(){
		startCooldown = Time.time;
		foreach(Transform spawnPoint in spawnPoints)
		{
			GameObject go = dgUtil.Instantiate(enemy, spawnPoint.position, Quaternion.identity, false, null, false);
			go.transform.localScale *= (maxSize - minSize) * Random.value + minSize;
			BaseEnemy be = go.GetComponent<BaseEnemy>();
			if(be != null)
			{
				if(patrolArea != null)
				{
					be.patrolBox = patrolArea;
				}
			}
		}
		if (!repeatSpawn) 
		{
			gameObject.SetActive (false);
		}
	}
}
