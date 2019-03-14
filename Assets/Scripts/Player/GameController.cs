﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraPool
    {
        public Material Mats;
        public RenderTexture Renders;
        public bool isUsing;
    }

public class GameController : MonoBehaviour
{
    public static GameController instance = null;

    public bool CreateMap;
    public bool SpawnMap;

    int xPlayer, yPlayer;

    public GameObject player;
    public GameObject scp173, startEv;
    public NewMapGen mapCreate;
    SCP_173 con_173;

    public Vector3 WorldAnchor;

    int xStart, xEnd, yStart, yEnd;
    bool CullerFlag;
    bool CullerOn, changeTrack, changed, swapAmbiance;
    float roomsize = 15.3f, ambiancetimer=0, Timer = 5, normalAmbiance, ambiancefreq;
    public float ambifreq;

    room_dat[,] SCP_Map;

    MapSize mapSize;
    int[,,] culllookup;
    int[,] Binary_Map;

    public bool doGameplay, spawnPlayer, spawnHere, spawn173, StopTimer = false;
    public Transform place173, playerSpawn;

    public AudioSource Music;
    public AudioSource Ambiance;
    public AudioSource MixAmbiance;
    public AudioSource Horror;


    public AudioClip[] AmbianceLibrary;
    public AudioClip [] PreBreach;
    public AudioClip[] Z1;
    AudioClip trackTo;

