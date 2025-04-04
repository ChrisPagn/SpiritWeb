using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace SpiritWeb.Shared.Models
{
    [FirestoreData]
    public class SaveData
    {
        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string DisplayName { get; set; }

        [FirestoreProperty]
        public int CoinsCount { get; set; }

        [FirestoreProperty]
        public int LevelReached { get; set; }

        [FirestoreProperty]
        public string LastLevelPlayed { get; set; } 

        [FirestoreProperty]
        public List<int> InventoryItems { get; set; } = new List<int>();

        [FirestoreProperty]
        public List<string> InventoryItemsName { get; set; } = new List<string>();

        [FirestoreProperty]
        public DateTime LastModified { get; set; }

        
        public SaveData() { }

        public SaveData(string userId, string displayName, int coinsCount,
                      int levelReached, List<int> inventoryItems,
                      List<string> inventoryItemsName, DateTime lastModified,
                      string lastLevelPlayed)
        {
            UserId = userId;
            DisplayName = displayName;
            CoinsCount = coinsCount;
            LevelReached = levelReached;
            InventoryItems = inventoryItems ?? new List<int>();
            InventoryItemsName = inventoryItemsName ?? new List<string>();
            LastModified = lastModified;
            LastLevelPlayed = lastLevelPlayed;
        }
    }
}