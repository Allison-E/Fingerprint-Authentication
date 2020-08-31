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
using System.Windows.Interop;
using System.IO;

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
        private DB.DBHandler db;
        private readonly Dictionary<string, string> args;
        private Task<Dictionary<byte[], int>> fingerprintsFromDBTask;

        /// <summary>
        /// The window for either verification or enrollment of fingerprints
        /// </summary>
        /// <param name="args">A string (either "enroll" or "verify") to tell what is to be done here.</param>
        /// <param name="ID">The ID/key used to store the info of the person who wants to register or verify fingerprints.</param>
        public MainWindow(Dictionary<string, string> arguments)
        {
            InitializeComponent();
            fingerprintImage = new Image();
            imageBox.DataContext = fingerprintImage;
            Binding imageBind = new Binding("Picture");
            imageBind.Mode = BindingMode.OneWay;
            imageBind.Source = fingerprintImage;
            imageBox.SetBinding(System.Windows.Controls.Image.SourceProperty, imageBind);
            db = DB.DBHandler.Instance;

            args = arguments;
            // Check if Dictionary is null and ends app if it's null.
            if (args == null || args.Count == 0)
            {
                MessageBox.Show("Argument is null. Please pass an argument to this application.", "Null argument error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                Application.Current?.Shutdown();
            }
            else
            {
                switch (args["functionToExecute"].ToLower().Trim())
                {
                    case "enroll":
                        Title = "Fingerprint Enrollment";
                        db.SetID(Convert.ToInt32(args["userID"]));
                        startEnrolling();
                        break;
                    case "verify":
                        Title = "Fingerprint Verification";
                        fingerprintsFromDBTask = db.GetFingerprintsFromDBAsync();
                        startVerifying();
                        break;
                }
           }
        }

        #region Fingerprint capture related
        public virtual void InitialiseCapturer()
        {
            try
            {
                Capturer = new Capture();				// Create a capture operation.

                if (Capturer != null)
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
            fingerprintImage.Picture = ConvertSampleToBitmapImage(sample);
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

            switch (args["functionToExecute"].ToLower().Trim())
            {
                case "enroll":
                    processEnrollmentAndSaveToDB(sample);
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
            SetPrompt("Using the fingerprint reader, scan your fingerprint.");
            WriteStatus("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            SetPrompt("Please connect your fingerprint scanner.");
            WriteErrorStatus("The fingerprint reader was disconnected.");
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
        public BitmapImage ConvertSampleToBitmapImage(DPFP.Sample sample)
        {
            int height = 0;
            int width = 0;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                height = (int)imageBox.Height;
                width = (int)imageBox.Width;
            });
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();  // Create a sample converter.
            Bitmap bitmap = new Bitmap(width, height);                                                          
            Convertor.ConvertToPicture(sample, ref bitmap);                                
            return bitmapToBitmapImage(bitmap);
        }

        private BitmapImage bitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public void WriteStatus(string statusMessage)
        {
            Application.Current?.Dispatcher.Invoke(() => statusText.Inlines.Add(statusMessage + "\r\n"));
        }

        public void WriteErrorStatus(string errorMessage)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Run run = new Run(errorMessage + "\r\n");
                run.Foreground = System.Windows.Media.Brushes.Red;
                statusText.Inlines.Add(run);
            });
        }

        public void WriteGoodStatus(string goodMessage)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Run run = new Run(goodMessage + "\r\n");
                run.Foreground = System.Windows.Media.Brushes.Green;
                statusText.Inlines.Add(run);
            });
        }

        public void SetPrompt(string prompt)
        {
            Application.Current?.Dispatcher.Invoke(() =>
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
