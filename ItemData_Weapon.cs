using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData_Weapon : MonoBehaviour
{
    public int id;
    public string help;
    public int type;
    public int subType;
    public int tier;
    public int grade;
    public int atack;
    public int defense;
    public int speed;
    public int durability;
    public int stackable;
    public string itemname;
    public string description;
    public string image;
    public string farmingRoute;
    void Start()
    {
        string originalName = gameObject.name;

        if (originalName.Length >= 8)
        {
            gameObject.name = originalName.Substring(0, 8);
        }
        else
        {
            gameObject.name = originalName; // 8자 미만이면 그대로
        }
        ItemData_WeaponsLaod();
    }
    void ItemData_WeaponsLaod()
    {
        Item_Weapon data = ItemManager.GetItemManager.Item_WeaponLoad_Id(int.Parse(gameObject.name));

        if (data != null)
        {
            id = data.id;
            help = data.help;
            type = data.type;
            subType = data.subType;
            tier = data.tier;
            grade = data.grade;
            atack = data.atack;
            defense = data.defense;
            speed = data.speed;
            durability = data.durability;
            stackable = data.stackable;
            itemname = data.name;
            description = data.description;
            image = data.image;
            farmingRoute = data.farmingRoute;
        }
    }
}
