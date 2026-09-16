# 视觉精品化升级 —— 实现计划

## Task 1: 激活 ParasiteBackground Shader 作为全局背景

**目标**: 将 6 张 floor_bg 静态图片升级为 `ParasiteBackground` Shader 动态背景

**依赖**: 无（基础层）
**优先级**: high
**对应 AC**: AC3

**实现**:
1. 创建新脚本 `Assets/Scripts/Runtime/Effects/PremiumBackground.cs` — 负责为战斗/探索 Canvas 的背景 Image 应用 ParasiteBackground Shader Material
2. 在 `ShaderMaterialManager` 中暴露 `_bgMat` 或新增 `GetPremiumBackgroundMat(int floorId)` 方法
3. 按楼层切换 Shader 参数（_MainColor / _SecondaryColor / _TertiaryColor / _NoiseScale / _Speed）:
   - F1-F2: 偏绿 (HealthGreen + DarkGreen 变体)
   - F3-F4: 偏紫 (AbyssPurple + DarkPurple 变体)
   - F5-F6: 偏红 (CriticalRed + DarkRed 变体)
4. 战斗界面：Canvas 最底层 Image 挂 ParasiteBackground Material，`_MainTex = Texture2D.whiteTexture`
5. 探索界面：保留 floor_bg 图片作为底图（不丢弃），上面叠一层半透明 ParasiteBackground Image 做动态噪音/粒子覆盖
6. 玩家进入新楼层时通过事件回调触发 Shader 参数切换

**文件清单**:
- 新建 `Assets/Scripts/Runtime/Effects/PremiumBackground.cs`
- 修改 `Assets/Scripts/Runtime/Effects/ShaderMaterialManager.cs` (新增方法)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Combat.cs` (战斗 Canvas 初始化)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Explore.cs` (探索 Canvas 初始化)

**测试要求**:
- `rule`: 战斗界面 Canvas 底层 Image Material 应为 `Game/UI/ParasiteBackground`，FBM 噪音 + 粒子可见
- `rule`: 探索界面有 2 层背景 — floor_bg 静态底图 + ParasiteBackground 动态叠加层
- `rule`: 切换楼层后 Shader 颜色参数随之变化（从 F→Green, F3→Purple, F5→Red）

---

## Task 2: 扩展 BioPunkPostFX — 扫描线 + 污染红光 + Bloom 叠加

**目标**: 现有暗角 + 颗粒之上，增加扫描线 + 污染红光脉动 + UI 层 Bloom

**依赖**: Task 1（全局氛围基础）
**优先级**: high
**对应 AC**: AC3

**实现**:
1. 在 `BioPunkPostFX.cs` 新增 3 层 Image 叠加：
   - **Scanlines**: 半透明水平条纹（运行时程序化生成 Texture2D），污染>0.5 时淡入 + 速度加快
   - **PollutionRedPulse**: 全屏红色 Image，污染>0.3 时出现 + 透明度随 Time.time 脉动，频率随污染度加快
   - **BloomOverlay**: 全屏 Image，极亮区域高亮（用简单 Shader 或程序化 Texture），污染>0.7 时增强
2. 所有叠加层使用 `raycastTarget=false`，保证不干扰 UI 交互
3. `SetPollutionIntensity(float)` 驱动所有层的透明度 + 速度 + 颜色强度

**文件清单**:
- 修改 `Assets/Scripts/Runtime/Effects/BioPunkPostFX.cs`

**测试要求**:
- `rule`: 污染度=0 时无扫描线、无红光、只有暗角
- `rule`: 污染度=0.5 时扫描线淡入、暗红脉动可见
- `rule`: 污染度=1.0 时扫描线快速、强红脉动、暗角+颗粒全部增强

---

## Task 3: 探索地图格子描边 + 玩家光标光效 + 出口脉冲

**目标**: 地图从"色块 + Outline 呼吸"升级为"格子描边 + 玩家光标光晕 + 出口金色脉冲圈"

