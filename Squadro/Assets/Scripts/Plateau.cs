﻿using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class Plateau : MonoBehaviour
{
    // Reference to the Prefab. Drag a Prefab into this field in the Inspector.
    public Camera mainCam; // 2d dkk
    public Camera cam; //3d
    public GameObject myPrefabRed;
    public GameObject myPrefabYellow;
    public GameObject SelectedPion;
    public GameObject tour;
    public Material redPlayerMaterial;
    public Material yellowPlayerMaterial;
    public AudioSource audio;
    private bool[,] plateau = new bool[7, 7];

    private int[] arrayOfNbCasesDepart = new int[] { 1, 3, 2, 3, 1 };
    private int[] arrayOfNbCasesRotate = new int[] { 3, 1, 2, 1, 3 };

    private Partie partie;
    private Player activePlayer;

    private PhotonView photonView;
    public int localPlayer;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        GameConf c = DataSaver.loadData<GameConf>("gameConf");
        if (c != null)
        {
            mainCam.GetComponent<main_cam>().speed = c.speedCamera;
            audio.volume = c.musicSound;
        }
        else
        {
            mainCam.GetComponent<main_cam>().speed = 5;
            audio.volume = 0.3f;
        }
        float initialValue = 21.4f;
        // Camera Setup
        cam.enabled = true;
        mainCam.enabled = false;

        // Instantiate at position (0, 0, 0) and zero rotation.
        Vector3 v1;

        InitPion[] pionsP1 = new InitPion[5];
        InitPion[] pionsP2 = new InitPion[5];

        for (int i = 0; i < 5; i++)
        {
            v1 = new Vector3(24, 3, initialValue - i * 7);
            pionsP1[4 - i] = Instantiate(myPrefabRed, v1, Quaternion.Euler(0f, 90f, 0f)).GetComponent<InitPion>();
            pionsP1[4 - i].tag = "Pion" + (4 - i + 1).ToString();
            pionsP1[4 - i].GetComponent<InitPion>().joueur = 1;
            pionsP1[4 - i].GetComponent<InitPion>().absolutePosition = v1;
            pionsP1[4 - i].GetComponent<InitPion>().ligne = 4 - i + 1;
            pionsP1[4 - i].GetComponent<InitPion>().colonne = 0;
            pionsP1[4 - i].GetComponent<InitPion>().NbCase = arrayOfNbCasesDepart[i];
            pionsP1[4 - i].GetComponent<InitPion>().absoluteLigne = 4 - i + 1;
            pionsP1[4 - i].GetComponent<InitPion>().absoluteColonne = 0;
        }

        initialValue = 13.4f;

        for (int i = 0; i < 5; i++)
        {
            v1 = new Vector3(initialValue - i * 7, 3, 31);
            pionsP2[i] = Instantiate(myPrefabYellow, v1, Quaternion.identity).GetComponent<InitPion>();
            pionsP2[i].tag = "Pion" + (i + 6).ToString();
            pionsP2[i].GetComponent<InitPion>().joueur = 2;
            pionsP2[i].GetComponent<InitPion>().absolutePosition = v1;
            pionsP2[i].GetComponent<InitPion>().ligne = 6;
            pionsP2[i].GetComponent<InitPion>().colonne = i + 1;
            pionsP2[i].GetComponent<InitPion>().NbCase = arrayOfNbCasesDepart[i];
            pionsP2[i].GetComponent<InitPion>().absoluteLigne = 6;
            pionsP2[i].GetComponent<InitPion>().absoluteColonne = i + 1;
        }
        for (int i = 1; i < 6; i++)
        {
            plateau[i, 0] = true;
            plateau[6, i] = true;
        }
        this.partie = GameObject.FindWithTag("GameController").GetComponent<Partie>();
        Player player1 = new Player(pionsP1);
        this.partie.setPlayer1(player1);
        Player player2 = new Player(pionsP2);
        this.partie.setPlayer2(player2);
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            cam.transform.localPosition = new Vector3(1.282f, 0.819f, -0.015f);
            cam.transform.localRotation = Quaternion.Euler(33.16f, -89.65501f, 0f);
            this.localPlayer = 1;
            this.partie.tourJoueur = new System.Random().Next() % 2 + 1;
            photonView.RPC(nameof(RPC_SetGameTour), RpcTarget.AllBuffered, new object[] { this.partie.tourJoueur });
        }
        else
        {
            cam.transform.localPosition = new Vector3(0.061f, 0.819f, 1.371f);
            cam.transform.localRotation = Quaternion.Euler(33.16f, -177.213f, 0f);

            // cam.transform.position = new Vector3(64.3f, 43.8f, 4.1f);
            localPlayer = 2;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (mainCam.enabled)// si 2d
            {
                mainCam.enabled = false;
                cam.enabled = true;
            }
            else
            {
                mainCam.enabled = true;
                cam.enabled = false;
            }
        }
        else if (Input.GetKeyDown("escape"))
        {
            PlayerPrefs.SetInt("joueur", localPlayer == 1 ? 2 : 1);
            StartCoroutine(WaitForDisconnect());
        }
    }

    private IEnumerator WaitForDisconnect()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
            yield return 0;
        SceneManager.LoadScene("OfficialScene");
    }

    public void DeplacerPion() // A compléter.
    {
        // Get the NbCase available to use in movement.
        InitPion pion = SelectedPion.GetComponent<InitPion>();
        int ligne = pion.ligne;
        int colonne = pion.colonne;
        PureDeplacement(pion);
        SelectedPion = null;
        pion.GetComponent<SelectPion>().UnShowMove(pion.joueur);
        endTurn(pion.joueur, ligne, colonne);
    }

    public void PureDeplacement(InitPion pion) // A compléter.
    {
        // Get the NbCase available to use in movement.
        int NbCase = pion.NbCase;
        int joueur = pion.joueur;
        int ligne = pion.ligne;
        int colonne = pion.colonne;
        bool collision = false;
        bool sauvCollision = false;
        int parcours, deplacement;
        if (joueur == 1)
        {
            //il faut après verifier les rotations.
            if (!pion.rotated)
            {
                parcours = colonne + 1;
                while ((parcours <= colonne + NbCase || collision) && parcours <= 6)
                {
                    sauvCollision = collision;
                    collision = this.plateau[ligne, parcours];
                    parcours++;
                    if (collision)
                    {
                        resetInitialPosition(this.partie.player2.pions, parcours - 2, ligne, parcours - 1);
                    }
                    else if (sauvCollision)
                        break;
                }
                deplacement = parcours - colonne - 1;
                pion.colonne += deplacement;
            }
            else
            {
                parcours = colonne - 1;
                while ((parcours >= colonne - NbCase || collision) && parcours >= 0)
                {
                    sauvCollision = collision;
                    collision = this.plateau[ligne, parcours];
                    parcours--;
                    if (collision)
                        resetInitialPosition(this.partie.player2.pions, parcours, ligne, parcours + 1);
                    else if (sauvCollision)
                        break;
                }
                deplacement = colonne - parcours - 1;
                pion.colonne -= deplacement;
            }
        }
        else
        {
            if (!pion.rotated)
            {
                parcours = ligne - 1;
                while ((parcours >= ligne - NbCase || collision) && parcours >= 0)
                {
                    sauvCollision = collision;
                    collision = this.plateau[parcours, colonne];
                    parcours--;
                    if (collision)
                        resetInitialPosition(this.partie.player1.pions, parcours, parcours + 1, colonne);
                    else if (sauvCollision)
                        break;
                }
                deplacement = ligne - parcours - 1;
                pion.ligne -= deplacement;
            }
            else
            {
                parcours = ligne + 1;
                while ((parcours <= ligne + NbCase || collision) && parcours <= 6)
                {
                    sauvCollision = collision;
                    collision = this.plateau[parcours, colonne];
                    parcours++;
                    if (collision)
                        resetInitialPosition(this.partie.player1.pions, parcours - 2, parcours - 1, colonne);
                    else if (sauvCollision)
                        break;
                }
                deplacement = parcours - ligne - 1;
                pion.ligne += deplacement;
            }
        }
        pion.MovedCase += deplacement;
        pion.transform.Translate(0, 0, -deplacement * 7);
        this.plateau[pion.ligne, pion.colonne] = true;
        this.plateau[ligne, colonne] = false;
        if (pion.MovedCase == 6)
        {
            if (!pion.rotated)
            {
                this.RotatePion(pion);
            }
            else
            {
                pion.DisparitionPion();
                this.incrementNbPionsAllerRetourPlayer(pion.joueur);
            } // le pion a fait un tour.
        }
    }

    private void RotatePion(InitPion pion)
    {
        if (pion.joueur == 1)//rotation joueur 1
        {
            float pos = pion.transform.position.z;
            pion.transform.position = new Vector3(-24f, 3f, pos - 1f);
            pion.RotateMotionP1();
            pion.NbCase = this.arrayOfNbCasesRotate[pion.ligne - 1];
            pion.absoluteColonne = 6;
        }
        else // rotation joueur 2
        {
            float pos = pion.transform.position.x;
            pion.transform.position = new Vector3(pos + 1f, 3f, -17f);
            pion.RotateMotionP2();
            pion.NbCase = this.arrayOfNbCasesRotate[pion.colonne - 1];
            pion.absoluteLigne = 0;
        }
        pion.absolutePosition = pion.transform.position;// setter le point absolue du parcours.
        pion.rotated = true;
        pion.MovedCase = 0;
    }

    private void resetInitialPosition(InitPion[] playerArray, int index, int ligne, int colonne)
    {
        InitPion pion = playerArray[index];
        Vector3 temp = pion.absolutePosition;
        pion.MovedCase = 0;
        playerArray[index].transform.position = temp;
        this.plateau[ligne, colonne] = false;
        this.plateau[pion.absoluteLigne, pion.absoluteColonne] = true;
        pion.ligne = pion.absoluteLigne;
        pion.colonne = pion.absoluteColonne;
    }

    private void incrementNbPionsAllerRetourPlayer(int joueur)
    {
        bool fin;
        if (joueur == 1)
            fin = this.partie.player1.incrementerNbPiecesAndTest();
        else
            fin = this.partie.player2.incrementerNbPiecesAndTest();
        if (fin)
        {
            PlayerPrefs.SetInt("joueur", joueur);
            endGame(joueur);
        }
    }

    public Partie getPartie()
    {
        return this.partie;
    }

    private void endTurn(int joueur, int ligne, int colonne)
    {
        changeTheTourMaterial(this.partie.tourJoueur == 1 ? yellowPlayerMaterial : redPlayerMaterial);
        this.partie.tourJoueur = this.partie.tourJoueur == 1 ? 2 : 1;
        photonView.RPC(nameof(RPC_EndTurn), RpcTarget.OthersBuffered, new object[] { this.partie.tourJoueur, joueur, ligne, colonne });
    }

    [PunRPC]
    private void RPC_EndTurn(int tourJoueur, int joueur, int ligne, int colonne)
    {
        changeTheTourMaterial(this.partie.tourJoueur == 1 ? yellowPlayerMaterial : redPlayerMaterial);
        this.partie.tourJoueur = tourJoueur;
        InitPion pion = GetTheMovedPion(joueur, ligne, colonne);
        PureDeplacement(pion);
    }

    private void endGame(int winner)
    {
        photonView.RPC(nameof(RPC_EndGame), RpcTarget.AllBuffered, new object[] { winner });
    }

    [PunRPC]
    private void RPC_EndGame(int winner)//RPC for game ending
    {
        PlayerPrefs.SetInt("joueur", winner);
        PhotonNetwork.LoadLevel("EndgameScene");
    }

    [PunRPC]
    private void RPC_SetGameTour(int joueur)
    {
        this.partie.tourJoueur = joueur;
        changeTheTourMaterial(this.partie.tourJoueur == 1 ? redPlayerMaterial : yellowPlayerMaterial);
    }
    private InitPion GetTheMovedPion(int joueur, int ligne, int colonne)
    {
        if (joueur == 1)
            return this.partie.player1.getInitPionWithInfo(ligne, colonne);
        return this.partie.player2.getInitPionWithInfo(ligne, colonne);
    }

    private void changeTheTourMaterial(Material material)
    {
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("Sphere");
        foreach (GameObject g in spheres)
        {
            g.GetComponent<Renderer>().material = material;
            g.GetComponent<Renderer>().enabled = true;
        }
        tour.GetComponent<Animator>().SetBool("action", true);
        tour.GetComponent<Animator>().Play("fall");
    }

}
