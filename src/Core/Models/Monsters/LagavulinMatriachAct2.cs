using System.Collections.Generic;
using System.Threading.Tasks;
using EXLagavulin.Core.Models.Powers;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using BurstPower = MegaCrit.Sts2.Core.Models.Powers.BurstPower;

namespace EXLagavulin.Core.Models.Monsters;

public sealed class LagavulinMatriarchAct2 : MonsterModel
{
	private const string _sleepMoveId = "SLEEP_MOVE";

	public const string slashMoveId = "SLASH_MOVE";

	private const string _sleepTrigger = "Sleep";

	public const string wakeTrigger = "Wake";

	private const string _attackHeavyTrigger = "AttackHeavy";

	private const string _attackDoubleTrigger = "AttackDouble";
	
	private bool _isAwake;

	private bool _isShellAwake;

	private NSleepingVfx? _sleepingVfx;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 277, 268);

	public override int MaxInitialHp => MinInitialHp;

	private int SlashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 23, 21);

	private int Slash2Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	private int Slash2Block => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 16, 14);

	private int DisembowelDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 11);

	private int DisembowelRepeat => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

	public bool IsAwake
	{
		get
		{
			return _isAwake;
		}
		set
		{
			AssertMutable();
			_isAwake = value;
		}
	}

	public bool IsShellAwake
	{
		get
		{
			return _isShellAwake;
		}
		set
		{
			AssertMutable();
			_isShellAwake = value;
		}
	}

	private NSleepingVfx? SleepingVfx
	{
		get
		{
			return _sleepingVfx;
		}
		set
		{
			AssertMutable();
			_sleepingVfx = value;
		}
	}
	

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await Sleep();
	}

	private async Task Sleep()
	{
		IsAwake = false;
		await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), base.Creature, 12m, base.Creature, null);
		await PowerCmd.Apply<ExSleepPower>(new ThrowingPlayerChoiceContext(), base.Creature, 3m, base.Creature, null);
		await PowerCmd.Apply<SplitPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Creature)
		{
			return Task.CompletedTask;
		}
		if (SleepingVfx != null)
		{
			SleepingVfx?.Stop();
			SleepingVfx = null;
		}
		if (base.Creature.CurrentHp <= base.Creature.MaxHp / 2 && !IsShellAwake)
		{
			NCreature creatureNode = base.Creature.GetCreatureNode();
			IsShellAwake = true;
		}
		return Task.CompletedTask;
	}

	public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != base.Creature)
		{
			return Task.CompletedTask;
		}
		SleepingVfx?.Stop();
		SleepingVfx = null;
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SLEEP_MOVE", SleepMove, new SleepIntent());
		MoveState moveState2 = new MoveState("SLASH_MOVE", SlashMove, new SingleAttackIntent(SlashDamage));
		MoveState moveState3 = new MoveState("SLASH2_MOVE", Slash2Move, new SingleAttackIntent(Slash2Damage), new DefendIntent());
		MoveState moveState4 = new MoveState("DISEMBOWEL_MOVE", DisembowelMove, new MultiAttackIntent(DisembowelDamage, DisembowelRepeat));
		MoveState moveState5 = new MoveState("SOUL_SIPHON_MOVE", SoulSiphonMove, new DebuffIntent(), new BuffIntent());
		ConditionalBranchState conditionalBranchState = (ConditionalBranchState)(moveState.FollowUpState = new ConditionalBranchState("SLEEP_BRANCH"));
		moveState2.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState2;
		conditionalBranchState.AddState(moveState, () => base.Creature.HasPower<ExSleepPower>());
		conditionalBranchState.AddState(moveState2, () => !base.Creature.HasPower<ExSleepPower>());
		list.Add(conditionalBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState5);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private Task SleepMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}
	
	private async Task SlashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SlashDamage).FromMonster(this).WithAttackerAnim(null, 0.3f)
			.WithAttackerFx(null, null)
			.WithHitFx(null)
			.Execute(null);
	}

	public async Task WakeUpMove(IReadOnlyList<Creature> _)
	{
		if (!_isAwake)
		{
			IsAwake = true;
		}
	}
	

	private async Task Slash2Move(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(Slash2Damage).FromMonster(this).WithAttackerAnim(null, 0.2f)
			.WithAttackerFx(null, null)
			.WithHitFx(null)
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, Slash2Block, ValueProp.Move, null);
	}
	
	private async Task DisembowelMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DisembowelDamage).WithHitCount(DisembowelRepeat).FromMonster(this)
			.WithAttackerAnim(null, 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, null)
			.WithHitFx(null)
			.Execute(null);
	}
	

	private async Task SoulSiphonMove(IReadOnlyList<Creature> targets)
	{
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -2m, base.Creature, null);
		await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -2m, base.Creature, null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, 2m, base.Creature, null);
	}
	

	protected override bool ShouldShowMoveInBestiary(string moveStateId)
	{
		return moveStateId != "SLEEP_MOVE";
	}

	
}
