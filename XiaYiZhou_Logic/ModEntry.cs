using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using System.Collections.Generic;

namespace XiaYiZhou_Logic
{
    public class ModEntry : Mod
    {
        private static PortraitManager? _staticManager;
        private PortraitManager? _manager;
        private string? _lastEventId;

        public static PortraitManager GetPortraitManager() => _staticManager!;

        public override void Entry(IModHelper helper)
        {
            _manager = new PortraitManager(helper, Monitor);
            _staticManager = _manager;
            LoadCharacters();

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped += OnWarped;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void LoadCharacters()
        {
            _manager!.AddCharacter(new CharacterConfig
            {
                Id = "Xiayizhou",           // 对应资源键 "Portraits/Xiayizhou"
                DefaultPortraitPath = "assets/XiaYiZhou_default.png",
                SeasonTemplate = "assets/XiaYiZhou/XiaYiZhou_{{season}}.png",
                GridTileSize = 500,
                GridColumns = 2,
                EventRules = new List<EventRuleConfig>()
            });
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.Name.StartsWith("Portraits/"))
                return;

            string assetKey = e.Name.Name;
            string customPath = _manager!.GetCurrentPortraitPath(assetKey);
            if (customPath == null)
                return;

            e.LoadFromModFile<Microsoft.Xna.Framework.Graphics.Texture2D>(customPath, AssetLoadPriority.Exclusive);
            // 注意：无法在此处获取加载后的纹理，因为 LoadFromModFile 是异步的，所以纹理映射需要在补丁中动态获取
            // 替代方案：在绘制时通过 assetKey 动态获取纹理，不依赖映射表
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            InvalidateAllPortraits();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(60)) return; // 每秒检查一次

            string currentEventId = Game1.eventUp! ? Game1.CurrentEvent?.id! : null!;
            if (currentEventId != _lastEventId)
            {
                _lastEventId = currentEventId;
                InvalidateAllPortraits();
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            InvalidateAllPortraits();
        }

        private void InvalidateAllPortraits()
        {
            foreach (string assetKey in _manager!.GetAllCharacterAssetKeys())
            {
                Helper.GameContent.InvalidateCache(assetKey);
            }
        }
    }
}
