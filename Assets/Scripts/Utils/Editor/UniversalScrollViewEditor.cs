using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

/// <summary>
/// UniversalScrollView的自定义编辑器
/// 提供友好的编辑器界面，支持实时预览和参数调节
/// </summary>
[CustomEditor(typeof(UniversalScrollView))]
public class UniversalScrollViewEditor : Editor
{
    private UniversalScrollView scrollView;
    private SerializedProperty scrollRectProp;
    private SerializedProperty contentProp;
    private SerializedProperty viewportProp;
    private SerializedProperty itemPrefabProp;
    private SerializedProperty itemSpacingProp;
    private SerializedProperty previewItemCountProp;
    private SerializedProperty scrollDirectionProp;
    private SerializedProperty containerWidthProp;
    private SerializedProperty containerHeightProp;

    // 预览相关
    private bool showPreview = false;
    private int previewCount = 5;

    private void OnEnable()
    {
        scrollView = (UniversalScrollView)target;

        // 获取序列化属性
        scrollRectProp = serializedObject.FindProperty("scrollRect");
        contentProp = serializedObject.FindProperty("content");
        viewportProp = serializedObject.FindProperty("viewport");
        itemPrefabProp = serializedObject.FindProperty("itemPrefab");
        itemSpacingProp = serializedObject.FindProperty("itemSpacing");
        previewItemCountProp = serializedObject.FindProperty("previewItemCount");
        scrollDirectionProp = serializedObject.FindProperty("scrollDirection");
        containerWidthProp = serializedObject.FindProperty("containerWidth");
        containerHeightProp = serializedObject.FindProperty("containerHeight");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 标题
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("通用滚动容器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 滚动设置
        DrawScrollSettings();
        EditorGUILayout.Space();

        // 项目设置
        DrawItemSettings();
        EditorGUILayout.Space();

        // 滚动方向
        DrawScrollDirection();
        EditorGUILayout.Space();

        // 容器尺寸
        DrawContainerSize();
        EditorGUILayout.Space();

        // 预览控制
        DrawPreviewControls();
        EditorGUILayout.Space();

        // 应用修改
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(scrollView);
        }
    }

