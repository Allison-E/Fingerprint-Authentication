using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPFP;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Fingerprint_Authentication.DB
{
    public class DBHandler
    {
        static DBHandler _instance;
        string id;
        int noOfChangesAllowedForTheId;
        SqlCommand command;
        SqlConnectionStringBuilder connectionStringBuilder;
        SqlConnection connection;
        bool hasFinishedGettingFingerprintsFromDB;

        public event PropertyChangedEventHandler HasFinishedFingerprintTransfer;
        public bool HasFinishedGettingFingerprintsFromDB
        {
            get
            {
                return hasFinishedGettingFingerprintsFromDB;
            }
            private set
            {
                if (hasFinishedGettingFingerprintsFromDB != value)
                {
                    hasFinishedGettingFingerprintsFromDB = value;
                    onHasFinishedGettingFingerPrintsFromDB();
                }
            }
        }
        public static DBHandler Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                else
                {
                    _instance = new DBHandler();
                    return _instance;
                }
            }
        }
        
        private DBHandler()
        {
            noOfChangesAllowedForTheId = 1;
            initialiseSqlStuff();
        }

        private void onHasFinishedGettingFingerPrintsFromDB([CallerMemberName]string propertyName = "")
        {
            HasFinishedFingerprintTransfer?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// This sets the ID/key used to store the user's information in the DB.
        /// </summary>
        /// <remarks>
        /// Note: This can only be set once in the lifetime of this application.
        /// </remarks>
        /// <param name="Id">The ID/key</param>
        public void SetID(string Id)
        {
            if (noOfChangesAllowedForTheId != 0)
            {
                id = Id;
                noOfChangesAllowedForTheId--;
            }
        }

        // Todo: Work on this.
        public bool StoreFingerprintInDBAsync(Template template)
        {
            return false;
        }

        // Todo: Work on this.
        /// <summary>
        /// Returns a collection of users IDs and their serialisedFingerprints.
        /// </summary>
        /// <returns>A <c>Dictionary<string, byte[]></c> where byte[] is the serialised fingerprint and string is the ID of the user.</returns>
        public Dictionary<byte[], string> GetFingerprintsFromDBAsync()
        {
            return null;
        }

        private void initialiseSqlStuff()
        {
            command = new SqlCommand();
            connectionStringBuilder = new SqlConnectionStringBuilder();
            connection = new SqlConnection();

            connectionStringBuilder.DataSource = "";    // Put in the name or network address of the instance of your SQL server here.
            connectionStringBuilder.InitialCatalog = ""; // Put in the name of the DB here.
            connectionStringBuilder.Password = "";  // Put in the password of your DB here (if there's one).
            connectionStringBuilder.UserID = "";    // Put in the admin ID here.

            connection.ConnectionString = connectionStringBuilder.ConnectionString;
            command.Connection = connection;
        }
    }
}
