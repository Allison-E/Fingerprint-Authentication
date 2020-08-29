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
        private Dictionary<Template, string> deserialisedTemplatesFromDB;
        private const int INT_N = 214748;
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
                WriteErrorStatus("Error: Could not initialise Verifier");
            }
        }

        private async void startVerifying()
        {
            initialiseVerifier();
            deserialisedTemplatesFromDB = new Dictionary<DPFP.Template, string>();
            WriteStatus("Put your finger on the scanner.");
            Dictionary<byte[], string> fingerprintsFromDB = await fingerprintsFromDBTask;
            deserialiseFingerprintsFromDBTask = deserialiseFingerprintsFromDBAsync(fingerprintsFromDB);
            deserialiseFingerprintsFromDBTask.Start();
        }

        private Task deserialiseFingerprintsFromDBAsync(Dictionary<byte[], string> serialisedFingerprints)
        {
            Task task1 = Task.Run(() =>
            {
                // Takes the first pair, deserialises the serialised fingerprint into a Template, and saves the 
                // new Template and ID in a dictionary.
                for (int i = 0; i < serialisedFingerprints.Count; i++)
                {
                    KeyValuePair<byte[], string> pair = serialisedFingerprints.First();
                    Template temp = new DPFP.Template();
                    temp.DeSerialize(pair.Key);

                    deserialisedTemplatesFromDB.Add(temp, pair.Value);
                    serialisedFingerprints.Remove(pair.Key);
                }
            });
            return task1;
        }

        private void processVerification(Sample sample)
        {
            FeatureSet feature = ExtractFeatures(sample, DataPurpose.Verification);

            if (feature != null)
            {
                WriteGoodStatus("The fingerprint feature set was created.");
                string id = findIDOfMatchingFingerprintAsync(feature).Result;
                if (id != null || id != "")
                {
                    WriteGoodStatus("Match found");
                    Application.Current.Shutdown(Convert.ToInt32(id));
                }
                else
                {
                    WriteErrorStatus("Match not found");
                    Application.Current.Shutdown(Convert.ToInt32(id));
                }
            }
        }

        private async Task<string> findIDOfMatchingFingerprintAsync(FeatureSet feature)
        {
            string resultId = null;
            await deserialiseFingerprintsFromDBTask;

            Task<string> task = Task.Run(() =>
            {
                Verification.Result result;

                for (int i = 0; i < deserialisedTemplatesFromDB.Count; i++)
                {
                    result = null;
                    KeyValuePair<Template, string> pair = deserialisedTemplatesFromDB.First();
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
