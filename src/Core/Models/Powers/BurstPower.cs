using System.Threading.Tasks;
using EXLagavulin.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EXLagavulin.Core.Models.Powers;

public sealed class SplitPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented && base.Owner == target)
        {
            await Cmd.CustomScaledWait(deathAnimLength, deathAnimLength);

            // 生成两个 Lagavulin
            var lag1 = ModelDb.Monster<Lagavulin>().ToMutable();
            var lag2 = ModelDb.Monster<Lagavulin>().ToMutable();
            

            await CreatureCmd.Add(lag1, base.CombatState, base.Owner.Side, "lagavulin1");
            await CreatureCmd.Add(lag2, base.CombatState, base.Owner.Side, "lagavulin2");
        }
    }

    public override bool ShouldStopCombatFromEnding()
    {
        // 只有当母体还活着时才阻止结束
        return base.Owner != null && base.Owner.IsAlive;
    }
}