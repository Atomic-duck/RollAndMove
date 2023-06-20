using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class ScoreTable : MonoBehaviour
{
    [SerializeField] private float templateHeight = 40f;
    private Transform entryContainer;
    private Transform entryTemplate;

    private void Awake()
    {
        entryContainer = transform.Find("EntryContainer");
        entryTemplate = entryContainer.Find("EntryTemplate");

        entryTemplate.gameObject.SetActive(false);
    }

    public void SetTable(Player[] players)
    {
        // Sort players list increasing by 'NO' property
        for(int i = 1; i < players.Length; i++)
        {
            for(int j = i; j > 0; j--)
            {
                if((int)players[j].CustomProperties["no"] < (int)players[j - 1].CustomProperties["no"])
                {
                    Player temp = players[j];
                    players[j] = players[j - 1];
                    players[j - 1] = temp;
                }
            }
        }

        // create entry for each player
        for (int i = 0; i < players.Length; i++)
        {
            CreateEntryTransform(players[i], i);
        }
    }

    private void CreateEntryTransform(Player player, int idx)
    {
        Transform entryTransform = Instantiate(entryTemplate, entryContainer);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * idx);
        entryTransform.gameObject.SetActive(true);

        // set rank string
        int rank = idx + 1;
        string rankString;
        switch (rank)
        {
            case 1: rankString = "1ST"; break;
            case 2: rankString = "2ND"; break;
            case 3: rankString = "3RD"; break;

            default: rankString = rank + "TH"; break;
        }

        // Set text
        entryTransform.Find("No").GetComponent<TMP_Text>().text = rankString;
        entryTransform.Find("Player Name").GetComponent<TMP_Text>().text = player.NickName;
        entryTransform.Find("Turns").GetComponent<TMP_Text>().text = player.CustomProperties["turns"].ToString();
        entryTransform.Find("Bonus Sectors").GetComponent<TMP_Text>().text = player.CustomProperties["bonus"].ToString();
        entryTransform.Find("Fail Sectors").GetComponent<TMP_Text>().text = player.CustomProperties["fail"].ToString();

        // Set background odd and even
        entryTransform.Find("Background").gameObject.SetActive(rank % 2 == 1);

        // Highlight No.1
        if(rank == 1)
        {
            entryTransform.Find("No").GetComponent<TMP_Text>().color = Color.green;
            entryTransform.Find("Player Name").GetComponent<TMP_Text>().color = Color.green;
            entryTransform.Find("Turns").GetComponent<TMP_Text>().color = Color.green;
            entryTransform.Find("Bonus Sectors").GetComponent<TMP_Text>().color = Color.green;
            entryTransform.Find("Fail Sectors").GetComponent<TMP_Text>().color = Color.green;
        }
    }
}