    /// <summary>
    /// 绘制滚动设置
    /// </summary>
    private void DrawScrollSettings()
    {
        EditorGUILayout.LabelField("滚动设置", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(scrollRectProp, new GUIContent("Scroll Rect", "滚动矩形组件"));
        EditorGUILayout.PropertyField(contentProp, new GUIContent("Content", "内容容器"));
        EditorGUILayout.PropertyField(viewportProp, new GUIContent("Viewport", "视口容器"));

        // 自动查找组件按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("自动查找组件"))
        {
            AutoFindComponents();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制项目设置
    /// </summary>
    private void DrawItemSettings()
    {
        EditorGUILayout.LabelField("项目设置", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(itemPrefabProp, new GUIContent("项目预制体", "滚动项目的预制体"));
        EditorGUILayout.PropertyField(itemSpacingProp, new GUIContent("项目间距", "项目之间的间距"));
        EditorGUILayout.PropertyField(previewItemCountProp, new GUIContent("预览数量", "编辑器中预览的项目数量"));
    }

    /// <summary>
    /// 绘制滚动方向
    /// </summary>
    private void DrawScrollDirection()
    {
        EditorGUILayout.LabelField("滚动方向", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(scrollDirectionProp, new GUIContent("滚动方向", "选择垂直或水平滚动"));

        // 显示当前滚动方向的说明
        var direction = (UniversalScrollView.ScrollDirection)scrollDirectionProp.enumValueIndex;
        string directionText = direction == UniversalScrollView.ScrollDirection.Vertical ?
            "垂直滚动：项目从上到下排列" : "水平滚动：项目从左到右排列";

        EditorGUILayout.HelpBox(directionText, MessageType.Info);
    }

    /// <summary>
    /// 绘制容器尺寸
    /// </summary>
    private void DrawContainerSize()
    {
        EditorGUILayout.LabelField("容器尺寸", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(containerWidthProp, new GUIContent("宽度", "容器的宽度"));
        EditorGUILayout.PropertyField(containerHeightProp, new GUIContent("高度", "容器的高度"));
        EditorGUILayout.EndHorizontal();

        // 常用尺寸预设
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("手机竖屏 (375x667)"))
        {
            containerWidthProp.floatValue = 375f;
            containerHeightProp.floatValue = 667f;
        }
        if (GUILayout.Button("手机横屏 (667x375)"))
        {
            containerWidthProp.floatValue = 667f;
            containerHeightProp.floatValue = 375f;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("平板竖屏 (768x1024)"))
        {
            containerWidthProp.floatValue = 768f;
            containerHeightProp.floatValue = 1024f;
        }
        if (GUILayout.Button("平板横屏 (1024x768)"))
        {
            containerWidthProp.floatValue = 1024f;
            containerHeightProp.floatValue = 768f;
        }
        EditorGUILayout.EndHorizontal();

        // 应用尺寸按钮
        if (GUILayout.Button("应用尺寸到视口"))
        {
            ApplySizeToViewport();
        }
    }

    /// <summary>
    /// 绘制预览控制
    /// </summary>
    private void DrawPreviewControls()
    {
        EditorGUILayout.LabelField("预览控制", EditorStyles.boldLabel);

        showPreview = EditorGUILayout.Foldout(showPreview, "编辑器预览");

        if (showPreview)
        {
            EditorGUI.indentLevel++;

            previewCount = EditorGUILayout.IntSlider("预览项目数量", previewCount, 1, 20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成预览"))
            {
                GeneratePreview();
            }
            if (GUILayout.Button("清除预览"))
            {
                ClearPreview();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("预览功能只在编辑器中有效，运行时不会影响实际数据。", MessageType.Info);

            EditorGUI.indentLevel--;
        }
    }

    /// <summary>
    /// 自动查找组件
    /// </summary>
    private void AutoFindComponents()
    {
        // 查找ScrollRect
        if (scrollRectProp.objectReferenceValue == null)
        {
            var scrollRect = scrollView.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRectProp.objectReferenceValue = scrollRect;
            }
        }

        // 查找Content
        if (contentProp.objectReferenceValue == null && scrollRectProp.objectReferenceValue != null)
        {
            var scrollRect = scrollRectProp.objectReferenceValue as ScrollRect;
            if (scrollRect != null && scrollRect.content != null)
            {
                contentProp.objectReferenceValue = scrollRect.content;
            }
        }

        // 查找Viewport
        if (viewportProp.objectReferenceValue == null && scrollRectProp.objectReferenceValue != null)
        {
            var scrollRect = scrollRectProp.objectReferenceValue as ScrollRect;
            if (scrollRect != null && scrollRect.viewport != null)
            {
                viewportProp.objectReferenceValue = scrollRect.viewport;
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(scrollView);
    }

    /// <summary>
    /// 应用尺寸到视口
    /// </summary>
    private void ApplySizeToViewport()
    {
        if (viewportProp.objectReferenceValue != null)
        {
            var viewport = viewportProp.objectReferenceValue as RectTransform;
            if (viewport != null)
            {
                viewport.sizeDelta = new Vector2(containerWidthProp.floatValue, containerHeightProp.floatValue);
                EditorUtility.SetDirty(viewport);
            }
        }
    }

    /// <summary>
    /// 生成预览
    /// </summary>
    private void GeneratePreview()
    {
        if (itemPrefabProp.objectReferenceValue == null)
        {
            EditorUtility.DisplayDialog("错误", "请先设置项目预制体！", "确定");
            return;
        }

        // 创建预览数据
        var previewData = new System.Collections.Generic.List<ISetComScrollItem>();
        for (int i = 0; i < previewCount; i++)
        {
            previewData.Add(new ISetComScrollItem { index = i });
        }

        // 调用预览方法
        scrollView.SetData(previewData);

        EditorUtility.DisplayDialog("成功", $"已生成 {previewCount} 个预览项目", "确定");
    }

    /// <summary>
    /// 清除预览
    /// </summary>
    private void ClearPreview()
    {
        scrollView.ClearData();
        EditorUtility.DisplayDialog("成功", "已清除预览项目", "确定");
    }

    /// <summary>
    /// 场景视图中的GUI绘制
    /// </summary>
    private void OnSceneGUI()
    {
        if (scrollView == null) return;

        // 在场景视图中显示容器尺寸信息
        Handles.BeginGUI();

        var screenPos = Camera.current.WorldToScreenPoint(scrollView.transform.position);
        if (screenPos.z > 0)
        {
            var rect = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 50, 200, 50);
            GUI.Box(rect, $"容器尺寸: {containerWidthProp.floatValue:F0} x {containerHeightProp.floatValue:F0}");
        }

        Handles.EndGUI();
    }
}


