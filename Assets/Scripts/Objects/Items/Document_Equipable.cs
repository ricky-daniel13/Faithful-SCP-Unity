﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Document", menuName = "Items/Document")]
public class Document_Equipable : Equipable_Wear
{
    public string filename;

    public override void Use(ref gameItem currItem)
    {
        Player_Control player = GameController.instance.player.GetComponent<Player_Control>();

        if (player.equipment[(int)this.part] == null || ItemController.instance.items[player.equipment[(int)this.part].itemFileName].itemName != this.itemName)
        {
            this.Overlay = Resources.Load<Sprite>(string.Concat("Items/Docs/", filename));
            //doc372
            if (this.itemName == "doc372")
                SCP_UI.instance.bottomScrible.text = GameController.instance.globalStrings[0];
            this.part = bodyPart.Hand;
            player.ACT_Equip(currItem);
        }
        else
        {
            //Resources.UnloadAsset(this.Overlay);
            player.ACT_UnEquip(part);
        }

    }

}
