using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using DPFP.Verification;
using DPFP;
using DPFP.Processing;

namespace Fingerprint_Authentication
{
    // Fingerprint verification methods are here
    public partial class MainWindow
    {
        private Verification verifier;
        private Dictionary<Template, int> deserialisedTemplatesFromDB;
        private const int INT_N = 214748;   // Indicates the precision of the fingerprint verification.
        private Task deserialiseFingerprintsFromDBTask;

        private void initialiseVerifier()
        {
            try
            {
                verifier = new Verification(INT_N);
                WriteGoodStatus("Verifier initialised.");
            }
            catch
            {
                WriteErrorStatus("Could not initialise Verifier");
            }
        }

        private async void startVerifying()
        {
            initialiseVerifier();
            deserialisedTemplatesFromDB = new Dictionary<Template, int>();
            WriteStatus("Put your finger on the scanner.");
            Dictionary<byte[], int> fingerprintsFromDB = await fingerprintsFromDBTask;
            deserialiseFingerprintsFromDBTask = deserialiseFingerprintsFromDBAsync(fingerprintsFromDB);
        }

        private Task deserialiseFingerprintsFromDBAsync(Dictionary<byte[], int> serialisedFingerprints)
        {
            return Task.Run(() =>
            {
                // Takes the first pair, deserialises the serialised fingerprint into a Template, and saves the 
                // new Template and ID in a dictionary.
                for (int i = 0; i < serialisedFingerprints.Count; i++)
                {
                    KeyValuePair<byte[], int> pair = serialisedFingerprints.First();
                    Template temp = new Template();
                    temp.DeSerialize(pair.Key);

                    deserialisedTemplatesFromDB.Add(temp, pair.Value);
                    serialisedFingerprints.Remove(pair.Key);
                }
            });
        }

        private async void processVerification(Sample sample)
        {
            FeatureSet feature = ExtractFeatures(sample, DataPurpose.Verification);

            if (feature != null)
            {
                WriteGoodStatus("The fingerprint feature set was created.");
                int id = findIDOfMatchingFingerprintAsync(feature).Result;
                if (id >= 0)
                {
                    WriteGoodStatus("Match found");

                    bool storageWasSuccessful = false;
                    try
                    {
                        storageWasSuccessful = await db.MarkPresentInAttendance(Convert.ToInt32(args["userID"]), Convert.ToInt32(args["eventID"]));
                    }
                    catch (DB.CouldNotMarkAttendanceException)
                    {
                        MessageBox.Show("Attendance marking was unsuccessful. Please try again.", "Uh oh :(", MessageBoxButton.OK);
                        Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
                    }

                    if (storageWasSuccessful)
                    {
                        MessageBox.Show("Attendance marking was successful!", "You're in!", MessageBoxButton.OK);
                        Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown(id));
                    }
                    else
                    {
                        MessageBox.Show("Attendance marking was unsuccessful. Please try again.", "Uh oh :(", MessageBoxButton.OK);
                        Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
                    }
                }
                else
                {
                    WriteErrorStatus("Match not found");
                    MessageBox.Show("Sorry, we could not find your fingerprint. Please try again.", "Something bad happened :(", MessageBoxButton.OK);
                    Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown(id));
                }
            }
        }

        private async Task<int> findIDOfMatchingFingerprintAsync(FeatureSet feature)
        {
            int resultId = -100;    // I used a negative number because the IDs would always be positive
            await deserialiseFingerprintsFromDBTask;

            Task<int> task = Task.Run(() =>
            {
                Verification.Result result;

                for (int i = 0; i < deserialisedTemplatesFromDB.Count; i++)
                {
                    result = null;
                    KeyValuePair<Template, int> pair = deserialisedTemplatesFromDB.First();
                    verifier.Verify(feature, pair.Key, ref result);
                    deserialisedTemplatesFromDB.Remove(pair.Key);

                    if (result.Verified)
                        resultId = pair.Value;
                }
                return resultId;
            });

            return task.Result;
        }
    }
}
