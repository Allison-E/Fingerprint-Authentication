using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text;
using DPFP.Processing;
using System.Threading.Tasks;
using DPFP;
using System.Windows;

namespace Fingerprint_Authentication
{
	// Fingerprint enrollment methods are here
	public partial class MainWindow
	{
		private Enrollment enroller;

		private void initialiseEnroller()
		{
			try
			{
				enroller = new Enrollment();
				WriteGoodStatus("Enroller initialised.");
			}
			catch
			{
				WriteErrorStatus("Could not initialise enroller.");
			}
		}

		private void startEnrolling()
		{
			initialiseEnroller();
			noOfScansLeft = enroller.FeaturesNeeded;
			WriteStatus($"Scans left: {noOfScansLeft}.");
			WriteStatus("Put your finger on the scanner.");
		}
		
		private void createFeatureAndAddItToTheEnroller(Sample sample)
		{
			FeatureSet feature = ExtractFeatures(sample, DataPurpose.Enrollment);

			if (feature != null)
			{
				WriteGoodStatus("\r\nThe fingerprint feature set was created.");
				enroller.AddFeatures(feature);
				noOfScansLeft--;
			}

			if (noOfScansLeft != 0)
			{
				WriteStatus($"\nScans left: {noOfScansLeft}.");
				WriteStatus("Put your finger on the fingerprint scanner.");
			}
		}

		private async void processEnrollmentAndSaveToDB(Sample sample)
		{
			createFeatureAndAddItToTheEnroller(sample);

			switch (enroller.TemplateStatus)
			{
				case Enrollment.Status.Ready:   // Report success and stop capturing
					byte[] byteArray = null;
					StopCapturing();
					enroller.Template.Serialize(ref byteArray);

					bool storageWasSuccessful = false;
					try
					{
						storageWasSuccessful = await db.StoreFingerprintInDBAsync(byteArray); ;
					}
					catch (DB.CouldNotStoreFingerprintInDBException)
					{
						MessageBox.Show("Fingerprint enrollment was unsuccessful. Please try again", "Something bad happened :(", MessageBoxButton.OK);
						Application.Current?.Shutdown();
					}

					if (storageWasSuccessful)
					{
						MessageBox.Show("Fingerprint enrollment was successful!", "Success!", MessageBoxButton.OK);
						Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
					}
					else
					{
						MessageBox.Show("Fingerprint enrollment was unsuccessful. Please try again.", "Something bad happened :(", MessageBoxButton.OK);
						Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
					}
					break;

				case Enrollment.Status.Failed:  // Report failure and restart capturing
					enroller.Clear();
					StopCapturing();
					StartCapturing();
					break;
			}
		}
	}
}