    public CameraPool [] cameraPool;



    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
    }



    void Start()
    {
        AmbianceLibrary = PreBreach;
        CullerFlag = false;
        CullerOn = false;

        if (CreateMap)
        {
                SCP_Map = mapCreate.CreaMundo();

                mapSize = mapCreate.mapSize;
                roomsize = mapCreate.roomsize;

                mapCreate.MostrarMundo();

            
            //postest = Instantiate(postest, new Vector3(roomsize * (mapSize.xSize / 2), 1.0f, roomsize * (mapSize.ySize - 1)), Quaternion.identity);
            Binary_Map = mapCreate.MapaBinario();

            culllookup = new int[mapSize.xSize, mapSize.ySize, 2];
            int i, j;
            for (i = 0; i < mapSize.xSize; i++)
            {
                for (j = 0; j < mapSize.ySize; j++)
                {
                    culllookup[i, j, 0] = 0;
                    culllookup[i, j, 1] = 0;
                }
            }
            StartCoroutine(HidAfterProbeRendering());
        }

        if (spawnPlayer)
        {
            if (!spawnHere)
                player = Instantiate(player, new Vector3(roomsize * (mapSize.xSize / 2), 1.0f, roomsize * (mapSize.ySize - 1)), Quaternion.identity);
            else
                player = Instantiate(player, playerSpawn.position, Quaternion.identity);
        }

        if (spawn173)
        {
            scp173 = Instantiate(scp173, place173.position, Quaternion.identity);
            con_173 = scp173.GetComponent<SCP_173>();
        }



    }

    void Update()
    {
        StartIntro();


        if (doGameplay)
            Gameplay();

        if (changeTrack == true)
            MusicChanging();

        DoAmbiance();




    }

    public void Warp173(bool beActive, Transform Here)
    {
        con_173.WarpMe(beActive, Here.position);
    }

    void DoAmbiance()
    {

        ambiancetimer -= Time.deltaTime;
        if (ambiancetimer <= 0)
        {
            MixAmbiance.PlayOneShot(AmbianceLibrary[Random.Range(0, AmbianceLibrary.Length)]);
            ambiancetimer = ambiancefreq * Random.Range(1, 5);
        }
    }

    public void ChangeAmbiance(AudioClip [] NewAmbiance, float freq)
    {
        AmbianceLibrary = NewAmbiance;
        ambiancefreq = freq;
        swapAmbiance = true;
    }
    public void DefaultAmbiance()
    {
        swapAmbiance = false;
    }




    public void ChangeMusic(AudioClip newMusic)
    {
        changeTrack = true;
        trackTo = newMusic;
        changed = false;
    }

    void MusicChanging()
    {
        if (changed == false)
            Music.volume -= (Time.deltaTime)/4;

        if (Music.volume <= 0.1 && changed == false)
        {
            changed = true;
            Music.clip = trackTo;
            Music.Play();
        }

        if (changed == true)
            Music.volume += Time.deltaTime;

        if (Music.volume >= 0.9 && changed == true)
        {
            changeTrack = false;
        }


    }

    public void PlayHorror(AudioClip horrorsound)
    {
        Horror.PlayOneShot(horrorsound);
    }


    void Gameplay()
    {
        xPlayer = (Mathf.Clamp((Mathf.RoundToInt((player.transform.position.x / roomsize))), 0, mapSize.xSize - 1));
        yPlayer = (Mathf.Clamp((Mathf.RoundToInt((player.transform.position.z / roomsize))), 0, mapSize.ySize - 1));
        //Debug.Log("Posicion X= " + xPlayer + " Posicion Y= " + yPlayer + " Hay cuarto? " + Binary_Map[xPlayer, yPlayer]);

        PlayerEvents();

        if (CullerFlag == true && CullerOn == false)
        {
            StartCoroutine(RoomHiding());
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            CullerFlag = !CullerFlag;
        }
    }

    public Vector3 GetPatrol(Vector3 MyPos)
    {
        int xPos = (Mathf.Clamp((Mathf.RoundToInt((MyPos.x / roomsize))), 0, mapSize.xSize-1));
        int yPos = (Mathf.Clamp((Mathf.RoundToInt((MyPos.z / roomsize))), 0, mapSize.ySize-1));
        Debug.Log("Recibi Posicion X= " + xPos + " Posicion Y= " + yPos);
        Debug.Log("Posicion X= " + xPlayer + " Posicion Y= " + yPlayer + " Hay cuarto? " + Binary_Map[xPlayer, yPlayer]);

        int xPatrol, yPatrol;

        do
        {
            xPatrol = Random.Range(Mathf.Clamp(xPos - 3, 0, mapSize.xSize-1), Mathf.Clamp(xPos + 3, 0, mapSize.xSize-1));
            yPatrol = Random.Range(Mathf.Clamp(yPos - 3, 0, mapSize.ySize-1), Mathf.Clamp(yPos + 3, 0, mapSize.ySize-1));
        }
        while (Binary_Map[xPatrol, yPatrol] == 0);

        Debug.Log("Otorgue Posicion X= " + xPatrol + " Posicion Y= " + yPatrol);

        return (new Vector3(xPatrol * roomsize, 0.0f, yPatrol * roomsize));
    }

    void StartIntro()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0.0f && StopTimer == false)
        {
            startEv.SetActive(true);
            StopTimer = true;
        }
    }







    void PlayerEvents()
    {
        if (Binary_Map[xPlayer, yPlayer]!= 0 && ((SCP_Map[xPlayer, yPlayer].hasEvents == true || SCP_Map[xPlayer, yPlayer].hasSpecial == true))&& SCP_Map[xPlayer, yPlayer].eventDone == false)
        {
            SCP_Map[xPlayer, yPlayer].RoomHolder.GetComponent<EventHandler>().EventStart();
            SCP_Map[xPlayer, yPlayer].eventDone = true;
        }
    }


    void HidRoom(int i, int j)
    {
        Renderer[] rs = SCP_Map[i, j].RoomHolder.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
            r.enabled = false;
    }

    void ShowRoom(int i, int j)
    {
        Renderer[] rs = SCP_Map[i, j].RoomHolder.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
            r.enabled = true;
    }

    IEnumerator HidAfterProbeRendering()
    {
        yield return new WaitForSeconds(20);
        int i, j;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if ((SCP_Map[i, j].empty == false))      //Imprime el mapa
                    HidRoom(i, j);
            }
        }
        CullerFlag = true;
    }


    IEnumerator RoomHiding()
    {
        CullerOn = true;
        int i, j;
        xStart = Mathf.Clamp(xPlayer - 2, 0, mapSize.xSize);
        xEnd = Mathf.Clamp(xPlayer + 2, 0, mapSize.xSize);
        yStart = Mathf.Clamp(yPlayer - 2, 0, mapSize.ySize);
        yEnd = Mathf.Clamp(yPlayer + 2, 0, mapSize.ySize);

        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                culllookup[i, j, 0] = 0;
            }
        }

        for (i = xStart; i < xEnd; i++)
        {
            for (j = yStart; j < yEnd; j++)
            {
                if ((Binary_Map[i, j] == 1))      //Imprime el mapa
                {
                    if (culllookup[i, j, 1] == 1)
                        culllookup[i, j, 0] = 1;
                    else
                    {
                        //Debug.Log("Showing Room at x" + i + " y " + j);
                        ShowRoom(i, j);
                        culllookup[i, j, 1] = 1;
                        culllookup[i, j, 0] = 1;
                    }
                }
            }
        }

        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if (culllookup[i, j, 0] == 1)
                    culllookup[i, j, 1] = 1;
                if (culllookup[i, j, 0] == 0)
                {
                    if (culllookup[i, j, 1] == 1)
                    {
                        HidRoom(i, j);
                        culllookup[i, j, 1] = 0;
                        //Debug.Log("Hiding Room at x" + i + " y " + j);
                    }
                }
            }
        }

        //Debug.Log("Culling Routine ended, waiting for next start");
        yield return null;
        CullerOn = false;
    }

}