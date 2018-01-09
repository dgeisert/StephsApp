using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class BaseEnemy : Photon.MonoBehaviour {

    public bool isDestructible = false;
    public bool isFriendly = false;
	public float hitPoints = 1, difficultyValue = 1;
	public bool isDead = false;
	protected bool lockOnPlayer = false;
    public Collider[] critAreas;
    public Material deadCrit;
    public Transform flytextSpawn;

    //this is the attacking and moving section
    public float speed = 4f
        , stopDistance = 3f
        , stopDistanceInner = 1.5f
        , normalDamageMult = 1f
        , critDamageMult = 1f
        , deathDuration = 1f
        , agroRange = 50f;
	public GameObject projectile, aoeAttack;
	public Vector3 toAttackPosition;
	public Transform patrolBox;
	public List<Vector3> patrolPath = new List<Vector3>();
	public float patrolPauseTime = 0.5f;
	protected Vector3 startPosition;
	Quaternion startRotation;
    public Transform bulletSpawnLocation;
    public Transform eyes;
    public Transform target;
    public OtherPlayerObject otherPlayerObject;
    public Vector3 movingTo, direction;
    public float movementCadence = 0.5f;
    protected bool is_backingUp = false;
	Vector3 movingAway = Vector3.zero;
	float outOfSightTime = 0;
	public Transform centerOfMass;
    public float attackSpeed = 2, attackDamage = 1, telegraphTime = 1;
    public bool attacking = false, autoMelee = true, selfDestruct = false;
    public Rigidbody rb;
	public float activeDistance = 100f;
	public bool is_active = false;
	public GameObject deathParticles;

	//this section is for the sounds
	public AudioSource audioSource;
	public AudioClip enemySound, enemyHitSound, enemyTelegraphSound, enemyAttackSound, enemyDeathSound;


    //this section for special status
    public bool frozen = false, onFire = false;


    public LootTable lootTable;

	void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
		}
		audioSource.clip = enemySound;
        if (lootTable == null)
        {
            foreach(LootTable lt in GetComponents<LootTable>())
            {
                if(lt.tableName == "primary")
                {
                    lootTable = lt;
                }
            }
        }
		startPosition = transform.position;
		startRotation = transform.rotation;
        GameManager.instance.EnemyChecks.Add(CheckDistance);
		EnemyStart();
        if (CreateLevel.instance != null)
        {
            CreateLevel.instance.totalEnemies++;
        }
        CheckDistance();
    }

    public void CheckDistance()
    {
        if (this == null)
        {
            GameManager.instance.EnemyChecks.Remove(CheckDistance);
            return;
        }
        if(Vector3.Distance(transform.position, PlayerManager.instance.GetPlayerPosition()) < activeDistance
			&& !isDestructible || (lockOnPlayer && !PlayerManager.instance.otherPlayerObject.isDead))
        {
            gameObject.SetActive(true);
            DetermineMovement();
            DoMove();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public virtual void EnemyStart()
    {

    }

    public void Update()
    {
        DoMove();
    }

    private void DoMove()
    {
        if (isDestructible)
        {
            return;
        }
        if (isDead)
        {
            DeadMove();
        }
        else
        {
            Move();
        }
    }
    
    public virtual void DetermineMovement()
    {
        if (isDestructible || isDead || frozen)
        {
            return;
        }
		if (lockOnPlayer && !PlayerManager.instance.otherPlayerObject.isDead)
        {
            if(target == null)
            {
                target = dgUtil.ClosestPlayer(eyes);
                if (target != null)
                {
                    otherPlayerObject = target.GetComponent<OtherPlayerObject>();
					PlayerManager.instance.isAgroed = true;
					PlayAudio (enemySound);
                }
            }
            else if (!otherPlayerObject.isDead)
            {
                AttackTarget();
            }
            return;
        }
        if ((photonView.isMine || NetworkManager.instance.singlePlayer) && !isFriendly)
        {
            if (isDead)
            {
                return;
            }
            if (target == null)
            {
                SetTarget();
            }
			if (target != null) {
				if (otherPlayerObject.isDead) {
					target = null;
					otherPlayerObject = null;
				} else {
					AttackTarget ();
					if (CheckTarget ()) {
						outOfSightTime = 0;
					} else {
						outOfSightTime += movementCadence;
						if (outOfSightTime > 10 && !(lockOnPlayer && !PlayerManager.instance.otherPlayerObject.isDead)) {
							NewPatrolPathPoint ();
							NewPatrolPoint ();
							target = null;
							otherPlayerObject = null;
							PlayerManager.instance.CheckAgro ();
						}
					}
				}
			}
        }
        if(target == null && patrolBox != null)
        {
            if(movingTo == Vector3.zero)
            {
                NewPatrolPoint();
            }
            if(Vector3.Distance(movingTo, transform.position) < 1)
            {
                NewPatrolPoint();
            }
		}
		else if (target == null && patrolPath.Count > 0)
		{
			if(movingTo == Vector3.zero)
			{
				NewPatrolPathPoint();
			}
			if(Vector3.Distance(movingTo, transform.position) < 0.5f)
			{
				PatrolPointReached ();
			}
		}
		else if (target == null)
		{
			movingTo = startPosition;
			if (Vector3.Distance(transform.position, startPosition) < 0.2f) 
			{
				movingTo = Vector3.zero;
				transform.rotation = startRotation;
			}
		}
        if (isFriendly)
        {
            if(target != null)
            {
                movingTo = target.position;
            }
        }
        EnemyUpdate();
    }

	bool updatingPath = false;
	public virtual void PatrolPointReached(){
		if (!updatingPath) {
			updatingPath = true;
			Invoke ("NewPatrolPathPoint", patrolPauseTime);
		}
	}

    public virtual void EnemyUpdate()
    {

    }

    public virtual void SetTarget()
    {
        Transform setTarget = dgUtil.ClosestPlayer(eyes);
        if(setTarget == null)
        {
            return;
        }
		if (Vector3.Distance(setTarget.position, eyes.position) <= agroRange * (1 + PlayerManager.instance.agroDistanceMod / 100))
        {
            RaycastHit raycastOut = new RaycastHit();
			Physics.Raycast(eyes.position, (setTarget.position - eyes.position).normalized, out raycastOut, agroRange * (1 + PlayerManager.instance.agroDistanceMod / 100));
            if (raycastOut.transform != null)
            {
                if (raycastOut.transform.GetComponentInParent<PlayerManager>() != null || raycastOut.transform.GetComponentInParent<OtherPlayerObject>() != null)
                {
					if (target == null) {
						target = setTarget;
						if (target != null) {
							otherPlayerObject = setTarget.GetComponent<OtherPlayerObject> ();
							PlayerManager.instance.isAgroed = true;
							PlayAudio (enemySound);
						}
					}
                }
            }
        }
    }

	public void LockOnPlayer(){
		lockOnPlayer = true;
        gameObject.SetActive(true);
	}

	public virtual bool CheckTarget()
	{
		if (Vector3.Distance(target.position, eyes.position) <= agroRange * (1 + PlayerManager.instance.agroDistanceMod / 100))
		{
			RaycastHit raycastOut = new RaycastHit();
			Physics.Raycast(eyes.position, (target.position - eyes.position).normalized, out raycastOut, agroRange * (1 + PlayerManager.instance.agroDistanceMod / 100));
			if (raycastOut.transform != null)
			{
				if (raycastOut.transform.GetComponentInParent<PlayerManager>() != null || raycastOut.transform.GetComponentInParent<OtherPlayerObject>() != null)
				{
					return true;
				}
			}
		}
		return false;
	}

    public virtual void NewPatrolPoint()
    {
        if (patrolBox == null)
        {
            return;
        }
		movingTo = patrolBox.position + 
			new Vector3(
				(Random.value - 0.5f) * patrolBox.lossyScale.x
				, (Random.value - 0.5f) * patrolBox.lossyScale.y
				, (Random.value - 0.5f) * patrolBox.lossyScale.z);
	}

	public virtual void NewPatrolPathPoint()
	{
		updatingPath = false;
        if(patrolPath.Count == 0)
        {
            return;
        }
		if (patrolPath.Contains (movingTo)) 
		{
			int i = patrolPath.IndexOf (movingTo) + 1;
			if (i >= patrolPath.Count) {
				i = 0;
			}
			movingTo = patrolPath[i];
		}
		else 
		{
			movingTo = patrolPath[0];
			foreach (Vector3 v3 in patrolPath) 
			{
				if(Vector3.Distance(transform.position, v3) < Vector3.Distance(transform.position, movingTo))
				{
					movingTo = v3;
				}
			}
		}
	}

    public virtual void AttackTarget()
    {
        if (!frozen)
        {
            transform.LookAt(target.position);
        }
        if (!attacking)
        {
            if (Vector3.Distance(target.position, transform.position) < stopDistance)
            {
                StartCoroutine("DoAttack");
            }
            else
            {
                movingTo = target.position;
                is_backingUp = false;
            }
        }
    }

    public virtual void Move()
    {
        if (frozen || updatingPath)
        {
            return;
        }
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if(target != null)
        {
            if (otherPlayerObject != null) {
                if (otherPlayerObject.isDead)
                {
                    target = null;
                    otherPlayerObject = null;
                    movingTo = startPosition;
                }
                else
                {
                    movingTo = target.position;
                }
            }
            else
            {
                movingTo = target.position;
            }
        }
        if (movingTo != Vector3.zero)
        {
            float distance = Vector3.Distance(movingTo, transform.position);
            if (distance < 0.5f && !(lockOnPlayer && !PlayerManager.instance.otherPlayerObject.isDead)) {
				DetermineMovement ();
			}
            if (distance > stopDistance || target == null)
            {
                if(target != null)
                {
                    if(Mathf.FloorToInt(distance - stopDistance) == 2 && !audioSource.isPlaying)
                    {
                        PlayAudio(enemySound);
                    }
                }
                direction = (movingTo - transform.position).normalized;
            }
            else if (distance < stopDistanceInner)
            {
                direction = (transform.position - movingTo).normalized;
            }
            else
            {
                if(target != null && !isFriendly)
                {
                    AttackTarget();
                }
                return;
            }
            transform.position += (direction * speed / 2 * Time.deltaTime + movingAway * 3 * Time.deltaTime);
			if (!attacking) {
				transform.LookAt (movingTo);
			}
			if (movingAway.magnitude > 0.5f) 
			{
				movingAway *= 0.95f;
			} 
			else 
			{
				movingAway = Vector3.zero;
			}
        }
    }

    public void DeadMove()
    {
        transform.position += (movingAway * Time.deltaTime);
    }

    public IEnumerator DoAttack(){
		attacking = true;
        Telegraph ();
		if (target != null) {
			movingTo = target.position;
            if (!frozen)
            {
                transform.LookAt(movingTo);
            }
		}
		toAttackPosition = PlayerManager.instance.GetPlayerPosition ();
		PlayAudio (enemyTelegraphSound);
		yield return new WaitForSeconds (telegraphTime);
        if(target != null)
        {
			if (Vector3.Distance (target.position, transform.position) < stopDistance + 1) {
				EnemyAttack ();
				if (projectile != null && bulletSpawnLocation != null) {
					GameManager.instance.CreateEnemyProjectile (projectile.name, bulletSpawnLocation.position, bulletSpawnLocation.rotation, Color.red, 500f, false, this);
					SelfDestructCheck ();
				} else if (aoeAttack != null && bulletSpawnLocation != null){
					dgUtil.Instantiate (aoeAttack, bulletSpawnLocation.position, Quaternion.Euler(bulletSpawnLocation.rotation.eulerAngles + new Vector3(-90, 0, 0)));
					SelfDestructCheck ();
				} else if (aoeAttack != null){
					dgUtil.Instantiate (aoeAttack, target.position, Quaternion.identity);
					SelfDestructCheck ();
				} else if (Vector3.Distance(PlayerManager.instance.GetPlayerPosition(), toAttackPosition) < 1 && autoMelee) {
					if (Random.value > PlayerManager.instance.blockChance / 100) {
						target.GetComponent<OtherPlayerObject> ().TakeDamage (attackDamage);
						SelfDestructCheck ();
					}
					TakeDamage (PlayerManager.instance.thornsDamage);
                }
                yield return new WaitForSeconds(attackSpeed - telegraphTime);
                attacking = false;
                StartCoroutine ("DoAttack");
			} else
            {
                attacking = false;
                CancelAttack ();
			}
		} else
        {
            attacking = false;
            CancelAttack ();
		}
	}

	public virtual void SelfDestructCheck(){
		if (selfDestruct) {
			PlayAudio (enemyDeathSound);
            GameManager.instance.EnemyChecks.Remove(CheckDistance);
            if (deathParticles != null) {
				Destroy(dgUtil.Instantiate (deathParticles, centerOfMass.position, transform.rotation), 5f);
			}
			Destroy (gameObject);
		}
	}

	public virtual void Telegraph(){

	}

	public virtual void CancelAttack(){

	}

	public virtual void EnemyAttack(){

	}

	public void Hit(float amount, Collider collider){
        float mult = normalDamageMult;
        for(int i = 0; i < critAreas.Length; i++)
        {
            if(critAreas[i] == collider)
            {
                mult = critDamageMult;
                break;
            }
		}
		BaseWeapon bw = collider.GetComponentInParent<BaseWeapon> ();
		float weaponKnockbackMult = 1f;
		if (bw != null) {
			weaponKnockbackMult = bw.knockbackMult;
			if (target == null) {
				mult *= bw.sneakDamageBonus;
			}
		}
		if (centerOfMass != null) 
		{
			movingAway = (centerOfMass.position - collider.transform.position).normalized * Mathf.Pow(amount, 0.1f) * mult * weaponKnockbackMult;
		}
        TakeDamage(amount * mult);
        if (PhotonNetwork.connected)
        {
            GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.Others, amount * mult);
        }
		if (!isDead) 
		{
			Shout ();
		}
	}

	public void Shout(){
        if(CreateLevel.instance == null || isFriendly || isDestructible)
        {
            return;
        }
		foreach(GameObject go in CreateLevel.instance.enemies)
		{
			if (go != null) {
				BaseEnemy be = go.GetComponent<BaseEnemy> ();
				if (Vector3.Distance (transform.position, be.transform.position) < 10f) {
					be.ForceAgro ();
				}
			}
		}
	}

    public void ForceAgro()
    {
        if (!isDestructible && !isFriendly)
        {
			if (target == null) {
				target = dgUtil.ClosestPlayer (eyes);
				if (target != null) {
					otherPlayerObject = target.GetComponent<OtherPlayerObject> ();
					PlayerManager.instance.isAgroed = true;
					PlayAudio (enemySound);
				}
			}
        }
    }

	public virtual void CheckDeath(){
		if (hitPoints <= 0) {
            StartDeath();
        }
    }

    public void StartDeath()
    {
		isDead = true;
        GameManager.instance.EnemyChecks.Remove(CheckDistance);
        if(CreateLevel.instance != null)
        {
            if(CreateLevel.instance.levelType == LevelType.InfiniteWave)
            {
                PlayerManager.instance.score += Mathf.FloorToInt(difficultyValue);
            }
        }
		if (!isDestructible && otherPlayerObject == null ? true : !otherPlayerObject.npc)
        {
            foreach (GameObject go in CreateLevel.instance.enemies)
            {
                if (go != null)
                {
                    BaseEnemy be = go.GetComponent<BaseEnemy>();
                    if (!be.isFriendly && !be.isDestructible)
                    {
                        if (Vector3.Distance(transform.position, be.transform.position) < be.agroRange * (1 + PlayerManager.instance.agroDistanceMod / 100))
                        {
                            RaycastHit raycastOut = new RaycastHit();
                            Physics.Raycast(eyes.position, (centerOfMass.position - eyes.position).normalized, out raycastOut, agroRange * (1 + PlayerManager.instance.agroDistanceMod / 100));
                            if (raycastOut.transform != null)
                            {
                                if (raycastOut.transform.GetComponentInParent<BaseEnemy>() == this)
                                {
                                    if (be.target == null)
                                    {
                                        be.target = PlayerManager.instance.otherPlayerObject.transform;
                                        be.otherPlayerObject = PlayerManager.instance.otherPlayerObject;
                                        be.PlayAudio(be.enemySound);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            PlayerManager.instance.CheckAgro();
            if (selfDestruct)
            {
                SelfDestructCheck();
                return;
            }
            rb.freezeRotation = false;
            PlayAudio(enemyDeathSound);
            if (deathParticles != null)
            {
                Destroy(dgUtil.Instantiate(deathParticles, centerOfMass.position, transform.rotation), 5f);
            }
            if (deadCrit != null)
            {
                foreach (Collider col in critAreas)
                {
                    MeshRenderer mr = col.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        List<Material> deadMatList = new List<Material>();
                        for (int i = 0; i < mr.materials.Length; i++)
                        {
                            deadMatList.Add(deadCrit);
                        }
                        col.GetComponent<MeshRenderer>().materials = deadMatList.ToArray();
                    }
                }
            }
        }
        //death animation
        Invoke("DoDeath", deathDuration);
    }

    public void DoDeath()
    {
        if (lootTable != null)
        {
            lootTable.RollTable();
        }
		if (CreateLevel.instance != null) {
			CreateLevel.instance.enemiesKilled++;
		}
        PhotonNetwork.Destroy(gameObject);
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data

        }
        else
        {
            // Network player, receive data

        }
    }

    [PunRPC]
    public void TakeDamage(float amount)
    {
        Vector3 spawnLocation = Vector3.zero;
        if (flytextSpawn != null)
        {
            spawnLocation = flytextSpawn.position;
        }
        else
        {
            spawnLocation = transform.position + Vector3.up * 1.5f;
        }
		if (amount == 0) {
			dgUtil.SpawnFlytext(Color.white, "Immune", spawnLocation);
		} else {
			dgUtil.SpawnFlytext(Color.red, Mathf.Max (Mathf.Floor (amount), 1).ToString (), spawnLocation);
		}
        hitPoints -= amount;
		PlayAudio (enemyHitSound);
		EnemyTakeDamage ();
        CheckDeath();
    }

	public virtual void EnemyTakeDamage(){

	}

	[PunRPC]
	public void CreateProjectile(string pName, Vector3 pPosition, Quaternion pRotation, Quaternion colTemp, float pSpeed, bool pGravity, bool isMine = false)
	{
		GameObject bulletClone = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(pName), pPosition, pRotation, false, null, false) as GameObject;
		Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
		rb.AddForce(-bulletClone.transform.forward * pSpeed);
		bulletClone.GetComponent<Bullet>().isMine = isMine;
		bulletClone.GetComponent<Bullet>().SetMaterial(colTemp);
	}

	public virtual void PlayAudio(AudioClip clip){
        if(audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.pitch = Random.value * 0.4f + 0.8f;
            audioSource.Play();
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.name == "Top" || collision.collider.name == "IslandBase")
        {
            if(transform.position.y < collision.collider.transform.position.y)
            {
                transform.position += new Vector3(0, collision.collider.transform.position.y - transform.position.y + 0.5f, 0);
            }
        }
    }
}
