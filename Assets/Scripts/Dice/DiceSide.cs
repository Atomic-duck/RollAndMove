using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    public bool onGround { get; private set; }
    public int value;

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Table")
        {
            onGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Table")
        {
            onGround = false;
        }
    }
}
