using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Fingerprint_Authentication.DB
{
    public class DBHandler
    {
        static DBHandler _instance;
        int id;
        int noOfChangesAllowedForTheId;
        SQLiteCommand command;
        SQLiteConnectionStringBuilder connectionStringBuilder;
        SQLiteConnection connection;
        
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

        /// <summary>
        /// This sets the ID/key associated to the user's information in the DB.
        /// </summary>
        /// <remarks>
        /// Note: This can only be set once in the lifetime of this application.
        /// </remarks>
        /// <param name="Id">The ID/key</param>
        public void SetID(int Id)
        {
            if (noOfChangesAllowedForTheId != 0)
            {
                id = Id;
                noOfChangesAllowedForTheId--;
            }
        }
        
        /// <summary>
        /// Stores the serialised fingerprint (of data type byte[]) in the database.
        /// </summary>
        /// <param name="serialisedFingerprint">A <c>byte[]</c> which is the serialised fingerprint.</param>
        /// <returns>A <c>Task<bool></c> which tells if the storage was successful or not.</returns>
        public Task<bool> StoreFingerprintInDBAsync(byte[] serialisedFingerprint)
        {
            bool isDone;
            command.CommandText = @"INSERT INTO eCapture_staff (id, finger_print)
                                    VALUES (" + id + ", @fingerprintParameter)";
            SQLiteParameter fingerprintParameter = new SQLiteParameter("@fingerprintParameter", serialisedFingerprint);
            fingerprintParameter.DbType = DbType.Binary;
            command.Parameters.Add(fingerprintParameter);

            return Task.Run(async () =>
            {
                isDone = false;

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    try
                    {
                        // Cross-checks to make sure the fingerprint was saved.
                        bool checkResult = await checkIfStorageOfFingerprintWorkedAsync(serialisedFingerprint);
                        if (checkResult)
                            isDone = true;
                    }
                    catch (CouldNotFindSavedFingerprintException)
                    {
                        throw new CouldNotStoreFingerprintInDBException();
                    }
                    connection.Close();
                }
                catch
                {
                    throw new CouldNotStoreFingerprintInDBException();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    }
                return isDone;

            });
        }

        public Task<Dictionary<byte[], int>> GetFingerprintsFromDBAsync()
        {
            command.CommandText = @"SELECT id, finger_print 
                                    FROM eCapture_staff";
            Dictionary<byte[], int> fingerprintsInDB = new Dictionary<byte[], int>();

            return Task.Run(() =>
            {
                try
                {
                    connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] fingerprint = null;
                            int id = readARow(reader, out fingerprint);
                            fingerprintsInDB.Add(fingerprint, id);
                        }
                    }
                    connection.Close();
                }
                catch
                {
                    throw new CouldNotFindSavedFingerprintException();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
                return fingerprintsInDB;
            });
        }

        private int readARow(IDataRecord record, out byte[] serialisedFingerprint)
        {
            serialisedFingerprint = (byte[])record[1];
            return (int)record[0];
        }

        private void initialiseSqlStuff()
        {
            command = new SQLiteCommand();
            connectionStringBuilder = new SQLiteConnectionStringBuilder();
            connection = new SQLiteConnection();

            connectionStringBuilder.DataSource = "";    // Put in the name or network address of the instance of your SQL server here.
            connectionStringBuilder.Password = "";  // Put in the password of your DB here (if there's one).

            connection.ConnectionString = connectionStringBuilder.ConnectionString;
            command.Connection = connection;
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToTheInternet()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }

        private Task<bool> checkIfStorageOfFingerprintWorkedAsync(byte[] originalByte)
        {
            command.CommandText = @"SELECT finger_print
                                    FROM eCapture_staff 
                                    WHERE id = " + id;
            byte[] byteFromDB;

            return Task.Run(() =>
            {
                try
                {
                    byteFromDB = command.ExecuteScalar() as byte[];
                }
                catch
                {
                    throw new CouldNotFindSavedFingerprintException();
                }

                return originalByte.SequenceEqual(byteFromDB);
            });
        }
    }

    [Serializable]
    public class CouldNotStoreFingerprintInDBException : Exception
    {
        public CouldNotStoreFingerprintInDBException() { }
        public CouldNotStoreFingerprintInDBException(string message) : base(message) { }
        public CouldNotStoreFingerprintInDBException(string message, Exception inner) : base(message, inner) { }
        protected CouldNotStoreFingerprintInDBException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class CouldNotFindSavedFingerprintException : Exception
    {
        public CouldNotFindSavedFingerprintException() { }
        public CouldNotFindSavedFingerprintException(string message) : base(message) { }
        public CouldNotFindSavedFingerprintException(string message, Exception inner) : base(message, inner) { }
        protected CouldNotFindSavedFingerprintException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
