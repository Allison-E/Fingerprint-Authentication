using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPFP;

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
        
        static DBHandler Instance
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
        bool StoreFingerprintTemplateInDBAsync(Template template)
        {
            return false;
        }

        // Todo: Work on this.
        byte[] retrieveFingerprintFromDBAsync()
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
