using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using ExitGames.Client.Photon;
using TMPro;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string[] pfrefabs = { "Character 1", "Character 2", "Character 3" };

    public List<Transform> spawnPositions;
    private GameObject stone;
    private PlayerMovement m_Movement;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public override void OnEnable()
    {
        if (PV == null || !PV.IsMine) return;
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        if (PV == null || !PV.IsMine) return;
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (PV == null || !PV.IsMine) return;

        byte eventCode = photonEvent.Code;
        if (eventCode == 1)
        {
            Debug.Log(PV.ViewID);
            CreateController();
        }
        else if (eventCode == 2)
        {
            EnableControl();
        }
        else if (eventCode == 3)
        {
            DisableControl();
        }
        else if (eventCode == 4)
        {
            DestroyCharacter();
        }
    }

    private void CreateController()
    {
        DestroyCharacter();

        int idx = (int)PhotonNetwork.LocalPlayer.CustomProperties["spawn"];
        if (idx < 0 || idx >= spawnPositions.Count)
        {
            Debug.LogError("spawn index is unvalid");
            return;
        }

        stone = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", pfrefabs[idx]), spawnPositions[idx].position, spawnPositions[idx].rotation, 0, new object[] { idx });

        m_Movement = stone.GetComponent<PlayerMovement>();
    }

    public void DisableControl()
    {
        if (m_Movement != null)
        {
            m_Movement.enabled = false;
        }
    }

    public void EnableControl()
    {
        if (m_Movement != null)
        {
            m_Movement.enabled = true;
        }
    }

    public void DestroyCharacter()
    {
        if (PV == null || stone == null) return;

        PhotonNetwork.Destroy(stone);
        stone = null;
        m_Movement = null;
    }
}
