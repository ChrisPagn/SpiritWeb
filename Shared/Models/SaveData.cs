using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpiritWeb.Shared.Models
{
    /// <summary>
    /// Classe représentant les données de sauvegarde d'un utilisateur dans Firestore.
    /// </summary>
    [FirestoreData]
    public class SaveData
    {
        /// <summary>
        /// Identifiant unique de l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public string UserId { get; set; }

        /// <summary>
        /// Nom d'affichage de l'utilisateur.
        /// </summary>
        ///     [Required(ErrorMessage = "Le nom est requis")]
        [RegularExpression(@"^[\p{L}\p{N}\s\-_']{2,20}$",
            ErrorMessage = "Caractères invalides (2-20 caractères, lettres, chiffres, -_')")]
        [FirestoreProperty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Nombre de pièces (coins) possédées par l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public int CoinsCount { get; set; }

        /// <summary>
        /// Niveau le plus élevé atteint par l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public int LevelReached { get; set; }

        /// <summary>
        /// Dernier niveau joué par l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public string LastLevelPlayed { get; set; } = "Pas encore atteint de niveau!";

        /// <summary>
        /// Liste des identifiants des objets dans l'inventaire de l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public List<int> InventoryItems { get; set; } = new List<int>();

        /// <summary>
        /// Liste des noms des objets dans l'inventaire de l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public List<string> InventoryItemsName { get; set; } = new List<string>();

        /// <summary>
        /// Date et heure de la dernière modification des données de l'utilisateur.
        /// </summary>
        [FirestoreProperty]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Rôle de l'utilisateur (ex: "admin", "contributor", "user").
        /// </summary>
        [FirestoreProperty]
        public string Role { get; set; } = "user";

        

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SaveData"/>.
        /// </summary>
        public SaveData() { }

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SaveData"/> avec les valeurs spécifiées.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        /// <param name="coinsCount">Nombre de pièces possédées par l'utilisateur.</param>
        /// <param name="levelReached">Niveau le plus élevé atteint par l'utilisateur.</param>
        /// <param name="inventoryItems">Liste des identifiants des objets dans l'inventaire.</param>
        /// <param name="inventoryItemsName">Liste des noms des objets dans l'inventaire.</param>
        /// <param name="lastModified">Date et heure de la dernière modification des données.</param>
        /// <param name="lastLevelPlayed">Dernier niveau joué par l'utilisateur.</param>
        public SaveData(string userId, string displayName, int coinsCount,
                        int levelReached, List<int> inventoryItems,
                        List<string> inventoryItemsName, DateTime lastModified,
                        string lastLevelPlayed, string roles)
        {
            UserId = userId;
            DisplayName = displayName;
            CoinsCount = coinsCount;
            LevelReached = levelReached;
            InventoryItems = inventoryItems ?? new List<int>();
            InventoryItemsName = inventoryItemsName ?? new List<string>();
            LastModified = lastModified;
            LastLevelPlayed = lastLevelPlayed;
            Role = roles;
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut être promu au rôle de contributeur.
        /// </summary>
        public void CheckForPromotion()
        {
            if (Role == "user" && CoinsCount >= 100)
            {
                Role = "contributor";
            }
        }
    }
}
