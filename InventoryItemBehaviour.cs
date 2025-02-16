using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;
using UnityEngine.EventSystems;

public class InventoryItemBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isProxy;
    public DataBase.ItemID itemID;
    public Image itemImage;
    public Text itemCount;
    private GameObject proxyObject;
    private Color color_HalfTransparent = new Color(255, 255, 255, 0.4f);

    public void InitItemData(DataBase.ItemID id, int count)
    {
        isProxy = false;
        itemID = id;
        itemImage.sprite = Level1Manager.instance.GetItemImage(id, true);
        itemCount.text = count > 1 ? count.ToString() : "";
    }
    public void InitItemData(DataBase.ItemID id, int count, bool _isProxy)
    {
        isProxy = _isProxy;
        itemID = id;
        itemImage.sprite = Level1Manager.instance.GetItemImage(id, true);
        itemCount.text = count > 1 ? count.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isProxy)
        {
            //�ͦ����N����
            proxyObject = Instantiate(gameObject, Level1Manager.instance.backPackObj.transform);
            InventoryItemBehaviour obj = proxyObject.GetComponent<InventoryItemBehaviour>();
            obj.InitItemData(itemID, 1, true);
            //�令�b�z��
            itemImage.color = color_HalfTransparent;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isProxy)
        {
            proxyObject.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isProxy)
        {
            //�������N����
            Destroy(proxyObject.gameObject);
            //��^���`�C��
            itemImage.color = Color.white;
            //�����즲�ƥ�
            if (!CheckEndDragEvent(eventData))
            {
                gameObject.transform.SetParent(InventoryManager.instance.itemSpawnPos);
            }
        }
    }

    private bool CheckEndDragEvent(PointerEventData eventData)
    {
        bool returnValue = false;

        //UI���󰻴�
        List<RaycastResult> results = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (eventData, results);
        foreach (RaycastResult result in results)
        {
            InteractableObjects value = result.gameObject.GetComponent<InteractableObjects>();
            if (value  != null)
            {
                returnValue = value.EndDragEvent(this);
            }
        }

        return returnValue;

        //2D���󰻴�
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, -1); ;
        //if (hit.collider)
        //{
        //    Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 0.1f, true);
        //    Debug.Log(hit.transform.name);
        //}
    }
}
