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
        /// <summary>
        /// Liste des utilisateurs ayant voté pour cette suggestion
        /// </summary>
        //[FirestoreProperty]
        public List<string> Votes { get; set; } = new List<string>();

        /// <summary>
        /// Nombre total de votes reçus
        /// </summary>
        //[FirestoreProperty]
        public int VotesCount { get; set; } = 0;

    }
}
