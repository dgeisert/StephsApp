using UnityEngine;
using Photon;
using VRTK;
using UnityEngine.UI;
using Holoville.HOTween;

public class OtherPlayerObject : Photon.MonoBehaviour
{
    public PlayerManager myPlayer;
	public bool isMine = false, npc = false;
    public GameObject renderer, body;
    public Transform head, left, right;
    private Vector3 correctHeadPos, CorrectBodyPos, correctRightPos, correctLeftPos, correctScale;
	private Quaternion correctHeadRot, CorrectBodyRot, correctRightRot, correctLeftRot;
	public Head bodyPartHead;
	public Body bodyPartBody;
	public Hand bodyPartHandLeft, bodyPartHandRight;
    public int leftGrab = -1, rightGrab = -1;
    private PhotonView rg, lg;
    public VRTK_InteractGrab rig, lig;
    public VRTK_InteractTouch rit, lit;
    Vector3 lsp, rsp;
    Quaternion lsr, rsr;

    public float health = 100, maxHealth = 100;
    public float lastDamageTime;
    public bool isDead = false;

    //This section for UI elements
    public Text healthText;

    private void Start()
    {
        if(PlayerManager.instance != null)
        {
            PlayerManager.instance.players.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.instance == null || npc)
        {
            return;
        }
        if (NetworkManager.instance.singlePlayer)
        {
            transform.position = myPlayer.head.position - Vector3.up;
            transform.localScale = myPlayer.transform.localScale;
            transform.eulerAngles = new Vector3(0, myPlayer.head.eulerAngles.y, 0);
            head.position = myPlayer.head.position;
            head.rotation = myPlayer.head.rotation;
            right.position = myPlayer.right.position;
            right.rotation = myPlayer.right.rotation;
            left.position = myPlayer.left.position;
            left.rotation = myPlayer.left.rotation;
        }
        else
        {
            if (!photonView.isMine)
            {
                this.CorrectBodyPos = correctHeadPos - Vector3.up;
                this.CorrectBodyRot.eulerAngles = new Vector3(0, this.correctHeadRot.eulerAngles.y, 0);
                transform.localScale = this.correctScale;
                head.position = Vector3.Lerp(head.position, this.correctHeadPos, Time.deltaTime * 5);
                head.rotation = Quaternion.Lerp(head.rotation, this.correctHeadRot, Time.deltaTime * 5);
                transform.position = Vector3.Lerp(transform.position, this.CorrectBodyPos, Time.deltaTime * 5);
                transform.rotation = Quaternion.Lerp(transform.rotation, this.CorrectBodyRot, Time.deltaTime * 5);
                right.position = Vector3.Lerp(right.position, this.correctRightPos, Time.deltaTime * 5);
                right.rotation = Quaternion.Lerp(right.rotation, this.correctRightRot, Time.deltaTime * 5);
                left.position = Vector3.Lerp(left.position, this.correctLeftPos, Time.deltaTime * 5);
                left.rotation = Quaternion.Lerp(left.rotation, this.correctLeftRot, Time.deltaTime * 5);
                UpdateHealthBar();
                if (leftGrab != -1)
                {
                    if (lg != null)
                    {
                        GrabLeft();
                    }
                    else
                    {
                        lg = PhotonView.Find(leftGrab);
                        if (lg != null)
                        {
                            Debug.Log("left Grabbing: " + leftGrab);
                            GrabLeft();
                        }
                    }
                }
                else if (lg != null)
                {
                    DropLeft();
                }
                if (rightGrab != -1)
                {
                    if (rg != null)
                    {
                        GrabRight();
                    }
                    else
                    {
                        rg = PhotonView.Find(rightGrab);
                        if (rg != null)
                        {
                            Debug.Log("Right Grabbing: " + rightGrab);
                            GrabRight();
                        }
                    }
                }
                else if (rg != null)
                {
                    DropRight();
                }
            }
            else
            {
                transform.position = myPlayer.head.position - Vector3.up;
                transform.eulerAngles = new Vector3(0, myPlayer.head.eulerAngles.y, 0);
                head.position = myPlayer.head.position;
                head.rotation = myPlayer.head.rotation;
                right.position = myPlayer.right.position;
                right.rotation = myPlayer.right.rotation;
                left.position = myPlayer.left.position;
                left.rotation = myPlayer.left.rotation;
                GameObject lgSet = myPlayer.leftManager.grab.grabbedObject;
                if (lgSet != null)
                {
                    leftGrab = lgSet.GetComponent<PhotonView>().viewID;
                }
                GameObject rgSet = myPlayer.rightManager.grab.grabbedObject;
                if (rgSet != null)
                {
                    rightGrab = rgSet.GetComponent<PhotonView>().viewID;
                }
            }
        }
    }

    public void DropLeft()
    {
        BaseWeapon bwl = lg.GetComponent<BaseWeapon>();
        if (bwl != null)
        {
            lig.grabbedObject.transform.SetParent(null);
            lig.ForceRelease();
            lig.grabbedObject = null;
            bwl.side = "";
            bwl = null;
            lg = null;
        }
    }

    public void DropRight()
    {
        BaseWeapon bwr = rg.GetComponent<BaseWeapon>();
        if (bwr != null)
        {
            rig.grabbedObject.transform.SetParent(null);
            rig.ForceRelease();
            rig.grabbedObject = null;
            bwr.side = "";
            bwr = null;
            rg = null;
        }
    }

    public void GrabLeft()
    {
        if (isDead)
        {
            return;
        }
        BaseWeapon bwl = lg.GetComponent<BaseWeapon>();
        if (bwl != null && lig.grabbedObject == null)
        {
            Debug.Log("Grabbing weapon left");
            bwl.StartTouching(lit);
            lit.SetTouchedObject(bwl.gameObject);
            lig.AttemptGrab();
            //lig.grabEnabledState = 1;
            bwl.side = "l";
        }
    }

