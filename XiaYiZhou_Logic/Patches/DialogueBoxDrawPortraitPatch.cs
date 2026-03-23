using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace XiaYiZhou_Logic.Patches
{
    [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.drawPortrait))]
    public static class DialogueBoxDrawPortraitPatch
    {
        // 缓存原方法中绘制名字的方法（私有方法，通过反射调用）
        private static MethodInfo? _drawPortraitNameMethod;

        static DialogueBoxDrawPortraitPatch()
        {
            // 获取 DialogueBox 的私有方法 drawPortraitName（实际方法名可能是 drawPortraitName 或类似）
            _drawPortraitNameMethod = typeof(DialogueBox).GetMethod("drawPortraitName", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        static bool Prefix(DialogueBox __instance, SpriteBatch b)
        {
            // 获取纹理和索引
            var texture = PortraitHelper.GetPortraitTexture(__instance);
            if (texture == null) return true; // 没有纹理，继续原方法

            int portraitIndex = PortraitHelper.GetCurrentPortraitIndex(__instance);
            if (portraitIndex < 0) portraitIndex = 0;

            // 获取网格信息
            var manager = ModEntry.GetPortraitManager();
            var npc = PortraitHelper.GetDialogueBoxNPC(__instance);
            if (npc == null) return true;

            string assetKey = $"Portraits/{npc.Name}";
            var gridInfo = manager.GetPortraitGridInfo(assetKey, texture);
            var sourceRect = PortraitHelper.GetSourceRectangle(texture, portraitIndex, gridInfo.TileSize, gridInfo.Columns);

            // 目标矩形（原版绘制位置）
            int x = __instance.x + 16;
            int y = __instance.y + 96;
            var destRect = new Rectangle(x, y, 64, 64);

            // 绘制头像
            b.Draw(texture, destRect, sourceRect, Color.White);

            // 调用原方法中绘制名字的部分（避免重复绘制头像）
            _drawPortraitNameMethod?.Invoke(__instance, new object[] { b });

            // 阻止原方法继续执行（因为我们已经绘制了头像和名字）
            return false;
        }
    }
}
