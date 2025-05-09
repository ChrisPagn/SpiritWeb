﻿using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpiritWeb.Shared.Models
{
    /// <summary>
    /// Modèle représentant une enquête de satisfaction et suggestion d'optimisation
    /// </summary>
    [FirestoreData]
    public class SurveyModel
    {
        /// <summary>
        /// Identifiant unique de l'enquête (généré automatiquement par Firestore)
        /// </summary>
        [FirestoreDocumentId]
        public string Id { get; set; } = "0";

        /// <summary>
        /// Identifiant de l'utilisateur qui a soumis l'enquête
        /// </summary>
        [FirestoreProperty]
        public string UserId { get; set; }

        ///// <summary>
        ///// Nom d'affichage de l'utilisateur qui a soumis l'enquête
        ///// </summary>
        [FirestoreProperty]
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Date et heure de soumission de l'enquête
        /// </summary>
        [FirestoreProperty]
        public DateTime SubmissionDate { get; set; }

        /// <summary>
        /// Niveau de satisfaction global (5: Excellent, 4:Très bon, 3:Bon, 2:Passable, 1:À améliorer)
        /// </summary>
        [FirestoreProperty]
        public int SatisfactionRating { get; set; }

        /// <summary>
        /// Fréquence de jeu (1: Quotidien, 2: Plusieurs fois/semaine, 3: Hebdomadaire, 4: Mensuel, 5: Rarement)
        /// </summary>
        [FirestoreProperty]
        public int PlayFrequency { get; set; }

        /// <summary>
        /// Fonctionnalité préférée du jeu ( Système de niveaux, Collection d'objets, Système de pièces, Interface utilisateur, Autre)
        /// </summary>
        [FirestoreProperty]
        public string FavoriteFeature { get; set; }

        /// <summary>
        /// Suggestion d'optimisation proposée par l'utilisateur
        /// </summary>
        private string _optimizationSuggestion = "";
        [FirestoreProperty]
        //public string OptimizationSuggestion { get; set; }
        public string OptimizationSuggestion
        {
            get => _optimizationSuggestion;
            set
            {
                if (IsValidSuggestion(value))
                    _optimizationSuggestion = value;
            }
        }

        /// <summary>
        /// Vérifie si la suggestion d'optimisation est valide
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsValidSuggestion(string input)
        {
            var regex = new Regex(@"^[\w\s.,!?àâäéèêëîïôöùûüçÀÂÄÉÈÊËÎÏÔÖÙÛÜÇ-]{10,300}$");
            return regex.IsMatch(input) && !input.Contains("<") && !input.Contains(">");
        }

        [FirestoreProperty]
        public int VotesCount { get; set; }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public SurveyModel()
        {
            // Constructeur par défaut
            Id = "0";
        }

        /// <summary>
        /// Constructeur avec paramètres
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userDisplayName"></param>
        /// <param name="submissionDate"></param>
        /// <param name="satisfactionRating"></param>
        /// <param name="playFrequency"></param>
        /// <param name="favoriteFeature"></param>
        /// <param name="optimizationSuggestion"></param>
        public SurveyModel(string userId, string userDisplayName, DateTime submissionDate, int satisfactionRating, int playFrequency, string favoriteFeature, string optimizationSuggestion)
        {
            UserId = userId;
            UserDisplayName = userDisplayName;
            SubmissionDate = submissionDate;
            SatisfactionRating = satisfactionRating;
            PlayFrequency = playFrequency;
            FavoriteFeature = favoriteFeature;
            OptimizationSuggestion = optimizationSuggestion;
        }
    }
}