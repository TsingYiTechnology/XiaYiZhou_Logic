using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

namespace XiaYiZhou_Logic
{
    public class ModEntry : Mod
    {
        private PortraitManager? _manager;
        private string? _lastEventId;

        public override void Entry(IModHelper helper)
        {
            _manager = new PortraitManager(helper, Monitor);
            LoadCharacters();

#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            // 订阅资源请求事件
            helper.Events.Content.AssetRequested += OnAssetRequested;

            // 订阅条件变化事件
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped += OnWarped;
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
        }

        private void LoadCharacters()
        {
            // 阿比盖尔配置
            _manager!.AddCharacter(new CharacterConfig
            {
                Id = "Portraits/XiaYiZhou",
                DefaultPortraitPath = "assets/XiaYiZhou_default.png",
                SeasonTemplate = "assets/XiaYiZhou/XiaYiZhou_{{season}}.png",
                EventRules = new List<EventRuleConfig>
                {
                    // new EventRuleConfig { EventId = "558291", PortraitPath = "assets/XiaYiZhou/event_4heart.png" }
                }
            });

            // 可以继续添加其他角色...
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            // 只处理我们关心的肖像资源
            if (!e.Name.StartsWith("Portraits/"))
                return;

            string assetKey = e.Name.Name; // 例如 "Portraits/Abigail"
            string customPath = _manager!.GetCurrentPortraitPath(assetKey);
            if (customPath == null)
                return; // 没有自定义文件，让游戏使用原版

            // 提供自定义纹理
            e.LoadFromModFile<Microsoft.Xna.Framework.Graphics.Texture2D>(customPath, AssetLoadPriority.Exclusive);
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
