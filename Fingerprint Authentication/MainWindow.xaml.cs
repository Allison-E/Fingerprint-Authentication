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
        private Capture Capturer;
        private Image fingerprintImage;
        private uint noOfScansLeft;
        string _functionToExecute;
        /// <summary>
        /// The ID/key for storage and retrieval of the fingerprint templates from the DB.
        /// </summary>
        string Id;
        Dictionary<string, string> args;
        DB.DBHandler db;

        /// <summary>
        /// The window for either verification or enrollment of fingerprints
        /// </summary>
        /// <param name="args">A string (either "enroll" or "verify") to tell what is to be done here.</param>
        /// <param name="ID">The ID/key used to store the info of the person who wants to register or verify fingerprints.</param>
        public MainWindow(Dictionary<string, string> arguments)
        {
            InitializeComponent();
            args = new Dictionary<string, string>();
            args = arguments;
            // Check if Dictionary is null and ends app if it's null.
            if (args == null)
            {
                MessageBox.Show("Argument is null. Please pass an argument to this application.", "Null argument error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                Application.Current.Shutdown();
            }
            db = DB.DBHandler.Instance;

            fingerprintImage = new Image();
            imageBox.DataContext = fingerprintImage;
            Binding bind = new Binding("Picture");
            bind.Source = fingerprintImage;
            imageBox.SetBinding(System.Windows.Controls.Image.SourceProperty, bind);
            
            switch (args["functionToExecute"].ToLower().Trim())
            {
                case "enroll":
                    db.SetID(args["ID"]);
                    startEnrolling();
                    break;
                case "verify":
                    startVerifying();
                    break;
                default:
                    break;
            }
        }

        #region Fingerprint capture related
        public virtual void InitialiseCapturer()
        {
            try
            {
                Capturer = new Capture();				// Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
                else
                    WriteErrorStatus("Capturer is null");
            }
            catch
            {
                WriteErrorStatus("Can't initiate capture operation!");
            }
        }

        private void DisplayFingerprintImage(DPFP.Sample sample)
        {
            fingerprintImage.Picture = ConvertSampleToBitmap(sample);
        }

        private void StartCapturing()
        {
            if (Capturer != null)
            {
                try
                {
                    Capturer.StartCapture();
                    SetPrompt("Using the fingerprint reader, scan your fingerprint.");
                }
                catch
                {
                    WriteErrorStatus("Can't start capturing!");
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
                    WriteErrorStatus("Can't terminate capture!");
                }
            }
        }
        #endregion

        #region Eventhandlers
        public void OnComplete(object capture, string readerSerialNumber, DPFP.Sample sample)
        {
            WriteStatus("The fingerprint sample was captured.");
            DisplayFingerprintImage(sample);

            switch (_functionToExecute.ToLower().Trim())
            {
                case "enroll":
                    processEnrollment(sample);
                    break;
                case "verify":
                    processVerification(sample);
                    break;
                default:
                    break;
            }
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
                WriteErrorStatus("The fingerprint sample contrast is low");
            }
            else if (CaptureFeedback == CaptureFeedback.NotEnoughFeatures)
            {
                WriteErrorStatus("Fingerprint sample does not contain enough information");
            }
            else if (CaptureFeedback == CaptureFeedback.NoCentralRegion)
            {
                WriteErrorStatus("The fingerprint sample is not centered");
            }
            else if (CaptureFeedback == CaptureFeedback.NoFinger)
            {
                WriteErrorStatus("The scanned object is not a finger");
            }
            else if (CaptureFeedback == CaptureFeedback.None)
            {
                WriteErrorStatus("No fingerprint received");
            }
            else if (CaptureFeedback == CaptureFeedback.TooDark)
            {
                WriteErrorStatus("Fingerprint sample is too dark");
            }
            else if (CaptureFeedback == CaptureFeedback.TooFast)
            {
                WriteErrorStatus("Finger was swiped too fast");
            }
            else if (CaptureFeedback == CaptureFeedback.TooHigh)
            {
                WriteErrorStatus("Fingerprint sample was too high");
            }
            else if (CaptureFeedback == CaptureFeedback.TooLeft)
            {
                WriteErrorStatus("Fingerprint sample was too close to the left");
            }
            else if (CaptureFeedback == CaptureFeedback.TooLight)
            {
                WriteErrorStatus("Fingerprint sample is too light");
            }
            else if (CaptureFeedback == CaptureFeedback.TooLow)
            {
                WriteErrorStatus("Fingerprint sample was too low");
            }
            else if (CaptureFeedback == CaptureFeedback.TooNoisy)
            {
                WriteErrorStatus("Fingerprint sample is too noisy");
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
                WriteErrorStatus("Scanned image looks strange");
            }
            else
            {
                WriteErrorStatus("Poor fingerprint sample");
            }
        }
        #endregion

        public DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();
            CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features); 
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }

        #region Utilities
        public Bitmap ConvertSampleToBitmap(DPFP.Sample sample)
        {
            int height = 0;
            int width = 0;
            Application.Current.Dispatcher.Invoke(() =>
            {
                height = (int)imageBox.Height;
                width = (int)imageBox.Width;
            });
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();  // Create a sample convertor.
            Bitmap bitmap = new Bitmap(width, height);                                                          
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

        public void WriteErrorStatus(string errorMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                statusText.Foreground = System.Windows.Media.Brushes.Red;
                statusText.Text += "\r\n" + errorMessage;
                statusText.Foreground = System.Windows.Media.Brushes.Black;
            });
        }

        public void WriteGoodStatus(string errorMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                statusText.Foreground = System.Windows.Media.Brushes.Green;
                statusText.Text += "\r\n" + errorMessage;
                statusText.Foreground = System.Windows.Media.Brushes.Black;
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
