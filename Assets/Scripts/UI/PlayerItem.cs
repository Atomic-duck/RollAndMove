using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;

    public void SetPlayerInfo(Player player)
    {
        playerName.text = player.NickName;
        playerName.color = Color.black;
    }
}