**依赖**: 无（独立层，可并行）
**优先级**: high
**对应 AC**: AC1

**实现**:
1. **格子描边**: 在每个 cell 上添加 4 条极细 Image（Left/Right/Top/Bottom）作为描边 Sprite，使用 `CreateCircleSprite`/WhiteSprite 程序化生成，颜色统一为 `BioBorderDim (0,1,0.8,0.08)`，相邻同类型格子合并描边（跳过共享边）
2. **玩家光标光晕**: 玩家格子 icon 之上叠一层放大 1.5× 的 CircleSprite，颜色为 Cyan + alpha 呼吸脉动，效果类似"光标在格子里发光"
3. **出口脉冲圈**: 出口格子 icon 之上叠 2 层 CircleSprite，分别以不同速度 + 不同相位扩散 + 淡出，形成金色脉冲环效果
4. **怪物 icon 背景发光**: 在怪物 icon 之下添加 CircleSprite（半径约格子 0.9×），颜色与怪物 Outline 同色系，做轻微脉冲

**文件清单**:
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Explore.cs` (格子初始化)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.cs` (渲染循环中新增玩家光标/出口脉冲逻辑)

**测试要求**:
- `rule`: 所有已发现格子有细微描边（相邻墙与地板描边分别渲染，相邻同类格子共享边不重复）
- `rule`: 玩家格子有明显 cyan 呼吸光晕圈（覆盖 icon）
- `rule`: 出口格子有 2 层金色脉冲环以不同速度向外扩散

---

## Task 4: 战斗界面 HP Bar 光流动画 + Possess 进度环脉冲

**目标**: HP Bar 从静态渐变升级为动态光流动画；Possess 进度环从静态条升级为脉冲环

**依赖**: 无（独立层，可并行）
**优先级**: high
**对应 AC**: AC2

**实现**:
1. **HP Bar 光流**: 在 `GradientHealthBar.shader` 上新增一个 `_FlowOffset` 属性，在 `ShaderMaterialManager` 中对 HP Bar Material 每帧累加 `_FlowOffset += _Speed * Time.deltaTime`，让高光从左到右流动
2. **HP Bar 边缘发光**: 在 HP Fill Image 之上叠一个小一圈的 Outline（代码创建 Outline 组件或第二个 Image），使用对应色系（玩家 Cyan，敌人 Red）做 1.5px 发光边
3. **Possess 进度环**: 在现有 PossessFill 之上叠一层 CircleSprite 进度环（代码生成扇形遮罩），possess 进度驱动扇形角度，同时做 alpha + 宽度脉冲
4. **EP Bar 同样光流**: 与 HP Bar 相同光流 + 边缘发光逻辑，颜色改为 Cyan/Purple

**文件清单**:
- 修改 `Assets/Shaders/GradientHealthBar.shader` (新增 _FlowOffset + 光流逻辑)
- 修改 `Assets/Scripts/Runtime/Effects/ShaderMaterialManager.cs` (光流驱动 + Possess 进度环 Material)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Combat.cs` (HP/EP bar 创建 + Possess 环)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.cs` (SyncCombat 中的 HP/EP 更新)

**测试要求**:
- `rule`: 玩家 HP Bar 有从左向右的青色高光流动动画（持续可见）
- `rule`: 敌人 HP Bar 有从左向右的红色高光流动动画
- `rule`: Possess 进度条有脉冲光环效果，possess 进行中时脉冲频率加快

---

## Task 5: 战斗界面角色卡片发光 + 攻击命中反馈

**目标**: 玩家/怪物卡片从静态升级为发光呼吸 + 攻击命中时闪光 + 伤害数字飞出

**依赖**: 无（独立层，可并行）
**优先级**: medium
**对应 AC**: AC2

