using System.Collections.Generic;
using System.Threading.Tasks;
using EXLagavulin.Core.Models.Powers;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace EXLagavulin.Core.Models.Monsters;

public sealed class Lagavulin : MonsterModel
{
    public override int MinInitialHp => 109;
    public override int MaxInitialHp => 111;

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var list = new List<MonsterState>();

        var stunned = new MoveState("STUNNED", SleepMove, new StunIntent());
        var attack = new MoveState("ATTACK", AttackMove, new SingleAttackIntent(16));
        var debuff = new MoveState("DEBUFF", DebuffMove, new DebuffIntent());
        var multi = new MoveState("MULTI", MultiAttackMove, new MultiAttackIntent(5, 3));
        
        stunned.FollowUpState = attack;
        attack.FollowUpState = debuff;
        debuff.FollowUpState = multi;
        multi.FollowUpState = attack;

        list.Add(stunned);
        list.Add(attack);
        list.Add(debuff);
        list.Add(multi);

        return new MonsterMoveStateMachine(list, stunned);
    }

    private Task SleepMove(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(16).FromMonster(this).Execute(null);
    }

    private async Task DebuffMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -1m, base.Creature, null);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -1m, base.Creature, null);
    }

    private async Task MultiAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(5).WithHitCount(3).FromMonster(this).Execute(null);
    }
}