using System;
using UnityEngine;

namespace AutoFarm.Utilities
{
    /// <summary>
    /// Utility class containing various formatting functions used throughout the game
    /// </summary>
    public static class FormatUtilities
    {
        #region Time Formatting
        
        /// <summary>
        /// Formats time in seconds to MM:SS format
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Formatted time string (MM:SS)</returns>
        public static string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0)
                return "00:00";
                
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Formats time in seconds to HH:MM:SS format
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Formatted time string (HH:MM:SS)</returns>
        public static string FormatTimeWithHours(float timeInSeconds)
        {
            if (timeInSeconds <= 0)
                return "00:00:00";
                
            int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
            int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Formats time with custom labels (e.g., "5m 30s")
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <param name="includeSeconds">Whether to include seconds in the output</param>
        /// <returns>Formatted time string with labels</returns>
        public static string FormatTimeWithLabels(float timeInSeconds, bool includeSeconds = true)
        {
            if (timeInSeconds <= 0)
                return includeSeconds ? "0s" : "0m";
                
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            
            if (minutes > 0)
            {
                if (includeSeconds && seconds > 0)
                    return $"{minutes}m {seconds}s";
                else
                    return $"{minutes}m";
            }
            else
            {
                return includeSeconds ? $"{seconds}s" : "0m";
            }
        }
        
        #endregion
        
        #region Number Formatting
        
        /// <summary>
        /// Formats numbers with appropriate suffixes (K, M, B, etc.)
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted number string</returns>
        public static string FormatNumber(long number)
        {
            if (number >= 1000000000)
                return $"{number / 1000000000.0:F1}B";
            else if (number >= 1000000)
                return $"{number / 1000000.0:F1}M";
            else if (number >= 1000)
                return $"{number / 1000.0:F1}K";
            else
                return number.ToString();
        }
        
        /// <summary>
        /// Formats currency with appropriate formatting
        /// </summary>
        /// <param name="amount">Currency amount</param>
        /// <param name="currencySymbol">Currency symbol (default: $)</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatCurrency(long amount, string currencySymbol = "$")
        {
            return $"{currencySymbol}{FormatNumber(amount)}";
        }
        
        /// <summary>
        /// Formats percentage values
        /// </summary>
        /// <param name="value">Value to format as percentage</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>Formatted percentage string</returns>
        public static string FormatPercentage(float value, int decimals = 1)
        {
            return $"{(value * 100).ToString($"F{decimals}")}%";
        }
        
        #endregion
        
        #region Entity State Formatting
        
        /// <summary>
        /// Formats entity state with appropriate time information
        /// </summary>
        /// <param name="state">Entity state</param>
        /// <param name="timeRemaining">Time remaining in seconds</param>
        /// <returns>Formatted state string</returns>
        public static string FormatEntityState(string state, float timeRemaining)
        {
            if (timeRemaining <= 0)
                return state;
                
            return $"{FormatTime(timeRemaining)} ({state})";
        }
        
        #endregion
        
        #region Resource Formatting
        
        /// <summary>
        /// Formats resource amounts with icons or labels
        /// </summary>
        /// <param name="amount">Resource amount</param>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="useIcon">Whether to use icon instead of name</param>
        /// <returns>Formatted resource string</returns>
        public static string FormatResource(int amount, string resourceName, bool useIcon = false)
        {
            string formattedAmount = FormatNumber(amount);
            string identifier = useIcon ? GetResourceIcon(resourceName) : resourceName;
            return $"{formattedAmount} {identifier}";
        }
        
        /// <summary>
        /// Gets resource icon based on resource name
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <returns>Resource icon string</returns>
        private static string GetResourceIcon(string resourceName)
        {
            // This could be expanded to use actual icon mappings
            return resourceName switch
            {
                "Wood" => "🪵",
                "Stone" => "🪨",
                "Gold" => "💰",
                "Food" => "🍎",
                _ => resourceName
            };
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Truncates text to specified length with ellipsis
        /// </summary>
        /// <param name="text">Text to truncate</param>
        /// <param name="maxLength">Maximum length</param>
        /// <returns>Truncated text</returns>
        public static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength - 3) + "...";
        }
        
        /// <summary>
        /// Capitalizes first letter of text
        /// </summary>
        /// <param name="text">Text to capitalize</param>
        /// <returns>Capitalized text</returns>
        public static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
        
        #endregion
    }
}
