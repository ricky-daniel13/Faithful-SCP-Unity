﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new SNAV", menuName = "Items/SNAV")]
public class Equipable_Nav : Equipable_Elec
{
    public bool isOnline;

    public override void Use(ref gameItem currItem)
    {
        Player_Control player = GameController.instance.player.GetComponent<Player_Control>();

        this.part = bodyPart.Hand;

        if (player.equipment[(int)this.part] == null || ItemController.instance.items[player.equipment[(int)this.part].itemFileName].itemName != this.itemName)
        {
            player.ACT_Equip(currItem);
            SCP_UI.instance.SNav.SetActive(true);
        }
        else
        {
            player.ACT_UnEquip(part);
        }

    }

    public override bool Mix(ref gameItem currItem, ref gameItem toMix)
    {
        if (ItemController.instance.items[toMix.itemFileName].itemName.Equals("bat_nor"))
        {
            currItem.valFloat = 100;
            return (true);
        }
        else
            return (false);
    }

}
