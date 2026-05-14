# EXLagavulin

一个为《杀戮尖塔2》制作的自定义 Boss Mod。

本 Mod 添加了一个基于 杀戮尖塔2的乐嘉维林族母和杀戮尖塔1的乐嘉维林 的全新 Boss 遭遇， 使用原版贴图 （动画会在未来的release里添加）。

---

# 功能特点

- 第二层 Boss
- 使用原版敌人与战斗机制（生成的乐嘉维林使用 单次攻击 -> -1力-1敏 -> 多段攻击 的回合循环）
- 原版贴图与场景
- 基于 Godot + C# 开发

---

# 项目结构

```text
EXLagavulin/
├── images/            # 图片与贴图资源
├── localization/      # 本地化文本
├── scenes/            # Godot 场景文件
├── src/               # C# 源代码
├── EXLagavulin.json   # Mod 元数据
├── project.godot      # Godot 工程文件
└── EXLagavulin.csproj # C# 项目文件
```

---

# 已知的问题

- 使用控制台强制第二次进入战斗时，如果玩家死亡不能正常结束游戏（如果玩家获胜则不受影响）
- Boss 动画和音效仍在调整中
