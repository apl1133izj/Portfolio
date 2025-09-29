using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData_Material : MonoBehaviour
{
    public int id;
    public string help;
    public int type;
    public int tier;
    public int grade;
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
        ItemData_MaterialLaod();
    }

    void ItemData_MaterialLaod()
    {
        Item_Material data = ItemManager.GetItemManager.Item_MaterialLoad_Id(int.Parse(gameObject.name));

        if (data != null)
        {
            id = data.id;
            help = data.help;
            type = data.type;
            tier = data.tier;
            grade = data.grade;
            itemname = data.name;
            stackable = data.stackable;
            description = data.description;
            image = data.image;
            farmingRoute = data.farmingRoute;
        }
    }
}
