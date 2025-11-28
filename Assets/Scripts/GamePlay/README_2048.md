# 2048 游戏

这是一个完整的2048游戏实现，包含游戏逻辑、UI界面和触摸控制。

## 功能特性

- 完整的2048游戏逻辑
- 支持键盘控制（WASD或方向键）
- 支持触摸/鼠标滑动控制
- 撤销功能（最多10步）
- 分数记录和最高分保存
- 游戏胜利和失败检测
- 平滑的动画效果
- 响应式UI设计

## 文件结构

### 核心脚本
- `Game2048.cs` - 游戏核心逻辑，包含移动、合并、分数计算等
- `Tile2048.cs` - 数字方块组件，处理显示和动画
- `Game2048Manager.cs` - 游戏管理器，处理输入和UI状态

## 设置步骤

### 1. 创建UI结构

在Unity中创建以下UI层级结构：

```
Canvas
├── Background Panel
│   ├── Score Panel
│   │   ├── Score Text
│   │   └── Best Score Text
│   ├── Grid Container (用于放置数字方块)
│   └── Button Panel
│       ├── Restart Button
│       └── Undo Button
├── Game Over Panel (初始隐藏)
│   ├── Game Over Text
│   ├── Final Score Text
│   └── Restart Button
└── Game Won Panel (初始隐藏)
    ├── Congratulations Text
    ├── Final Score Text
    └── Continue Button
```

### 2. 创建数字方块预制体

1. 创建一个UI Image作为数字方块
2. 添加一个Text子对象用于显示数字
3. 添加`Tile2048`脚本到Image对象
4. 在脚本中设置`numberText`和`backgroundImage`引用
5. 将对象制作成预制体

### 3. 设置游戏对象

1. 创建一个空GameObject，命名为"Game2048"
2. 添加`Game2048`脚本
3. 设置脚本中的引用：
   - Grid Container: 放置数字方块的容器
   - Tile Prefab: 数字方块预制体
   - Score Text: 分数显示文本
   - Best Score Text: 最高分显示文本
   - Restart Button: 重新开始按钮
   - Undo Button: 撤销按钮

4. 创建另一个空GameObject，命名为"Game2048Manager"
5. 添加`Game2048Manager`脚本
6. 设置脚本中的引用：
   - Game2048: 游戏逻辑脚本
   - Game Canvas: 主画布
   - Game Over Panel: 游戏结束面板
   - Game Won Panel: 游戏胜利面板
   - Final Score Text: 最终分数文本

### 4. 设置输入区域

1. 在Grid Container上添加`GraphicRaycaster`组件
2. 添加`Game2048Manager`脚本到Grid Container（用于处理触摸输入）
3. 确保Grid Container有Image组件作为射线检测目标

## 游戏控制

### 键盘控制
- W 或 ↑: 向上移动
- S 或 ↓: 向下移动
- A 或 ←: 向左移动
- D 或 →: 向右移动

### 触摸/鼠标控制
- 在游戏区域滑动即可移动方块
- 支持四个方向的滑动

## 游戏规则

1. 游戏开始时，棋盘上会随机出现两个数字（2或4）
2. 每次移动后，会在空位置随机生成一个新的数字（2或4）
3. 相同数字的方块相撞时会合并成它们的和
4. 当无法再移动时游戏结束
5. 达到2048时获得胜利，但可以继续游戏

## 自定义设置

### 修改游戏参数
在`Game2048`脚本中可以修改：
- `gridSize`: 网格大小（默认4x4）
- `targetScore`: 目标分数（默认2048）

### 修改颜色主题
在`Tile2048`脚本中可以修改`tileColors`数组来自定义不同数字的颜色。

### 修改动画效果
在`Tile2048`脚本中可以调整各种动画的持续时间。

## 注意事项

1. 确保所有UI组件都正确设置了引用
2. 数字方块预制体必须包含`Tile2048`脚本
3. 触摸输入需要正确设置GraphicRaycaster
4. 游戏会自动保存最高分到PlayerPrefs中

## 扩展功能

可以考虑添加的功能：
- 音效系统
- 粒子效果
- 更多主题
- 游戏统计
- 成就系统
- 在线排行榜 