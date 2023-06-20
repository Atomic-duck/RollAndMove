using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private float m_StartDelay = 3f;
    [SerializeField] private float m_EndDelay = 4f;
    [SerializeField] private float m_PlayDelay = 1f;
    [SerializeField] private TMP_Text m_MessageText;
    [SerializeField] private ScoreTable m_ScoreTable;
    [SerializeField] private GameObject m_QuitButton;

    private bool m_endGame = false;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private WaitForSeconds m_EndPlay;

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        m_EndPlay = new WaitForSeconds(m_PlayDelay);

        StartCoroutine(GameLoop());
    }


    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 2)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        HanldeTurnUpdate(targetPlayer, changedProps);
        HanldeBonusUpdate(targetPlayer, changedProps);
    }

    private void HanldeTurnUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("myTurn"))
        {
            // our player finished turn, 
            if (targetPlayer == PhotonNetwork.LocalPlayer && (bool)changedProps["myTurn"] == false)
            {
                EndTurn();
            }

            // Display whose turn message
            if ((bool)changedProps["myTurn"] == true)
            {
                string message;
                if (targetPlayer == PhotonNetwork.LocalPlayer) message = "Your Turn!!";
                else message = targetPlayer.NickName + " Turn";

                StartCoroutine(DisplayMessage(message));
            }
        }
    }

    private void HanldeBonusUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("bonus"))
        {
            string message;
            if (targetPlayer == PhotonNetwork.LocalPlayer) message = "Bonus Turn!!";
            else message = targetPlayer.NickName + " Get Bonus Turn!!";

            StartCoroutine(DisplayMessage(message));
        }
    }

    private void EndTurn()
    {
        if (AllPlayerReached()) return;

        // Move to the next player that not finish yet
        Player[] players = PhotonNetwork.PlayerList;
        int activePlayerIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["activePlayerIndex"];

        do
        {
            activePlayerIndex = (activePlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
        }while ((int)players[activePlayerIndex].CustomProperties["no"] != -1);

        // Update the properties with the new active player information
        Hashtable playerProp = new Hashtable();
        playerProp["myTurn"] = true;
        PhotonNetwork.PlayerList[activePlayerIndex].SetCustomProperties(playerProp);

        Hashtable roomProp = new Hashtable();
        roomProp["activePlayerIndex"] = activePlayerIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProp);
    }

    public IEnumerator GameLoop()
    {
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());
        yield return StartCoroutine(GameEnding());

        if (m_endGame)
        {
            RaiseEevent();
        }
        else StartCoroutine(GameLoop());
    }

    private IEnumerator GameStarting()
    {
        SpawnAllPlayers();
        DisablePlayerControl();

        m_MessageText.text = $"Starting";

        yield return m_StartWait;
    }


    private IEnumerator GamePlaying()
    {
        EnablePlayerControl();

        m_MessageText.text = string.Empty;
        while (!(m_endGame = AllPlayerReached())) yield return null;

        DisablePlayerControl();
        yield return m_EndPlay;
    }


    private IEnumerator GameEnding()
    {
        m_QuitButton.SetActive(true);
        m_ScoreTable.gameObject.SetActive(true);
        m_ScoreTable.SetTable(PhotonNetwork.PlayerList);

        yield return m_EndWait;
    }

    private bool AllPlayerReached()
    {
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            if ((int)player.Value.CustomProperties["no"] == -1)
            {
                return false;
            }
        }

        return true;
    }

    private void RaiseEevent()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            object[] content = new object[] { };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(4, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private void SpawnAllPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("saas");
            object[] content = new object[] { };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private void EnablePlayerControl()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            object[] content = new object[] { };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(2, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }


    private void DisablePlayerControl()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            object[] content = new object[] { };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(3, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private IEnumerator DisplayMessage(string message)
    {
        m_MessageText.text = message;
        yield return new WaitForSeconds(1);
        m_MessageText.text = string.Empty;
    }

    public void OnClickQuitButton()
    {
        SceneManager.LoadScene("Lobby");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            EndTurn();
        }
    }
}
