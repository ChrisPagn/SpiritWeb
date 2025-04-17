using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritWeb.Shared.Models
{
    [FirestoreData]
    public class Vote
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = "0";

        [FirestoreProperty]
        public string SuggestionId { get; set; }

        [FirestoreProperty]
        public List<string> UserIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public int VotesCount { get; set; } = 0;

    }
}
