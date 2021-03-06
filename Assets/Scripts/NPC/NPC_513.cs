﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Forward is Y+
//Right is X+

public class NPC_513 : SimpleNPC
{
    public Transform ghostModel;
    public float minSpawn, maxSpawn, waitForSeen, speed;
    public int framerate;

    public float speedUpDown = 1;
    public float distanceUpDown = 1, distanceLeftRight, distanceForwardBackwards;

    public LayerMask col;

    public AudioClip bell;

    float Timer, random1, random2;
    bool seen = false;

    public void Start()
    {
        random1 = Random.Range(-1.5f, 1.5f);
        random2 = Random.Range(-1.5f, 1.5f);
    }

    public override void NPCUpdate()
    {
        base.NPCUpdate();

        transform.rotation = Quaternion.Inverse(GameController.instance.playercache.transform.rotation);
        ghostModel.position = new Vector3(transform.position.x + (Mathf.Sin(speedUpDown * (Time.time * random1)) * distanceLeftRight), transform.position.y + (Mathf.Sin(speedUpDown * Time.time) * distanceUpDown), transform.position.z + ((Mathf.Sin(speedUpDown * (Time.time * random2))) * distanceForwardBackwards));

        Timer -= Time.deltaTime;
        if (Timer < 0 && !seen)
        {
            float dotF = Vector3.Dot(Vector3.forward, GameController.instance.playercache.transform.forward);
            float dotR = Vector3.Dot(Vector3.right, GameController.instance.playercache.transform.forward);

            bool invertRight = false, invertForw = false, goUp = true;

            if (dotF < 0)
            {
                dotF = -dotF;
                invertForw = true;
            }
            if (dotR < 0)
            {
                dotR = -dotR;
                invertRight = true;
            }

            if (dotR > dotF)
                goUp = false;

            transform.position = new Vector3((goUp ? (GameController.instance.roomsize * GameController.instance.xPlayer) : (GameController.instance.roomsize * (GameController.instance.xPlayer + (invertRight ? -2 : 2)))), 0, (!goUp ? (GameController.instance.roomsize * GameController.instance.yPlayer) : (GameController.instance.roomsize * (GameController.instance.yPlayer + (invertForw ? -2 : 2)))));

            Timer = waitForSeen;

        }

        //Debug.DrawRay(transform.position + Vector3.up, ((GameController.instance.playercache.transform.position + Vector3.up)- (transform.position + Vector3.up)));

        if (!seen && Time.frameCount % framerate == 0)
        {
            float dis = Vector3.Distance(GameController.instance.playercache.transform.position, transform.position);
            if (dis < 14 && (!Physics.Raycast(transform.position + Vector3.up, ((GameController.instance.playercache.transform.position + Vector3.up)- (transform.position + Vector3.up)).normalized, dis, col)))
            {
                seen = true;
                Timer = 10f;
                GameController.instance.PlayHorror(bell, transform, npc.none);
            }
        }

        if (seen)
        {
            transform.position += (GameController.instance.playercache.transform.forward * speed) * Time.deltaTime;
            if (Timer < 0)
            {
                transform.position = new Vector3(0, -100, 0);
                Timer = Random.Range(minSpawn, maxSpawn);
                seen = false;
            }
        }
    }

    
}
