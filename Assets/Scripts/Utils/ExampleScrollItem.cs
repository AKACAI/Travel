using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 示例滚动项目类
/// 展示如何继承ComScrollItem来实现自定义的滚动项目
/// </summary>
public class ExampleScrollItem : ComScrollItem
{
    [Header("UI组件")]
    [SerializeField] private Text indexText;
    [SerializeField] private Text contentText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button clickButton;

    [Header("样式设置")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color hoverColor = Color.cyan;

    private bool isSelected = false;

    public override void Init()
    {
        base.Init();

        // 初始化UI组件
        InitializeUI();

        // 绑定按钮事件
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnItemClicked);
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        // 清理按钮事件
        if (clickButton != null)
        {
            clickButton.onClick.RemoveListener(OnItemClicked);
        }
    }

    public override void SetData(ISetComScrollItem data)
    {
        base.SetData(data);

        // 更新UI显示
        UpdateUI();
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeUI()
    {
        // 如果没有找到UI组件，尝试自动查找
        if (indexText == null)
        {
            indexText = transform.Find("IndexText")?.GetComponent<Text>();
        }

        if (contentText == null)
        {
            contentText = transform.Find("ContentText")?.GetComponent<Text>();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (clickButton == null)
        {
            clickButton = GetComponent<Button>();
        }
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (indexText != null)
        {
            indexText.text = $"项目 {_data.index}";
        }

        if (contentText != null)
        {
            contentText.text = $"这是第 {_data.index + 1} 个滚动项目的内容";
        }

        UpdateBackgroundColor();
    }

    /// <summary>
    /// 更新背景颜色
    /// </summary>
    private void UpdateBackgroundColor()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// 项目被点击时的处理
    /// </summary>
    private void OnItemClicked()
    {
        Debug.Log($"点击了项目 {_data.index}");

        // 切换选中状态
        SetSelected(!isSelected);

        // 可以在这里添加更多点击逻辑
        // 比如发送事件、播放音效等
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    /// <param name="selected">是否选中</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }

    /// <summary>
    /// 获取选中状态
    /// </summary>
    /// <returns>是否选中</returns>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// 设置悬停状态（用于鼠标悬停效果）
    /// </summary>
    /// <param name="hover">是否悬停</param>
    public void SetHover(bool hover)
    {
        if (backgroundImage != null && !isSelected)
        {
            backgroundImage.color = hover ? hoverColor : normalColor;
        }
    }

    #region 鼠标事件处理（可选）

    private void OnMouseEnter()
    {
        SetHover(true);
    }

    private void OnMouseExit()
    {
        SetHover(false);
    }

    #endregion
}


