using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public struct ISetComScrollItem
{
    public int index;
}

public class ComScrollItem : MonoBehaviour
{
    protected ISetComScrollItem _data;

    public virtual void Init()
    {
        // 子类重写此方法进行初始化
    }

    public virtual void Dispose()
    {
        // 子类重写此方法进行清理
    }

    public virtual void SetData(ISetComScrollItem data)
    {
        _data = data;
    }
}
