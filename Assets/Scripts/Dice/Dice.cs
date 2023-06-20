using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Dice : MonoBehaviourPunCallbacks
{
    public int minForce = 5;     // Minimum force to apply when rolling the dice
    public int maxForce = 10;    // Maximum force to apply when rolling the dice
    public float rotationSpeed = 5f;

    [SerializeField] private DiceSide[] diceSides;
    [SerializeField] private GameObject diceTable;
    [SerializeField] private GameObject table;

    private int diceValue;
    private Rigidbody rb;   
    private bool hasLanded = false; // Flag to check if the dice has landed
    private bool thrown = false;    // Flag to check if the dice has thrown
    private bool notDefine = false; // Flag to check if rolled number is unclear
    private Vector3 initPos;    // relative postion to the dice table

    PhotonView diceView;
    PhotonView diceTableView;
    PhotonView tableView;

    private void Awake()
    {
        tableView = table.GetComponent<PhotonView>();
        diceTableView = diceTable.GetComponent<PhotonView>();
        diceView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        initPos = transform.localPosition;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (diceView == null || !diceView.IsMine) return;

        if (!thrown)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.localPosition = initPos;
        }
        else if (rb.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rb.useGravity = false;
            SideValueCheck();
        }
        else if(rb.IsSleeping() && hasLanded && diceValue == 0)
        {
            notDefine = true;
            Reset();
        }
    }

    public void RollDice()
    {
        if (diceView == null || !diceView.IsMine) return;

        if (!thrown && !hasLanded)
        {
            thrown = true;
            rb.useGravity = true;
            
            // Apply a random force to the dice
            Vector3 force = new Vector3(0f, Random.Range(minForce, maxForce), 0f);
            rb.AddForce(force, ForceMode.Impulse);

            // Apply torque to make the dice rotate
            Vector3 torque = new Vector3(Random.Range(-rotationSpeed, rotationSpeed), Random.Range(-rotationSpeed, rotationSpeed), Random.Range(-rotationSpeed, rotationSpeed));
            rb.AddTorque(torque, ForceMode.Impulse);
        }
        else if(thrown && hasLanded)
        {
            Reset();
        }
    }

    private void Reset()
    {
        thrown = false;
        hasLanded = false;
        rb.useGravity = false;
    }

    private void SideValueCheck()
    {
        diceValue = 0;
        foreach(DiceSide side in diceSides)
        {
            if (side.onGround)
            {
                diceValue = side.value;
                notDefine = false;
            }
        }
    }

    public int GetRollingResult()
    {
        if(diceValue == 0) return 0;

        int res = diceValue;
        diceValue = 0;
        return res;
    }

    public bool ResultIsNotDefined()
    {
        return notDefine;
    }

    public void SetTablePosition(Vector3 pos)
    {
        if (diceView == null || !diceView.IsMine) return;

        table.gameObject.SetActive(true);
        diceTable.transform.position = pos;
        transform.localPosition = initPos;
    }

    public bool IsMine()
    {
        return diceView.IsMine;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (diceView == null || !diceView.IsMine) return;

        if (changedProps.ContainsKey("myTurn"))
        {
            if ((bool)changedProps["myTurn"] == true)
            {
                diceTableView.TransferOwnership(targetPlayer);
                diceView.TransferOwnership(targetPlayer);
                tableView.TransferOwnership(targetPlayer);
            }
        }
    }
}
