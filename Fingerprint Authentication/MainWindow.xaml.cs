using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DPFP.Processing;
using DPFP.Capture;

namespace Fingerprint_Authentication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, DPFP.Capture.EventHandler
    {
        private DPFP.Capture.Capture Capturer;
        private Image fingerprintImage;
        private Enrollment enroll;
        private uint noOfScansLeft;

        public MainWindow(string functionToExecute)
        {
            InitializeComponent();
            fingerprintImage = new Image();
            imageBox.DataContext = fingerprintImage;
            Binding bind = new Binding("Picture");
            bind.Source = fingerprintImage;

            imageBox.SetBinding(System.Windows.Controls.Image.SourceProperty, bind);

            switch (functionToExecute.ToLower().Trim())
            {
                case "enroll":
                    break;
                case "verify":
                    break;
                default:
                    break;
            }
        }

        #region Enroll methods
        private void initialiseEnroller()
        {
            try
            {
                enroll = new Enrollment();
                WriteStatus("Enroller initialised");
            }
            catch
            {
                WriteStatus("Error: Could not initialise enroller");
            }
        }

        private void startEnrolling()
        {
            try
            {
                initialiseEnroller();
                WriteStatus("Put a finger on the scanner.");
            }
            catch (Exception)
            {
                WriteStatus("Could not start capturer");
            }

            noOfScansLeft = enroll.FeaturesNeeded;
        }
        #endregion

        #region Fingerprint capture related
        public virtual void InitialiseCapturer()
        {
            try
            {
                Capturer = new DPFP.Capture.Capture();				// Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
                else
                    SetPrompt("Can't initiate capture operation!");
            }
            catch
            {
                MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButton.OK);
            }
        }

        private void DisplayFingerprintImage(DPFP.Sample sample)
        {
            fingerprintImage.Picture = ConvertSampleToBitmap(sample);
        }

        private void StartCapturing()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                    SetPrompt("Using the fingerprint reader, scan your fingerprint.");
                }
                catch
                {
                    SetPrompt("Can't initiate capture!");
                }
            }
        }

        private void StopCapturing()
        {
            if (Capturer != null)
            {
                try
                {
                    Capturer.StopCapture();
                }
                catch
                {
                    SetPrompt("Can't terminate capture!");
                }
            }
        }
        #endregion

        #region Eventhandlers
        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            WriteStatus("The fingerprint sample was captured.");
            SetPrompt("Scan the same fingerprint again.");
            DisplayFingerprintImage(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            WriteStatus("The finger was removed from the fingerprint reader.");
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            WriteStatus("The fingerprint reader was touched.");
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            WriteStatus("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            WriteStatus("The fingerprint reader was disconnected.");
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == CaptureFeedback.Good)
            {
                WriteStatus("Good fingerprint sample");
            }
            else if (CaptureFeedback == CaptureFeedback.LowContrast)
            {
                WriteStatus("The fingerprint sample contrast is low");
            }
            else if (CaptureFeedback == CaptureFeedback.NotEnoughFeatures)
            {
                WriteStatus("Fingerprint sample does not contain enough information");
            }
            else if (CaptureFeedback == CaptureFeedback.NoCentralRegion)
            {
                WriteStatus("The fingerprint sample is not centered");
            }
            else if (CaptureFeedback == CaptureFeedback.NoFinger)
            {
                WriteStatus("The scanned object is not a finger");
            }
            else if (CaptureFeedback == CaptureFeedback.None)
            {
                WriteStatus("No fingerprint received");
            }
            else if (CaptureFeedback == CaptureFeedback.TooDark)
            {
                WriteStatus("Fingerprint sample is too dark");
            }
            else if (CaptureFeedback == CaptureFeedback.TooFast)
            {
                WriteStatus("Finger was swiped too fast");
            }
            else if (CaptureFeedback == CaptureFeedback.TooHigh)
            {
                WriteStatus("Fingerprint sample was too high");
            }
            else if (CaptureFeedback == CaptureFeedback.TooLeft)
            {
                WriteStatus("Fingerprint sample was too close to the left");
            }
            else if (CaptureFeedback == CaptureFeedback.TooLight)
            {
                WriteStatus("Fingerprint sample is too light");
            }
            else if (CaptureFeedback == CaptureFeedback.TooLow)
            {
                WriteStatus("Fingerprint sample was too low");
            }
            else if (CaptureFeedback == CaptureFeedback.TooNoisy)
            {
                WriteStatus("Fingerprint sample is too noisy");
            }
            else if (CaptureFeedback == CaptureFeedback.TooRight)
            {
                WriteStatus("Fingerprint sample was too close to the right");
            }
            else if (CaptureFeedback == CaptureFeedback.TooShort)
            {
                WriteStatus("Fingerprint sample is too short");
            }
            else if (CaptureFeedback == CaptureFeedback.TooSkewed)
            {
                WriteStatus("Fingerprint sample is too skewed");
            }
            else if (CaptureFeedback == CaptureFeedback.TooSlow)
            {
                WriteStatus("Fingerprint was swiped too slowly");
            }
            else if (CaptureFeedback == CaptureFeedback.TooSmall)
            {
                WriteStatus("Size of fingerprint sample is too small");
            }
            else if (CaptureFeedback == CaptureFeedback.TooStrange)
            {
                WriteStatus("Scanned image looks strange");
            }
            else
            {
                WriteStatus("Poor fingerprint sample");
            }
        }
        #endregion

        public DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();  // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);            // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }

        #region Utilities
        public Bitmap ConvertSampleToBitmap(DPFP.Sample sample)
        {
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();  // Create a sample convertor.
            Bitmap bitmap = null;                                                          
            Convertor.ConvertToPicture(sample, ref bitmap);                                
            return bitmap;
        }

        public void WriteStatus(string status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                statusText.Text += "\r\n" + status;
            });
        }

        public void SetPrompt(string prompt)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                promptText.Text = prompt;
            });
        }
        #endregion

        #region Window-events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitialiseCapturer();
            StartCapturing();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopCapturing();
        }
        #endregion
    }
}
