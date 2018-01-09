using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagingCollider : MonoBehaviour {

	Dictionary<PlayerManager, float> playersHit;
	Dictionary<BaseEnemy, float> enemiesHit;
	public float damage = 1;
	public float cooldown = 2;
	public bool hurtsEnemies = false;
	public bool hurtsPlayer = true;

    private void Start()
    {
        playersHit = new Dictionary<PlayerManager, float>();
        enemiesHit = new Dictionary<BaseEnemy, float>();
    }

	public void OnCollisionEnter(Collision col){
		PlayerManager pm = col.collider.GetComponentInParent<PlayerManager> ();
        CheckHit(pm);
	}

    public void OnTriggerEnter(Collider col)
    {
		if (hurtsPlayer) {
			PlayerManager pm = col.GetComponentInParent<PlayerManager> ();
			CheckHit (pm);
		}
		if (hurtsEnemies) {
			BaseEnemy be = col.GetComponentInParent<BaseEnemy> ();
			CheckHit (be);
		}
	}

	void CheckHit(PlayerManager pm)
	{
		if (pm != null)
		{
			if (playersHit.ContainsKey(pm))
			{
				if (playersHit[pm] + cooldown > Time.time)
				{
					return;
				}
			}
			pm.otherPlayerObject.TakeDamage(damage);
			if (playersHit.ContainsKey(pm))
			{
				playersHit[pm] = Time.time;
			}
			else
			{
				playersHit.Add(pm, Time.time);
			}
		}
	}

	void CheckHit(BaseEnemy be)
	{
		if (be != null)
		{
			if (enemiesHit.ContainsKey(be))
			{
				if (enemiesHit[be] + cooldown > Time.time)
				{
					return;
				}
			}
			be.TakeDamage(damage);
			if (enemiesHit.ContainsKey(be))
			{
				enemiesHit[be] = Time.time;
			}
			else
			{
				enemiesHit.Add(be, Time.time);
			}
		}
	}
}
