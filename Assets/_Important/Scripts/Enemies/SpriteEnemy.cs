using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteEnemy : BaseEnemy {

	public GameObject chargeParticles;
	public override void Telegraph(){
		GameObject go = dgUtil.Instantiate (chargeParticles, bulletSpawnLocation.position, bulletSpawnLocation.rotation);
		Destroy (go, telegraphTime);
	}
	
}
