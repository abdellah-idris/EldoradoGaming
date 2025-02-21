﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetwokGameManagement : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("OfficialScene");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        PlayerPrefs.SetInt("joueur", otherPlayer.ActorNumber == 1 ? 2 : 1);
        SceneManager.LoadScene("EndgameScene");
    }
}
