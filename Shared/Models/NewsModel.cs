using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpiritWeb.Shared.Models
{
    /// <summary>
    /// Modèle représentant une actualité concernant le jeu Spirit
    /// </summary>
    [FirestoreData]
    public class NewsModel
    {
        /// <summary>
        /// Identifiant unique de l'actualité (généré automatiquement par Firestore)
        /// </summary>
        [FirestoreDocumentId]
        public string Id { get; set; } = "0";

        /// <summary>
        /// Titre de l'actualité
        /// </summary>
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [RegularExpression(@"^[\p{L}\p{N}\s\-_',;:!?()]{5,100}$",
            ErrorMessage = "5-100 caractères (lettres, chiffres, ponctuation de base)")]
        [FirestoreProperty]
        public string Title { get; set; }

        /// <summary>
        /// Contenu de l'actualité
        /// </summary>
        [Required(ErrorMessage = "Le contenu est obligatoire")]
        [RegularExpression(@"^[\p{L}\p{N}\s\-_',;:!?()\r\n]{20,5000}$",
            ErrorMessage = "20-5000 caractères (lettres, chiffres, ponctuation)")]
        [FirestoreProperty]
        public string Content { get; set; }

        /// <summary>
        /// Date de publication de l'actualité
        /// </summary>
        [FirestoreProperty]
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur administrateur qui a créé/modifié l'actualité
        /// </summary>
        [FirestoreProperty]
        public string AdminId { get; set; }

        /// <summary>
        /// Nom d'affichage de l'administrateur
        /// </summary>
        [FirestoreProperty]
        public string AdminDisplayName { get; set; }

        /// <summary>
        /// Date de dernière modification de l'actualité
        /// </summary>
        [FirestoreProperty]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indique si l'actualité est épinglée en haut de la liste
        /// </summary>
        [FirestoreProperty]
        public bool IsPinned { get; set; }

        /// <summary>
        /// Chemin de l'image associée à l'actualité (optionnel)
        /// </summary>
        [FirestoreProperty]
        public string ImagePath { get; set; }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public NewsModel()
        {
            // Constructeur par défaut
            Id = "0";
            PublishDate = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructeur avec paramètres
        /// </summary>
        public NewsModel(string title, string content, string adminId, string adminDisplayName, bool isPinned = false, string imagePath = null)
        {
            Title = title;
            Content = content;
            PublishDate = DateTime.UtcNow;
            AdminId = adminId;
            AdminDisplayName = adminDisplayName;
            LastModified = DateTime.UtcNow;
            IsPinned = isPinned;
            ImagePath = imagePath;
        }
    }
}