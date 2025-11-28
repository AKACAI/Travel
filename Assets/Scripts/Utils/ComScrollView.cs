using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class ComScrollView
{
    private GameObject _itemPrefab;
    private List<ComScrollItem> _items = new List<ComScrollItem>();
    private float _curProgress;
    private float _itemHeight;
    private int _itemCount;

    public ComScrollView(GameObject itemPrefab, float itemHeight)
    {
        _itemPrefab = itemPrefab;
        _itemHeight = itemHeight;
    }

    public void SetData(List<ISetComScrollItem> data)
    {
        // 清空现有项目
        ClearItems();

        // 为每个数据项创建对应的ComScrollItem
        for (int i = 0; i < data.Count; i++)
        {
            GameObject itemObj = GameObject.Instantiate(_itemPrefab);
            ComScrollItem item = itemObj.GetComponent<ComScrollItem>();
            if (item != null)
            {
                item.SetData(data[i]);
                item.Init();
                _items.Add(item);
            }
        }

        _itemCount = data.Count;
    }

    private void ClearItems()
    {
        foreach (var item in _items)
        {
            if (item != null)
            {
                item.Dispose();
                GameObject.Destroy(item.gameObject);
            }
        }
        _items.Clear();
    }
}
