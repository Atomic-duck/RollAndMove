using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NodeType
{
    None,
    Bonus,
    Fail
}

public class NodeController : MonoBehaviour
{

    public NodeType nodeType;
    [SerializeField] private Color[] nodeColors;
    [SerializeField] private Color[] textColors;
    [SerializeField] private TMP_Text nodeText;
    [SerializeField] private Transform[] standPoints;


    private void Start()
    {
        nodeText.gameObject.SetActive(true);
        Setup();
    }

    private void Setup()
    {
        if(textColors.Length < 3 || textColors.Length < 3 || standPoints.Length < 3)
        {
            Debug.LogError("Not enough elements");
            return;
        }

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        Color textColor = textColors[0];
        Color nodeColor = nodeColors[0];
        string text = string.Empty;

        if (nodeType == NodeType.Bonus)
        {
            text = "Bonus";
            textColor = textColors[1];
            nodeColor = nodeColors[1];
        }
        else if (nodeType == NodeType.Fail)
        {
            text = "Fail";
            textColor = textColors[2];
            nodeColor = nodeColors[2];
        }

        nodeText.text = text;
        nodeText.color = textColor;
        transform.Rotate(Vector3.up, Random.Range(0, 360));
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = nodeColor;
        }
    }

    public Vector3 GetStandPoint(int idx)
    {
        return standPoints[idx].position;
    }
}
