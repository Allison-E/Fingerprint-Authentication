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
        Verification.Result result;
        Template templateFromDB;
        static uint PROBABILITY = 0X80003FE0;

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

        private void startVerifying(Template templateFromDB)
        {
            this.templateFromDB = templateFromDB;
            initialiseVerifier();
            WriteStatus("Put your finger on the scanner.");
        }

        private void processVerification(Sample sample)
        {
            FeatureSet feature = ExtractFeatures(sample, DataPurpose.Verification);

            if (feature != null)
            {
                WriteGoodStatus("The fingerprint feature set was created.");
                if (areTheSameFingerprints(feature))
                    WriteGoodStatus("Access granted");
                else
                    WriteErrorStatus("Access denied");
            }
        }

        private bool areTheSameFingerprints(FeatureSet feature)
        {
            Verification.Result result = null;
            verifier.Verify(feature, templateFromDB, ref result);

            if (result.Verified)
                return true;
            else
                return false;
        }
    }
}