**实现**:
1. **角色卡片呼吸发光**: `_cPCardOutline` / `_cECardOutline` 改为持续呼吸动画——每帧更新 effectColor alpha (0.3~0.8 脉动) + 边框厚度微调，玩家用 Cyan，敌人用 Red
2. **攻击命中闪光**: 攻击事件发生时，被击中角色卡片叠一层白色 Image (alpha 0.6)，0.15s 内淡出；同时卡片做一次 scale 抖动 (shake)
3. **伤害数字飞出**: 攻击事件发生时，程序化创建一个 Text（大号 + 白 + 黑色 Outline），放在被击中角色卡片上方，做 Y 轴向上位移 + alpha 淡出 + 放大动画，0.6s 销毁
4. **按钮按下反馈**: 攻击/防御/Possess 按钮按下时，按钮 Outline 颜色瞬间变亮 + Image scale 缩小 0.93 + 白色闪光 overlay

**文件清单**:
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Combat.cs` (卡片创建 + 攻击反馈 + 伤害数字)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.cs` (SyncCombat 或新增 AttackFeedback 方法)
- 新建 `Assets/Scripts/Runtime/Effects/CombatFeedback.cs` (攻击命中/伤害数字/按钮反馈管理器)

**测试要求**:
- `rule`: 战斗进行中玩家卡片 Outline 持续呼吸（alpha 脉动可见）
- `rule`: 攻击敌人后，敌人卡片有一次白色闪光 + 轻微抖动
- `rule`: 伤害数字从敌人卡片位置向上飞出，字体醒目（白字+黑边，字号>28）

---

## Task 6: UI 过渡动画 + 污染度视觉强化

**目标**: 按钮入场、面板展开、污染度变化有动画反馈

**依赖**: Task 1, Task 2
**优先级**: low
**对应 AC**: AC2, AC3

**实现**:
1. **面板入场动画**: 探索/战斗 Canvas 启动时，所有主要面板从 scale 0.92 + alpha 0 插值到 scale 1 + alpha 1，使用 LeanTween 或协程
2. **按钮 hover/press**: 鼠标 hover 时按钮 Outline color 变亮 + scale 1.02；press 时 scale 0.95 + Outline 变黑
3. **污染度变化动画**: 污染度上升时，BioPunkPostFX 红光强度插值 + 一个全屏红色闪一下（0.1s alpha 0.4 红色 overlay）
4. **退出战斗过渡**: 从战斗返回探索时，画面用 ParasiteDissolve 的"故障抖动"效果做 0.3s 过渡

**文件清单**:
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Combat.cs` (入场动画)
- 修改 `Assets/Scripts/Runtime/UI/CanvasUIManager.Explore.cs` (入场动画)
- 修改 `Assets/Scripts/Runtime/Effects/BioPunkPostFX.cs` (污染变化 flash)
- 修改 `Assets/Scripts/Runtime/Effects/ParasiteShaderManager.cs` (UI breakdown 过渡)

**测试要求**:
- `rule`: 进入战斗界面时所有面板有 0.3s 内入场动画（可见）
- `rule`: 污染度上升 25+ 时有红色 flash 反馈
- `rule`: 战斗→探索过渡有故障/抖动效果

---

## Task 7: 性能优化 & 清理

**目标**: 确保所有新增效果不影响性能

**依赖**: Task 1-6
**优先级**: low
**对应 AC**: AC4, AC5

**实现**:
1. 所有程序化创建的 Texture2D/Sprite/Material 加入对象池 + 生命周期管理（OnDestroy 中 Destroy）
2. 呼吸/脉冲动画用 `Time.time` 静态变量驱动，避免每帧遍历
3. Particle/Overlay 在不需要时 SetActive(false)
4. 检查所有新建脚本的 using 引用、Assembly-CSharp 兼容性
5. 移除任何未使用的 Shader/Material/Texture 引用

**测试要求**:
- `rule`: Canvas.Refresh() 或 Update 中无 GC Alloc（Profiler 验证）
- `rule`: 切换场景后无 Texture/Material 泄漏（数量不持续增长）
- `rule`: 编译零 error 零 warning
