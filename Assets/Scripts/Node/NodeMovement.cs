using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeMovement : MonoBehaviour
{
    [SerializeField] private float length = 0.3f;
    [SerializeField] private float speed = 4.5f;
    [SerializeField] private float range = 0.08f;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool toEndPos;
    private bool fluctuate = false;

    private void Start()
    {
        startPos = transform.position;
        endPos = startPos + Vector3.down * length;
        toEndPos = true;
    }

    private void Update()
    {
        if (fluctuate)
        {
            MoveFluctuate();
        }
    }

    private void MoveFluctuate()
    {
        // move to end pos
        if (toEndPos)
        {
            Vector3 moveDir = endPos - transform.position;
            transform.position += moveDir * speed * Time.deltaTime;

            if (Mathf.Abs(transform.position.y - endPos.y) < range) toEndPos = false;
        }
        // move back to start pos
        else
        {
            Vector3 moveDir = startPos - transform.position;
            transform.position += moveDir * speed * Time.deltaTime;

            if (Mathf.Abs(transform.position.y - startPos.y) < 0.01)
            {
                transform.position = startPos;
                fluctuate = false;
                toEndPos = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fluctuate = true;
        }
    }
}
