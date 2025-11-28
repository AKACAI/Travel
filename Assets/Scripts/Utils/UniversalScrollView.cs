using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用滚动容器类
/// 支持横竖两种滚动方向，可在编辑器中调节容器宽高
/// </summary>
public class UniversalScrollView : MonoBehaviour
{
    [Header("滚动设置")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;

    [Header("项目设置")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private int previewItemCount = 5; // 编辑器中预览的项目数量

    [Header("滚动方向")]
    [SerializeField] private ScrollDirection scrollDirection = ScrollDirection.Vertical;

    [Header("容器尺寸")]
    [SerializeField] private float containerWidth = 300f;
    [SerializeField] private float containerHeight = 400f;

    // 私有变量
    private List<ComScrollItem> _items = new List<ComScrollItem>();
    private List<ISetComScrollItem> _dataList = new List<ISetComScrollItem>();
    private bool _isInitialized = false;

    public enum ScrollDirection
    {
        Vertical,   // 垂直滚动
        Horizontal  // 水平滚动
    }

    #region Unity生命周期

    private void Awake()
    {
        InitializeScrollView();
    }

    private void Start()
    {
        if (_isInitialized)
        {
            RefreshLayout();
        }
    }

    #endregion

    #region 初始化方法

    /// <summary>
    /// 初始化滚动视图
    /// </summary>
    private void InitializeScrollView()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogError("UniversalScrollView: 未找到ScrollRect组件！");
                return;
            }
        }

        if (content == null)
        {
            content = scrollRect.content;
        }

        if (viewport == null)
        {
            viewport = scrollRect.viewport;
        }

        SetupScrollDirection();
        SetupContainerSize();
        _isInitialized = true;
    }

    /// <summary>
    /// 设置滚动方向
    /// </summary>
    private void SetupScrollDirection()
    {
        if (scrollDirection == ScrollDirection.Vertical)
        {
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            // 设置Content的布局
            var layoutGroup = content.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.spacing = itemSpacing;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

            // 移除水平布局组件
            var horizontalLayout = content.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                DestroyImmediate(horizontalLayout);
            }
        }
        else
        {
            scrollRect.vertical = false;
            scrollRect.horizontal = true;

            // 设置Content的布局
            var layoutGroup = content.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            layoutGroup.spacing = itemSpacing;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = true;

            // 移除垂直布局组件
            var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                DestroyImmediate(verticalLayout);
            }
        }
    }

    /// <summary>
    /// 设置容器尺寸
    /// </summary>
    private void SetupContainerSize()
    {
        if (viewport != null)
        {
            viewport.sizeDelta = new Vector2(containerWidth, containerHeight);
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 设置数据列表
    /// </summary>
    /// <param name="dataList">数据列表</param>
    public void SetData(List<ISetComScrollItem> dataList)
    {
        if (!_isInitialized)
        {
            InitializeScrollView();
        }

        _dataList = dataList ?? new List<ISetComScrollItem>();
        RefreshItems();
    }

    /// <summary>
    /// 添加单个数据项
    /// </summary>
    /// <param name="data">数据项</param>
    public void AddItem(ISetComScrollItem data)
    {
        if (_dataList == null)
        {
            _dataList = new List<ISetComScrollItem>();
        }

        _dataList.Add(data);
        RefreshItems();
    }

    /// <summary>
    /// 移除指定索引的数据项
    /// </summary>
    /// <param name="index">索引</param>
    public void RemoveItem(int index)
    {
        if (_dataList != null && index >= 0 && index < _dataList.Count)
        {
            _dataList.RemoveAt(index);
            RefreshItems();
        }
    }

    /// <summary>
    /// 清空所有数据
    /// </summary>
    public void ClearData()
    {
        _dataList?.Clear();
        RefreshItems();
    }

    /// <summary>
    /// 设置滚动方向
    /// </summary>
    /// <param name="direction">滚动方向</param>
    public void SetScrollDirection(ScrollDirection direction)
    {
        if (scrollDirection != direction)
        {
            scrollDirection = direction;
            SetupScrollDirection();
            RefreshLayout();
        }
    }

    /// <summary>
    /// 获取滚动方向
    /// </summary>
    /// <returns>滚动方向</returns>
    public ScrollDirection GetScrollDirection()
    {
        return scrollDirection;
    }

    /// <summary>
    /// 设置容器尺寸
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public void SetContainerSize(float width, float height)
    {
        containerWidth = width;
        containerHeight = height;
        SetupContainerSize();
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 刷新项目列表
    /// </summary>
    private void RefreshItems()
    {
        ClearItems();

        if (_dataList == null || itemPrefab == null)
        {
            return;
        }

        // 创建项目
        for (int i = 0; i < _dataList.Count; i++)
        {
            CreateItem(_dataList[i], i);
        }

        RefreshLayout();
    }

    /// <summary>
    /// 创建单个项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="index">索引</param>
    private void CreateItem(ISetComScrollItem data, int index)
    {
        GameObject itemObj = Instantiate(itemPrefab, content);
        ComScrollItem item = itemObj.GetComponent<ComScrollItem>();

        if (item != null)
        {
            item.SetData(data);
            item.Init();
            _items.Add(item);
        }
        else
        {
            Debug.LogWarning($"UniversalScrollView: 预制体 {itemPrefab.name} 没有ComScrollItem组件！");
        }
    }

    /// <summary>
    /// 清空所有项目
    /// </summary>
    private void ClearItems()
    {
        foreach (var item in _items)
        {
            if (item != null)
            {
                item.Dispose();
                DestroyImmediate(item.gameObject);
            }
        }
        _items.Clear();
    }

    /// <summary>
    /// 刷新布局
    /// </summary>
    private void RefreshLayout()
    {
        if (content != null)
        {
            // 强制重新计算布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    #endregion

    #region 编辑器预览方法

    /// <summary>
    /// 在编辑器中预览效果
    /// </summary>
    [ContextMenu("预览滚动视图")]
    public void PreviewInEditor()
    {
        if (!Application.isPlaying)
        {
            // 创建预览数据
            var previewData = new List<ISetComScrollItem>();
            for (int i = 0; i < previewItemCount; i++)
            {
                previewData.Add(new ISetComScrollItem { index = i });
            }

            SetData(previewData);
        }
    }

    #endregion
}


