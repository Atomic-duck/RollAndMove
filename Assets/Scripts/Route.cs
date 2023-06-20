using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    public int numNode { get; private set; }
    private List<NodeController> nodeList;

    private void Start()
    {
        nodeList = new List<NodeController>();
        FillNodes();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        FillNodes();

        for(int i = 1; i < nodeList.Count; i++)
        {
            Vector3 curPos = nodeList[i].transform.position;
            Vector3 prePos = nodeList[i-1].transform.position;
            Gizmos.DrawLine(prePos, curPos);
        }
    }

    private void FillNodes()
    {
        nodeList.Clear();
        NodeController[] childObjects = GetComponentsInChildren<NodeController>();

        foreach(NodeController child in childObjects)
        {
            if(child != this.transform)
            {
                nodeList.Add(child);
            }
        }

        numNode = nodeList.Count;
    }

    public NodeController GetNode(int idx)
    {
        if (idx < 0 || idx >= nodeList.Count) return null;

        return nodeList[idx];
    }
}
