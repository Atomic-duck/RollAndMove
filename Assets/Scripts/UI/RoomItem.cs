using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private Color[] colors;
    
    private LobbyManager manager;

    private void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomName(string _roomName, bool open)
    {
        roomName.text = _roomName;

        if(open) roomName.color = colors[0];
        else roomName.color = colors[1];
    }

    public void OnClickItem()
    {
        manager.JoinRoom(roomName.text);
    }

    public string getRoomName()
    {
        return roomName.text;
    }
}
