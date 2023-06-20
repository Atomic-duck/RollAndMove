using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerMovement : MonoBehaviour
{
    public int steps;
    public bool isMoving = false;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private CinemachineVirtualCameraBase myCamera;

    private Route currentRoute;
    private Dice dice;

    private int curNodeIdx = 0;
    private bool thrown = false;
    private bool isDelay = false;
    private bool notSet = true;
    private int id;

    private PhotonView m_View;

    private void Awake()
    {
        m_View = GetComponent<PhotonView>();
        if (m_View != null && m_View.IsMine)
        {
            // Add player camera to enable to switch
            FindObjectOfType<CameraSwitcher>().AddCamera(myCamera, true);
            id = (int)m_View.InstantiationData[0];
        }
    }

    private void Start()
    {
        // Get route and dice
        currentRoute = FindObjectOfType<Route>();
        dice = FindObjectOfType<Dice>();

        // Set start position
        if (currentRoute)
        {
            transform.position = currentRoute.GetNode(curNodeIdx).GetStandPoint(id) + 0.3f / 2 * Vector3.up;
        }
    }

    private void Update()
    {
        if (m_View != null && !m_View.IsMine) return;
        // If not player turn, do nothing
        if (!IsMyTurn())
        {
            thrown = false;
            notSet = true;
            return;
        }

        // check if dice is transfered onwer
        if (dice.IsMine())
        {
            HandleRollDiceInput();
            HandleRolledNumber();
        }
    }

    private void FixedUpdate()
    {
        if (m_View != null && !m_View.IsMine) return;
        if (isDelay || !isMoving)
        {
            transform.position = GetRelativePositionToNode(curNodeIdx);
        }
    }

    IEnumerator Move()
    {
        if (isMoving)
        {
            yield break;
        }
        isMoving = true;

        yield return StartCoroutine(MoveSteps(steps, true));
        yield return StartCoroutine(CheckNodeAfterArrived());
        
        isMoving = false;
        steps = 0;
    }

    IEnumerator MoveSteps(int steps, bool toForward)
    {
        while (steps > 0)
        {
            if(toForward) curNodeIdx++;
            else
            {
                if (curNodeIdx == 0) break;
                curNodeIdx--;
            }

            float time = 0;
            Vector3 start = transform.position;
            Vector3 nextPos = GetRelativePositionToNode(curNodeIdx);
            Vector3 inter = transform.position + (nextPos - transform.position) / 2 + Vector3.up * 2;

            // Make Bezier move
            yield return BezierMove(start, inter, nextPos, time);

            // player arrived destination, mark delay to enable player position update to follow current node position (node fluctuates after collision)
            isDelay = true;

            // Rotate player to next node
            if(curNodeIdx + 1 < currentRoute.numNode)
            {
                Vector3 direction = currentRoute.GetNode(curNodeIdx).transform.position - currentRoute.GetNode(curNodeIdx+1).transform.position;
                while (!TurnToDirection(direction)) yield return null;
            }

            // waitting for x second after reached node
            yield return new WaitForSeconds(0.15f);

            isDelay = false;
            steps--;
        }
    }

    IEnumerator CheckNodeAfterArrived()
    {
        Hashtable playerProp = new Hashtable();
        // fail node and regular node
        if (currentRoute.GetNode(curNodeIdx).nodeType != NodeType.Bonus || curNodeIdx == currentRoute.numNode - 1)
        {
            // Notify finish turn
            playerProp.Add("myTurn", false);

            // if current node's type is fail and not end node, update faile sector and move back 3 steps
            if (currentRoute.GetNode(curNodeIdx).nodeType == NodeType.Fail && curNodeIdx != currentRoute.numNode - 1)
            {
                playerProp.Add("fail", (int)PhotonNetwork.LocalPlayer.CustomProperties["fail"] + 1);

                yield return new WaitForSeconds(0.3f);
                yield return StartCoroutine(MoveSteps(3, false));
            }

            // if reach the end node
            if (curNodeIdx == currentRoute.numNode - 1)
            {
                // set 'no' property of player
                playerProp.Add("no", (int)PhotonNetwork.CurrentRoom.CustomProperties["no"] + 1);

                // update 'no' property of room
                Hashtable roomProp = new Hashtable();
                roomProp.Add("no", (int)PhotonNetwork.CurrentRoom.CustomProperties["no"] + 1);
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProp);
            }
        }
        // bonus node
        else
        {
            thrown = false;
            notSet = true;

            // update bonus sector
            playerProp.Add("bonus", (int)PhotonNetwork.LocalPlayer.CustomProperties["bonus"] + 1);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProp);
    }

    public IEnumerator BezierMove(Vector3 start, Vector3 inter, Vector3 nextPos, float time)
    {
        while (MoveToPosition(start, inter, nextPos, time))
        {
            time += 2f * Time.deltaTime;
            if (time > 1) time = 1;

            yield return null;
        }
    }
    private bool IsMyTurn()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return false;

        return (bool)PhotonNetwork.LocalPlayer.CustomProperties["myTurn"];
    }
    private void HandleRollDiceInput()
    {
        // If dice wasn't moved to player position, move it
        if (notSet)
        {
            dice.SetTablePosition(transform.position + Vector3.up);
            notSet = false;
            return;
        }

        // Check roll dice input
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving && !thrown)
        {
            dice.RollDice();
            thrown = true;
        }
    }
    private void HandleRolledNumber()
    {
        // If dice was rolled and has result
        if ((steps = dice.GetRollingResult()) != 0)
        {
            // Valid Rolled Number
            if (curNodeIdx + steps < currentRoute.numNode)
            {
                StartCoroutine("Move");
            }
            // Rolled Number is to high
            else
            {
                // Notify player has finished turn
                SetPlayerProperty("myTurn", false);
            }

            // Increase player's turns
            SetPlayerProperty("turns", (int)PhotonNetwork.LocalPlayer.CustomProperties["turns"] + 1);
        }
        // Dice was thrown but the result is ambiguous or unclear
        else if (dice.ResultIsNotDefined())
        {
            thrown = false;
        }
    }

    private void SetPlayerProperty(object key, object value)
    {
        Hashtable playerProps = new Hashtable();
        playerProps.Add(key, value);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    private Vector3 GetRelativePositionToNode(int idx)
    {
        if(currentRoute == null) return Vector3.zero;

        return currentRoute.GetNode(idx).GetStandPoint(id) + 0.3f / 2 * Vector3.up;
    }

    private bool TurnToDirection(Vector3 direction)
    {
        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float degree = Quaternion.Angle(transform.rotation, targetRotation);

        if (degree < 60) return true;

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        return transform.rotation == targetRotation;
    }

    private bool MoveToPosition(Vector3 start, Vector3 inter, Vector3 goal, float time)
    {
        return goal != (transform.position = GetBezierPosition(start, inter, goal, time));
    }

    private Vector3 GetBezierPosition(Vector3 p_start, Vector3 p_inter, Vector3 p_end, float t)
    {
        Vector3 point = Mathf.Pow(1 - t, 2) * p_start + 2 * (1 - t) * t * p_inter + t * t * p_end;
        return point;
    }
}
