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
        int user_id;
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
                user_id = Id;
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
            bool wasSuccessful;
            command.CommandText = @"INSERT INTO eCapture_capture (user_id, finger_print)
                                    VALUES (" + user_id + ", @fingerprintParameter)";
            SQLiteParameter fingerprintParameter = new SQLiteParameter("@fingerprintParameter", serialisedFingerprint);
            fingerprintParameter.DbType = DbType.Binary;
            command.Parameters.Add(fingerprintParameter);

            return Task.Run(async () =>
            {
                wasSuccessful = false;

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    try
                    {
                        // Cross-checks to make sure the fingerprint was saved.
                        bool checkResult = await checkIfStorageOfFingerprintWorkedAsync(serialisedFingerprint);
                        if (checkResult)
                            wasSuccessful = true;
                    }
                    catch (CouldNotFindSavedFingerprintsException)
                    {
                        throw new CouldNotStoreFingerprintInDBException("Crosscheck failed. Could not find record in DB after saving it.");
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
                return wasSuccessful;

            });
        }

        public Task<Dictionary<byte[], int>> GetFingerprintsFromDBAsync()
        {
            command.CommandText = @"SELECT user_id, finger_print 
                                    FROM eCapture_capture";
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
                    //throw new CouldNotFindSavedFingerprintsException();

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

        public Task<bool> MarkPresentInAttendance(int userID, int eventID)
        {
            bool wasSuccessful;
            command.CommandText = @"INSERT INTO eCapture_attendance (present, excused, event_id, user_id)
                                    VALUES (@present, @excused, @event_id, @user_id)";
            SQLiteParameter presentParam = new SQLiteParameter("@present", true);
            presentParam.DbType = DbType.Boolean;
            SQLiteParameter excusedParam = new SQLiteParameter("@excused", false);
            presentParam.DbType = DbType.Boolean;
            SQLiteParameter event_idParam = new SQLiteParameter("@event_id", eventID);
            presentParam.DbType = DbType.Int32;
            SQLiteParameter user_idParam = new SQLiteParameter("@user_id", userID);
            presentParam.DbType = DbType.Int32;
            command.Parameters.Add(presentParam);
            command.Parameters.Add(excusedParam);
            command.Parameters.Add(event_idParam);
            command.Parameters.Add(user_idParam);

            return Task.Run(async () =>
            {
                wasSuccessful = false;

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    try
                    {
                        // Cross-checks to make sure the attendance was registered.
                        bool checkResult = await checkIfAttedanceWasRegisteredAsync(userID, eventID);
                        if (checkResult)
                            wasSuccessful = true;
                    }
                    catch (CouldNotFindMarkedAttendance)
                    {
                        throw new CouldNotFindMarkedAttendance("Crosscheck failed. Could not find marked attendance in DB after saving it.");
                    }
                    connection.Close();
                }
                catch
                {
                    throw new CouldNotMarkAttendanceException();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
                return wasSuccessful;
            });
        }

        private int readARow(IDataRecord record, out byte[] serialisedFingerprint)
        {
            serialisedFingerprint = (byte[])record[1];
            return Convert.ToInt32(record[0]);
        }

        private void initialiseSqlStuff()
        {
            command = new SQLiteCommand();
            connectionStringBuilder = new SQLiteConnectionStringBuilder();
            connection = new SQLiteConnection();
            connectionStringBuilder.DataSource = @"C:\Users\MVT1\Desktop\finalyearproject\db.sqlite3";    // Put in the name or network address of the instance of your SQL server here.
            connection.ConnectionString = connectionStringBuilder.ConnectionString;
            command.Connection = connection;
        }

        private Task<bool> checkIfStorageOfFingerprintWorkedAsync(byte[] originalByte)
        {
            command.CommandText = @"SELECT finger_print
                                    FROM eCapture_capture 
                                    WHERE user_id = " + user_id;
            byte[] byteFromDB;

            return Task.Run(() =>
            {
                try
                {
                    byteFromDB = command.ExecuteScalar() as byte[];
                }
                catch
                {
                    throw new CouldNotFindSavedFingerprintsException();
                }

                return originalByte.SequenceEqual(byteFromDB);
            });
        }

        private Task<bool> checkIfAttedanceWasRegisteredAsync(int userID, int eventID)
        {
            command.CommandText = @"SELECT present
                                    FROM eCapture_attendance 
                                    WHERE user_id = " + userID + " AND event_id = " + eventID;
            bool presentReturned;

            return Task.Run(() =>
            {
                try
                {
                    presentReturned = Convert.ToBoolean(command.ExecuteScalar());
                }
                catch
                {
                    throw new CouldNotFindMarkedAttendance();
                }

                return presentReturned;
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
    public class CouldNotFindSavedFingerprintsException : Exception
    {
        public CouldNotFindSavedFingerprintsException() { }
        public CouldNotFindSavedFingerprintsException(string message) : base(message) { }
        public CouldNotFindSavedFingerprintsException(string message, Exception inner) : base(message, inner) { }
        protected CouldNotFindSavedFingerprintsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class CouldNotMarkAttendanceException : Exception
    {
        public CouldNotMarkAttendanceException() { }
        public CouldNotMarkAttendanceException(string message) : base(message) { }
        public CouldNotMarkAttendanceException(string message, Exception inner) : base(message, inner) { }
        protected CouldNotMarkAttendanceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class CouldNotFindMarkedAttendance : Exception
    {
        public CouldNotFindMarkedAttendance() { }
        public CouldNotFindMarkedAttendance(string message) : base(message) { }
        public CouldNotFindMarkedAttendance(string message, Exception inner) : base(message, inner) { }
        protected CouldNotFindMarkedAttendance(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
