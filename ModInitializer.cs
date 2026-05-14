using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace EXLagavulin   // ← 这里改成你的模组ID（必须和你的.json文件中的"id"一致！）
{
    // 通用 ModInitializer
    [ModInitializer(nameof(Initialize))]
    public static class ModInitializer
    {
        public static void Initialize()
        {
            {
                // 1. 初始化 Harmony（强烈推荐这样写，避免与其他模组冲突。）
                var harmony = new Harmony("EXLagavulin.yuzucyan");   // 格式：模组ID.作者名
                harmony.PatchAll();

                // 2. 在这里添加你的注册逻辑（卡牌、遗物、药水等）
                //ModHelper.AddModelToPool(typeof( 卡牌池 ), typeof( 卡牌名字 ));
                //ModHelper.AddModelToPool(typeof( 遗物池 ), typeof( 遗物名字 ));
                //ModHelper.AddModelToPool(typeof( 药水池 ), typeof( 药水名字 ));
            }
            Log.Info("加载成功！");// 可以删掉
        }
    }
}