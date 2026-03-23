using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace XiaYiZhou_Logic
{
    public class PortraitManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly Dictionary<string, CharacterConfig> _characters = new();
        private readonly Dictionary<string, PortraitGridInfo> _gridCache = new();
        private readonly Dictionary<Texture2D, string> _textureToAssetKey = new(); // 纹理→assetKey映射

        public PortraitManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public void AddCharacter(CharacterConfig character) => _characters[character.Id!] = character;

        /// <summary>获取角色肖像路径（支持事件、季节、默认）</summary>
        public string GetCurrentPortraitPath(string assetKey)
        {
            if (!_characters.TryGetValue(assetKey, out var character))
                return null!;

            // 1. 事件规则
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

            // 2. 季节规则
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

            // 3. 默认肖像
            if (!string.IsNullOrEmpty(character.DefaultPortraitPath))
            {
                string fullPath = Path.Combine(_helper.DirectoryPath, character.DefaultPortraitPath);
                if (File.Exists(fullPath))
                    return character.DefaultPortraitPath;
                else
                    _monitor.Log($"Default portrait missing: {character.DefaultPortraitPath}", LogLevel.Warn);
            }

            return null!;
        }

        /// <summary>获取纹理网格信息</summary>
        public PortraitGridInfo GetPortraitGridInfo(string assetKey, Texture2D texture)
        {
            if (_gridCache.TryGetValue(assetKey, out var info))
                return info;

            if (!_characters.TryGetValue(assetKey, out var config))
                return new PortraitGridInfo(64, texture.Width / 64);

            // 优先使用配置中的值
            if (config.GridTileSize.HasValue && config.GridColumns.HasValue)
            {
                info = new PortraitGridInfo(config.GridTileSize.Value, config.GridColumns.Value);
            }
            else
            {
                int gcd = GCD(texture.Width, texture.Height);
                int tileSize = gcd;
                int columns = texture.Width / tileSize;
                if (texture.Width % tileSize != 0 || texture.Height % tileSize != 0)
                {
                    tileSize = 64;
                    columns = texture.Width / 64;
                }
                info = new PortraitGridInfo(tileSize, columns);
            }

            _gridCache[assetKey] = info;
            return info;
        }

        /// <summary>记录纹理与assetKey的映射</summary>
        public void RegisterTexture(Texture2D texture, string assetKey)
        {
            if (!_textureToAssetKey.ContainsKey(texture))
                _textureToAssetKey[texture] = assetKey;
        }

        /// <summary>根据纹理获取assetKey</summary>
        public string GetAssetKeyFromTexture(Texture2D texture)
        {
            _textureToAssetKey.TryGetValue(texture, out var key);
            return key!;
        }

        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public IEnumerable<string> GetAllCharacterAssetKeys() => _characters.Keys;
    }

    public class CharacterConfig
    {
        public string? Id { get; set; }
        public string? DefaultPortraitPath { get; set; }
        public string? SeasonTemplate { get; set; }
        public List<EventRuleConfig>? EventRules { get; set; }
        public int? GridTileSize { get; set; }   // 可选：手动指定格子大小
        public int? GridColumns { get; set; }    // 可选：手动指定一行格子数
    }

    public class EventRuleConfig
    {
        public string? EventId { get; set; }
        public string? PortraitPath { get; set; }
    }

    public record PortraitGridInfo(int TileSize, int Columns);
}
