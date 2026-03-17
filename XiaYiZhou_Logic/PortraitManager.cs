using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace XiaYiZhou_Logic
{
    public class PortraitManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly Dictionary<string, CharacterConfig> _characters = new();

        public PortraitManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public void AddCharacter(CharacterConfig character)
        {
            _characters[character.Id!] = character;
        }

        /// <summary>获取指定角色的当前肖像路径（若无可用的自定义路径，返回 null）</summary>
        public string GetCurrentPortraitPath(string assetKey)
        {
            if (!_characters.TryGetValue(assetKey, out var character))
                return null!; // 未配置的角色，使用原版

            // 1. 检查事件规则
            if (Game1.eventUp && Game1.CurrentEvent != null)
            {
                string currentEventId = Game1.CurrentEvent.id;
                var eventRule = character.EventRules?.FirstOrDefault(r => r.EventId == currentEventId);
                if (eventRule != null)
                {
                    string fullPath = Path.Combine(_helper.DirectoryPath, eventRule.PortraitPath!);
                    if (File.Exists(fullPath))
                        return eventRule.PortraitPath!;
                    else
                        _monitor.Log($"Event portrait missing: {eventRule.PortraitPath}", LogLevel.Warn);
                }
            }

            // 2. 检查季节规则
            if (!string.IsNullOrEmpty(character.SeasonTemplate))
            {
                string season = Game1.currentSeason;
                string seasonPath = character.SeasonTemplate.Replace("{{season}}", season);
                string fullPath = Path.Combine(_helper.DirectoryPath, seasonPath);
                if (File.Exists(fullPath))
                    return seasonPath;
                else
                    _monitor.Log($"Season portrait missing: {seasonPath}", LogLevel.Trace);
            }

            // 3. 回退到默认肖像
            if (!string.IsNullOrEmpty(character.DefaultPortraitPath))
            {
                string fullPath = Path.Combine(_helper.DirectoryPath, character.DefaultPortraitPath);
                if (File.Exists(fullPath))
                    return character.DefaultPortraitPath;
                else
                    _monitor.Log($"Default portrait missing: {character.DefaultPortraitPath}", LogLevel.Warn);
            }

            // 4. 没有任何自定义文件，使用原版
            return null!;
        }

        public IEnumerable<string> GetAllCharacterAssetKeys() => _characters.Keys;
    }

    public class CharacterConfig
    {
        public string? Id { get; set; }                // 资源键，例如 "Portraits/Abigail"
        public string? DefaultPortraitPath { get; set; }
        public string? SeasonTemplate { get; set; }    // 支持 {{season}} 占位符
        public List<EventRuleConfig>? EventRules { get; set; }
    }

    public class EventRuleConfig
    {
        public string? EventId { get; set; }
        public string? PortraitPath { get; set; }
    }
}