    public void GrabRight()
    {
        if (isDead)
        {
            return;
        }
        BaseWeapon bwr = rg.GetComponent<BaseWeapon>();
        if (bwr != null && rig.grabbedObject == null)
        {
            Debug.Log("Grabbing weapon right");
            bwr.StartTouching(rit);
            rit.SetTouchedObject(bwr.gameObject);
            rig.AttemptGrab();
            //rig.grabEnabledState = 1;
            bwr.side = "r";
        }
    }

    public void InitLocal(PlayerManager pm)
    {
        myPlayer = pm;
		maxHealth = pm.health;
		health = pm.health;
        UpdateHealthBar();
        renderer.SetActive(false);
        body.SetActive(false);
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        isMine = true;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(myPlayer.head.position);
            stream.SendNext(myPlayer.head.rotation);
            stream.SendNext(myPlayer.right.position);
            stream.SendNext(myPlayer.right.rotation);
            stream.SendNext(myPlayer.left.position);
            stream.SendNext(myPlayer.left.rotation);
            stream.SendNext(leftGrab);
            stream.SendNext(rightGrab);
            stream.SendNext(health);
            stream.SendNext(transform.localScale);
        }
        else
        {
            // Network player, receive data
            this.correctHeadPos = (Vector3)stream.ReceiveNext();
            this.correctHeadRot = (Quaternion)stream.ReceiveNext();
            this.correctRightPos = (Vector3)stream.ReceiveNext();
            this.correctRightRot = (Quaternion)stream.ReceiveNext();
            this.correctLeftPos = (Vector3)stream.ReceiveNext();
            this.correctLeftRot = (Quaternion)stream.ReceiveNext();
            this.leftGrab = (int)stream.ReceiveNext();
            this.rightGrab = (int)stream.ReceiveNext();
            this.health = (float)stream.ReceiveNext();
            this.correctScale = (Vector3)stream.ReceiveNext();
        }
    }

	[PunRPC]
	public void DropWeapon(string side)
    {
        Debug.Log("shield2: " + side);
        switch (side)
        {
            case "l":
                myPlayer.leftManager.grab.ForceRelease();
                break;
            case "r":
                myPlayer.rightManager.grab.ForceRelease();
                break;
            default:
                break;
        }
    }

    [PunRPC]
	public void CreateProjectile(string pName, Vector3 pPosition, Quaternion pRotation, Quaternion mat, float pSpeed, bool pGravity, bool isMine = false)
    {
        if (isDead)
        {
            return;
        }
        GameObject bulletClone = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(pName), pPosition, pRotation, false, null, false) as GameObject;
        Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
		rb.AddForce(-bulletClone.transform.forward * pSpeed);
		bulletClone.GetComponent<Bullet>().isMine = isMine;
		bulletClone.GetComponent<Bullet>().SetMaterial(mat);
    }

    public void TakeDamage(float damage)
    {
		if (isDead || npc)
        {
            return;
        }
        if (photonView.isMine || NetworkManager.instance.singlePlayer)
        {
            lastDamageTime = Time.time;
            if (health != 0)
            {
                if (health - damage <= 0)
				{
					if (CreateLevel.instance != null) {
						CreateLevel.instance.damageTaken += health;
					}
					health = 0;
                    Die();
                }
                else
				{
					if (CreateLevel.instance != null) {
						CreateLevel.instance.damageTaken += damage;
					}
                    health -= damage;
                }
                if(damage > 0)
                {
                    myPlayer.TakeDamage();
                }
            }
        }
    }

    public void Die()
    {
        isDead = true;
        if (photonView.isMine || NetworkManager.instance.singlePlayer)
        {
            PlayerManager.instance.Die();
        }
    }

    public void Revive()
    {
        isDead = false;
        Heal(maxHealth / 2);
        if (photonView.isMine || NetworkManager.instance.singlePlayer)
        {
            PlayerManager.instance.Revive();
        }
    }

    public void Heal(float healing)
    {
        if (isDead)
        {
            return;
        }
        if (photonView.isMine || NetworkManager.instance.singlePlayer)
        {
            if (health != maxHealth)
            {
                if (health + healing >= maxHealth)
				{
					if (CreateLevel.instance != null) {
						CreateLevel.instance.healing += maxHealth - health;
					}
                    health = maxHealth;
                    FullHealth();
                }
                else
                {
					health += healing;
					if (CreateLevel.instance != null) {
						CreateLevel.instance.healing += healing;
					}
                }
                myPlayer.UpdateHealthFade();
            }
        }
    }

    private void FullHealth()
    {

    }

    public void UpdateHealthBar()
    {
        healthText.text = Mathf.CeilToInt(health).ToString();
        healthText.color = Color.red;
        HOTween.To(healthText, 1, "color", Color.clear, false, EaseType.EaseOutQuad, 5);
	}

	[PunRPC]
	public void SetHead(string setItem){
		bodyPartHead.ChangeItemSlot (setItem);
	}
	[PunRPC]
	public void SetBody(string setItem){
		bodyPartBody.ChangeItemSlot (setItem);
	}
	[PunRPC]
	public void SetHand(string setItem, bool isLeft){
		if (isLeft) {
			bodyPartHandLeft.ChangeItemSlot (setItem);
		} else {
			bodyPartHandRight.ChangeItemSlot (setItem);
		}
	}

    private void OnDestroy()
    {
        if(PlayerManager.instance != null)
        {
            PlayerManager.instance.players.Remove(this);
        }
    }
}