using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MoveTest
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator BezierMove()
    {
        var gameObject = new GameObject();
        var playerMovement = gameObject.AddComponent<PlayerMovement>();

        float time = 0;
        Vector3 start = new Vector3(1,0,1);
        Vector3 nextPos = new Vector3(3,0,3);
        Vector3 inter = new Vector3(4, 5, 1);

        playerMovement.isMoving = true;
        yield return playerMovement.BezierMove(start, inter, nextPos, time);

        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(nextPos, playerMovement.transform.position, "The values should be equal.");
    }
}
