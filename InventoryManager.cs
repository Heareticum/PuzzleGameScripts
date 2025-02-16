using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public Transform itemSpawnPos;
    public GameObject itemPrefab;
    public List<DataBase.InventoryItem> inventoryItemList;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        inventoryItemList = new List<DataBase.InventoryItem>();
    }

    public void AddItemToInventory(DataBase.ItemID id, int count)
    {
        if (GetItemData(id) != null)
        {
            DataBase.InventoryItem item = GetItemData(id);
            item.count += count;

            item.script.InitItemData(id, item.count);
        }
        else
        {
            DataBase.InventoryItem newItem = new DataBase.InventoryItem()
            {
                id = id,
                count = count,
            };

            GameObject obj = Instantiate(itemPrefab, itemSpawnPos);
            newItem.script = obj.GetComponent<InventoryItemBehaviour>();
            newItem.script.InitItemData(id, count);

            inventoryItemList.Add(newItem);
        }
    }

    public void RemoveItemFromInventory(DataBase.ItemID id, int count)
    {
        DataBase.InventoryItem item = GetItemData(id);
        item.count -= count;
        item.script.InitItemData(id, item.count);

        if (item.count == 0)
        {
            inventoryItemList.Remove(item);
            Destroy(item.script.gameObject);
        }
    }

    public DataBase.InventoryItem GetItemData(DataBase.ItemID id)
    {
        foreach (var item in inventoryItemList)
        {
            if(item.id == id)
            {
                return item;
            }
        }
        return null;
    }
}
