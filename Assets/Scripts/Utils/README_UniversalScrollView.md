# 通用滚动容器使用说明

## 概述

`UniversalScrollView` 是一个功能强大的通用滚动容器类，支持横竖两种滚动方向，可以在编辑器中调节容器宽高，并支持预制体预览效果。

## 主要特性

- ✅ 支持垂直和水平两种滚动方向
- ✅ 可在编辑器中实时调节容器宽高
- ✅ 支持预制体预览功能
- ✅ 自动布局管理
- ✅ 友好的编辑器界面
- ✅ 完整的API支持

## 文件结构

```
Assets/Scripts/Utils/
├── UniversalScrollView.cs          # 主要的滚动容器类
├── UniversalScrollViewEditor.cs    # 自定义编辑器脚本
├── ExampleScrollItem.cs            # 示例滚动项目类
├── ScrollViewExample.cs            # 使用示例脚本
├── ComScrollView.cs                # 原有的滚动视图类
├── ComScrollItem.cs                # 滚动项目基类
└── README_UniversalScrollView.md   # 使用说明文档
```

## 快速开始

### 1. 设置滚动容器

1. 在场景中创建一个GameObject
2. 添加 `UniversalScrollView` 组件
3. 设置必要的引用：
   - **Scroll Rect**: ScrollRect组件引用
   - **Content**: 内容容器引用
   - **Viewport**: 视口容器引用
   - **Item Prefab**: 滚动项目的预制体

### 2. 创建滚动项目预制体

1. 创建一个UI预制体作为滚动项目
2. 添加 `ComScrollItem` 或 `ExampleScrollItem` 组件
3. 设置UI布局（文本、图片、按钮等）

### 3. 在代码中使用

```csharp
// 获取滚动视图组件
UniversalScrollView scrollView = GetComponent<UniversalScrollView>();

// 创建数据列表
List<ISetComScrollItem> dataList = new List<ISetComScrollItem>();
for (int i = 0; i < 10; i++)
{
    dataList.Add(new ISetComScrollItem { index = i });
}

// 设置数据
scrollView.SetData(dataList);
```

## 详细使用说明

### 编辑器设置

#### 滚动设置
- **Scroll Rect**: 拖拽ScrollRect组件到此处
- **Content**: 拖拽内容容器到此处
- **Viewport**: 拖拽视口容器到此处
- **自动查找组件**: 点击按钮自动查找相关组件

#### 项目设置
- **项目预制体**: 设置滚动项目的预制体
- **项目间距**: 设置项目之间的间距
- **预览数量**: 设置编辑器中预览的项目数量

#### 滚动方向
- **垂直滚动**: 项目从上到下排列
- **水平滚动**: 项目从左到右排列

#### 容器尺寸
- **宽度/高度**: 手动设置容器尺寸
- **预设按钮**: 快速设置常用尺寸
  - 手机竖屏 (375x667)
  - 手机横屏 (667x375)
  - 平板竖屏 (768x1024)
  - 平板横屏 (1024x768)

#### 预览控制
- **生成预览**: 在编辑器中生成预览项目
- **清除预览**: 清除预览项目

### API 参考

#### 主要方法

```csharp
// 设置数据列表
public void SetData(List<ISetComScrollItem> dataList)

// 添加单个数据项
public void AddItem(ISetComScrollItem data)

// 移除指定索引的数据项
public void RemoveItem(int index)

// 清空所有数据
public void ClearData()

// 设置滚动方向
public void SetScrollDirection(ScrollDirection direction)

// 设置容器尺寸
public void SetContainerSize(float width, float height)
```

#### 枚举类型

```csharp
public enum ScrollDirection
{
    Vertical,   // 垂直滚动
    Horizontal  // 水平滚动
}
```

### 自定义滚动项目

继承 `ComScrollItem` 类来创建自定义的滚动项目：

```csharp
public class MyScrollItem : ComScrollItem
{
    [SerializeField] private Text titleText;
    [SerializeField] private Image iconImage;
    
    public override void Init()
    {
        base.Init();
        // 初始化逻辑
    }
    
    public override void Dispose()
    {
        base.Dispose();
        // 清理逻辑
    }
    
    public override void SetData(ISetComScrollItem data)
    {
        base.SetData(data);
        // 更新UI显示
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // 根据数据更新UI
        titleText.text = $"项目 {_data.index}";
    }
}
```

## 使用示例

### 基本使用

```csharp
public class MyScrollController : MonoBehaviour
{
    [SerializeField] private UniversalScrollView scrollView;
    
    private void Start()
    {
        // 创建测试数据
        var dataList = new List<ISetComScrollItem>();
        for (int i = 0; i < 20; i++)
        {
            dataList.Add(new ISetComScrollItem { index = i });
        }
        
        // 设置数据
        scrollView.SetData(dataList);
    }
    
    public void AddNewItem()
    {
        var newItem = new ISetComScrollItem { index = scrollView.GetItemCount() };
        scrollView.AddItem(newItem);
    }
}
```

### 动态切换滚动方向

```csharp
public void SwitchToHorizontal()
{
    scrollView.SetScrollDirection(UniversalScrollView.ScrollDirection.Horizontal);
}

public void SwitchToVertical()
{
    scrollView.SetScrollDirection(UniversalScrollView.ScrollDirection.Vertical);
}
```

### 响应式尺寸调整

```csharp
public void AdjustForScreenSize()
{
    float screenWidth = Screen.width;
    float screenHeight = Screen.height;
    
    if (screenWidth > screenHeight)
    {
        // 横屏模式
        scrollView.SetContainerSize(screenWidth * 0.8f, screenHeight * 0.6f);
    }
    else
    {
        // 竖屏模式
        scrollView.SetContainerSize(screenWidth * 0.9f, screenHeight * 0.7f);
    }
}
```

## 注意事项

1. **预制体要求**: 滚动项目预制体必须包含 `ComScrollItem` 组件
2. **布局组件**: 系统会自动添加和配置 `VerticalLayoutGroup` 或 `HorizontalLayoutGroup`
3. **性能优化**: 对于大量数据，建议使用对象池技术
4. **编辑器预览**: 预览功能只在编辑器中有效，不会影响运行时数据

## 扩展功能

### 对象池支持

可以扩展 `UniversalScrollView` 来支持对象池，提高性能：

```csharp
public class PooledScrollView : UniversalScrollView
{
    private Queue<ComScrollItem> itemPool = new Queue<ComScrollItem>();
    
    protected override ComScrollItem CreateItem(ISetComScrollItem data, int index)
    {
        ComScrollItem item = GetPooledItem();
        if (item == null)
        {
            item = base.CreateItem(data, index);
        }
        else
        {
            item.SetData(data);
            item.Init();
        }
        return item;
    }
    
    private ComScrollItem GetPooledItem()
    {
        return itemPool.Count > 0 ? itemPool.Dequeue() : null;
    }
}
```

### 虚拟化滚动

对于超大数据集，可以实现虚拟化滚动来只渲染可见项目。

## 故障排除

### 常见问题

1. **项目不显示**: 检查预制体是否正确设置了 `ComScrollItem` 组件
2. **布局错乱**: 确保ScrollRect、Content、Viewport的层级关系正确
3. **滚动不工作**: 检查ScrollRect的配置和Content的尺寸设置

### 调试技巧

1. 使用编辑器预览功能测试布局
2. 检查Console中的错误信息
3. 使用Scene视图查看UI层级结构

## 更新日志

- **v1.0.0**: 初始版本，支持基本的横竖滚动功能
- 支持编辑器预览和尺寸调节
- 提供完整的API和示例代码


