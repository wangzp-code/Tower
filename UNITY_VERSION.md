# Unity版本配置

## 项目Unity版本

**Unity 2022.3.62f2c1**

## 项目配置

### 已创建的配置文件

| 文件 | 路径 | 描述 |
|------|------|------|
| **ProjectVersion.txt** | ProjectSettings/ProjectVersion.txt | Unity版本配置 |
| **ProjectSettings.asset** | ProjectSettings/ProjectSettings.asset | 项目主要设置 |
| **TagManager.asset** | ProjectSettings/TagManager.asset | 标签和层设置 |
| **QualitySettings.asset** | ProjectSettings/QualitySettings.asset | 质量设置 |
| **InputManager.asset** | ProjectSettings/InputManager.asset | 输入管理器配置 |

## 在Unity中打开项目

### 方法1: 通过Unity Hub
1. 打开Unity Hub
2. 点击 "Projects" 选项卡
3. 点击 "Add" 按钮
4. 选择 `/Users/wangzhipeng/tower` 目录
5. Unity会自动检测到版本为2022.3.62f2c1
6. 点击项目启动

### 方法2: 命令行
```bash
open -a Unity /Users/wangzhipeng/tower
```

## 项目设置

### 主要配置
- **产品名称**: Parasite Tower
- **公司名称**: YourCompany (可修改)
- **应用包名**: com.parasitetower.game (可修改)
- **版本号**: 1.0.0
- **默认质量**: Ultra
- **目标帧率**: 60 FPS
- **颜色空间**: Gamma

### 输入映射
- Horizontal: A/D 或 ←/→
- Vertical: W/S 或 ↑/↓
- Fire1: 左键或左Ctrl
- Fire2: 右键或左Alt
- Fire3: 中键或左Shift
- Jump: 空格
- Submit: 回车
- Cancel: Esc

## 注意事项

1. **确保使用正确的Unity版本**: 必须是2022.3.62f2c1
2. **打开项目时**: 第一次打开可能需要重新导入资源
3. **场景配置**: 参考 [SceneSetupGuide.md](SceneSetupGuide.md) 进行详细配置

## Build Settings

首次打开项目后，需要配置Build Settings:

1. 打开 Unity 编辑器
2. 菜单: File → Build Settings
3. 添加场景:
   - Scenes/BootstrapScene.unity
   - Scenes/MainMenu.unity
   - Scenes/Game.unity
4. 选择目标平台 (Standalone OSX)
5. 配置Player Settings

---

**项目已配置完成，可以在Unity 2022.3.62f2c1中打开！**
