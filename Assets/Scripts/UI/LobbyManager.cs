using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField roomInput;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private RoomItem roomItemPrefab;
    [SerializeField] private List<RoomItem> roomItems = new List<RoomItem>();
    [SerializeField] private Transform contentObject;

    [SerializeField] private float timeBetweenUpdates = 1.5f;

    [SerializeField] private List<PlayerItem> playerItems = new List<PlayerItem>();
    [SerializeField] private PlayerItem playerItemPrefab;
    [SerializeField] private Transform playerItemParent;

    [SerializeField] private GameObject startButton;

    private float nextUpdateTime;
    private int activePlayerIndex = 0;
    
    void Start()
    {
        // if turn back from previous game, change to room panel
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient) {
                PhotonNetwork.DestroyAll();
                PhotonNetwork.CurrentRoom.IsOpen = true;
            } 

            SetRoom();

        }
        else PhotonNetwork.JoinLobby();
    }

    public void OnClickBack()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ConnectToServer");
    }

    public void OnClickCreate() // create new room
    {
        if(roomInput.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions() { MaxPlayers = 3 });
        }
    }

    public override void OnJoinedRoom()
    {
        SetRoom();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if(Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    private void SetRoom()
    {
        // init room properties
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("activePlayerIndex", activePlayerIndex);
            hashtable.Add("no", 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
        }

        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        List<RoomItem> deleteItem = new List<RoomItem>();

        foreach (RoomInfo room in roomList)
        {
            bool isExist = false;
            // if game finish and room is opened, update roomitem state 
            foreach (RoomItem roomitem in roomItems)
            {
                if (roomitem.getRoomName() == room.Name) {
                    if(room.PlayerCount == 0) deleteItem.Add(roomitem);
                    else
                    {
                        bool open = room.IsOpen && (room.PlayerCount < room.MaxPlayers);
                        roomitem.SetRoomName(room.Name, open);
                    }
                    isExist = true;
                } 
            }

            if (!isExist)
            {
                RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
                bool open = room.IsOpen && (room.PlayerCount < room.MaxPlayers);
                newRoom.SetRoomName(room.Name, open);
                roomItems.Add(newRoom);
            }
    
        }

        // delete unavailabe room
        foreach (RoomItem roomitem in deleteItem)
        {
            Destroy(roomitem.gameObject);
            roomItems.Remove(roomitem);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void UpdatePlayerList()
    {
        foreach(PlayerItem item in playerItems)
        {
            Destroy (item.gameObject);
        }
        playerItems.Clear();

        if(PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player);
            playerItems.Add(newPlayerItem);

            // init player properties
            if (PhotonNetwork.IsMasterClient)
            {
                Hashtable hashtable = new Hashtable();
                hashtable.Add("spawn", i);  // spawn position index
                hashtable.Add("no", -1);    // player NO
                hashtable.Add("turns", 0);  // turns 
                hashtable.Add("bonus", 0);  // bonus times
                hashtable.Add("fail", 0); // fail times

                if (i == activePlayerIndex) hashtable.Add("myTurn", true);
                else hashtable.Add("myTurn", false);

                player.SetCustomProperties(hashtable);
                
                i++;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }


    private void Update()
    {
        if(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void OnClickStartButton()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Game");
    }
}
