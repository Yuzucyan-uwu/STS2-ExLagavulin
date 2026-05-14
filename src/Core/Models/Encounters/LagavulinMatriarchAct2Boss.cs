using System.Collections.Generic;
using EXLagavulin.Core.Models.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace EXLagavulin.Core.Models.Encounters;

public sealed class LagavulinMatriarchAct2Boss : EncounterModel
{
    private const string _matriarchSlot = "matriarch";
    private const string _lagavulinSlotPrefix = "lagavulin";

    public override RoomType RoomType => RoomType.Boss;

    public override string CustomBgm => "event:/music/act2_boss_knowledge_demon";

    public override string BossNodePath => "res://images/map/placeholder/lagavulin_matriarch_act2_boss_icon";

    public override MegaSkeletonDataResource? BossNodeSpineResource => null;

    public override bool HasScene => true;
    
    // 所有可能出现的怪物
    public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
    {
        ModelDb.Monster<LagavulinMatriarchAct2>(),
        ModelDb.Monster<Lagavulin>()
    };

    // 定义场景中的所有槽位
    public override IReadOnlyList<string> Slots => new[]
    {
        _matriarchSlot,
        GetLagavulinSlotName(1),
        GetLagavulinSlotName(2)
    };

    protected override bool HasCustomBackground => false;   // 先关闭，避免背景报错

    public override float GetCameraScaling() => 0.85f;
    public override Vector2 GetCameraOffset() => Vector2.Down * 70f;

    // 获取小怪槽位名称
    public static string GetLagavulinSlotName(int index)
    {
        return $"{_lagavulinSlotPrefix}{index}";
    }

    // 生成怪物时指定位置
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new[]
        {
            (ModelDb.Monster<LagavulinMatriarchAct2>().ToMutable(), _matriarchSlot)
        };
    }
}