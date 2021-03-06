﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ev_ZombieDeath : MonoBehaviour
{
    public Transform[] Path;
    public Transform viewTarget;
    public AudioSource scientistSound;
    public AudioClip part1, part2;
    public Animator scientist;

    float Timer;
    int state = 0;

    private void OnEnable()
    {
        GameController.instance.player.GetComponent<Player_Control>().ForceWalk(Path);
        GameController.instance.playercache.ForceLook(viewTarget.position, 5f);
        GameController.instance.playercache.allowMove = false;
        GameController.instance.playercache.forceWalkSpeed = 0.5f;

        Timer = 1f;
    }

    private void Update()
    {
        Timer -= Time.deltaTime;

        if (Timer < 0)
        {
            switch (state)
            {
                case 0:
                    {
                        scientistSound.clip = part1;
                        scientistSound.Play();
                        Timer = 7;
                        state = 1;
                        break;
                    }
                case 1:
                    {
                        state = 2;
                        GameController.instance.playercache.FakeBlink(3);
                        Timer = 8f;
                        break;
                    }
                case 2:
                    {
                        scientistSound.clip = part2;
                        scientistSound.Play();
                        Timer = 1f;
                        state = 3;
                        break;
                    }
                case 3:
                    {
                        scientist.SetTrigger("die");
                        GameController.instance.playercache.FakeBlink(5);
                        Timer = 6f;
                        state = 4;
                        break;
                    }
                case 4:
                    {
                        state = 5;
                        GameController.instance.deathmsg = Localization.GetString("deathStrings", "death_008");
                        GameController.instance.playercache.Death(0);
                        break;
                    }

            }
        }
    }

    /* Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
