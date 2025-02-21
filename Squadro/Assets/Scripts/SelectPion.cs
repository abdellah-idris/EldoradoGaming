﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPion : MonoBehaviour
{
    public Material m_base_red;
    public Material m_selection_red;
    public Material m_base_yellow;
    public Material m_selection_yellow;
    public bool selected = false;
    public GameObject PionRedWithLowAlpha;
    public GameObject PionYellowWithLowAlpha;
    private Plateau plateau;

    // Start is called before the first frame update
    void Start()
    {
        plateau = GameObject.FindWithTag("Plateau").GetComponent<Plateau>();
    }

    void OnMouseDown()
    {
        if (this.plateau.getPartie().tourJoueur == this.GetComponent<InitPion>().joueur && this.plateau.getPartie().tourJoueur == this.plateau.localPlayer)
        {
            if (selected)
            {
                this.plateau.DeplacerPion();
                selected = false;
            }
            else
            {
                if (this.plateau.getPartie().tourJoueur == 1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        this.plateau.getPartie().player1.pions[i].GetComponent<SelectPion>().selected = false;
                        this.plateau.getPartie().player1.pions[i].GetComponent<SelectPion>().UnShowMove(1);
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {

                        this.plateau.getPartie().player2.pions[i].GetComponent<SelectPion>().selected = false;
                        this.plateau.getPartie().player2.pions[i].GetComponent<SelectPion>().UnShowMove(2);
                    }
                }
                selected = true;
                plateau.SelectedPion = GameObject.FindWithTag(this.tag);
                ShowMove();
            }
        }
    }

    void ShowMove()
    {
        InitPion pion = this.plateau.SelectedPion.GetComponent<InitPion>();
        this.GetComponent<Renderer>().material = pion.joueur == 1 ? m_selection_red : m_selection_yellow;
        float t = this.transform.rotation.y;
        string tag_p = this.tag + "alpha";
        int deplacement = pion.MovedCase + pion.NbCase <= 6 ? pion.NbCase : 6 - pion.MovedCase;
        // check the rotation
        if (pion.joueur == 1 && !pion.rotated)
        {
            float x = this.transform.position.x - (deplacement * 7);
            float z = this.transform.position.z;
            Instantiate(PionRedWithLowAlpha, new Vector3(x, 3, z), Quaternion.Euler(0f, 90f, 0f)).tag = tag_p;
        }
        else if (pion.joueur == 1 && pion.rotated)
        {
            float x = this.transform.position.x + (deplacement * 7);
            float z = this.transform.position.z;
            Instantiate(PionRedWithLowAlpha, new Vector3(x, 3, z), Quaternion.Euler(0f, -90f, 0f)).tag = tag_p;
        }
        else if (pion.joueur == 2 && !pion.rotated)
        {
            float x = this.transform.position.x;
            float z = this.transform.position.z - (deplacement * 7);
            Instantiate(PionYellowWithLowAlpha, new Vector3(x, 3, z), Quaternion.identity).tag = tag_p;
        }
        else
        {
            float x = this.transform.position.x;
            float z = this.transform.position.z + (deplacement * 7);
            Instantiate(PionYellowWithLowAlpha, new Vector3(x, 3, z), Quaternion.Euler(0f, 180f, 0f)).tag = tag_p;
        }
    }

    public void UnShowMove(int joueur)
    {
        string tag_p = this.tag + "alpha";
        this.GetComponent<Renderer>().material = joueur == 1 ? m_base_red : m_base_yellow;
        Destroy(GameObject.FindWithTag(tag_p));
    }

}
