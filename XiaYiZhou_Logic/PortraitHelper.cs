using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace XiaYiZhou_Logic
{
    public static class PortraitHelper
    {
        private static FieldInfo? _dialogueBoxPortraitTextureField;
        private static FieldInfo? _dialogueBoxCurrentPortraitIndexField;
        private static FieldInfo? _dialogueBoxCharacterField;   // 用于获取 NPC
        private static MethodInfo? _npcGetPortraitTextureMethod;

        static PortraitHelper()
        {
            // DialogueBox 私有字段
            _dialogueBoxPortraitTextureField = typeof(DialogueBox).GetField("portraitTexture", BindingFlags.Instance | BindingFlags.NonPublic);
            _dialogueBoxCurrentPortraitIndexField = typeof(DialogueBox).GetField("currentPortraitIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            _dialogueBoxCharacterField = typeof(DialogueBox).GetField("character", BindingFlags.Instance | BindingFlags.NonPublic);

            // NPC 公开方法
            _npcGetPortraitTextureMethod = typeof(NPC).GetMethod("getPortraitTexture", BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>获取对话气泡的肖像纹理</summary>
        public static Texture2D GetPortraitTexture(DialogueBox dialogueBox)
        {
            return _dialogueBoxPortraitTextureField?.GetValue(dialogueBox) as Texture2D;
        }

        /// <summary>获取对话气泡当前表情索引</summary>
        public static int GetCurrentPortraitIndex(DialogueBox dialogueBox)
        {
            return (int)(_dialogueBoxCurrentPortraitIndexField?.GetValue(dialogueBox) ?? 0);
        }

        /// <summary>获取对话气泡关联的 NPC</summary>
        public static NPC GetDialogueBoxNPC(DialogueBox dialogueBox)
        {
            return _dialogueBoxCharacterField?.GetValue(dialogueBox) as NPC;
        }

        /// <summary>获取 NPC 的肖像纹理（调用公开方法）</summary>
        public static Texture2D GetPortraitTexture(NPC npc)
        {
            return _npcGetPortraitTextureMethod?.Invoke(npc, null) as Texture2D;
        }

        /// <summary>根据纹理和索引计算源矩形</summary>
        public static Rectangle GetSourceRectangle(Texture2D texture, int index, int tileSize, int columns)
        {
            int row = index / columns;
            int col = index % columns;
            return new Rectangle(col * tileSize, row * tileSize, tileSize, tileSize);
        }

        /// <summary>为 DialogueBox 获取源矩形（自动获取 NPC 和网格信息）</summary>
        public static Rectangle GetSourceRectangleForDialogueBox(DialogueBox dialogueBox, PortraitManager manager)
        {
            var texture = GetPortraitTexture(dialogueBox);
            if (texture == null) return new Rectangle(0, 0, 64, 64);

            int index = GetCurrentPortraitIndex(dialogueBox);
            // 从对话气泡的 NPC 获取 assetKey
            var npc = GetDialogueBoxNPC(dialogueBox);
            if (npc == null) return new Rectangle(0, 0, 64, 64);

            string assetKey = $"Portraits/{npc.Name}";
            var gridInfo = manager.GetPortraitGridInfo(assetKey, texture);
            return GetSourceRectangle(texture, index, gridInfo.TileSize, gridInfo.Columns);
        }
    }
}
