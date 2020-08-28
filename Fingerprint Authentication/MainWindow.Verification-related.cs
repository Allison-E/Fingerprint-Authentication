using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPFP.Verification;
using DPFP;
using DPFP.Processing;

namespace Fingerprint_Authentication
{
    // Fingerprint verification methods are here
    partial class MainWindow
    {
        Verification verifier;
        Dictionary<Template, string> deserialisedTemplatesFromDB;
        static uint PROBABILITY = 0X80003FE0;
        Task deserialiseFingerprintsFromDBTask;

        private void initialiseVerifier()
        {
            try
            {
                verifier = new Verification((int)PROBABILITY / 100000);
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
                for (int i = 0; i < serialisedFingerprints.Count(); i++)
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

        private async void processVerification(Sample sample)
        {
            FeatureSet feature = ExtractFeatures(sample, DataPurpose.Verification);

            if (feature != null)
            {
                WriteGoodStatus("The fingerprint feature set was created.");
                if (findIDOfMatchingFingerprintAsync(feature) != null)
                    WriteGoodStatus("Match found");
                else
                    WriteErrorStatus("Match not found");
            }
        }

        private async Task<string> findIDOfMatchingFingerprintAsync(FeatureSet feature)
        {
            string resultId = null;
            await deserialiseFingerprintsFromDBTask;

            Task<string> task = Task.Run(() =>
            {
                Verification.Result result;

                for (int i = 0; i < deserialisedTemplatesFromDB.Count(); i++)
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
