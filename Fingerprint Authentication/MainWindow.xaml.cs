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

namespace Fingerprint_Authentication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, DPFP.Capture.EventHandler
    {
        private DPFP.Capture.Capture Capturer;
        private Image fingerprintImage;

        public MainWindow()
        {
            InitializeComponent();
            fingerprintImage = new Image();
            imageBox.DataContext = fingerprintImage;
            Binding bind = new Binding("Picture");
            bind.Source = fingerprintImage;

            imageBox.SetBinding(System.Windows.Controls.Image.SourceProperty, bind);
        }
        
        protected virtual void InitialiseCapturer()
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

        #region Eventhandlers
        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            MakeReport("The fingerprint sample was captured.");
            SetPrompt("Scan the same fingerprint again.");
            DisplayFingerprintImage(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The finger was removed from the fingerprint reader.");
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was touched.");
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was disconnected.");
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
                MakeReport("The quality of the fingerprint sample is good.");
            else
                MakeReport("The quality of the fingerprint sample is poor.");
        }
        #endregion

        public Bitmap ConvertSampleToBitmap(DPFP.Sample sample)
        {
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();  // Create a sample convertor.
            Bitmap bitmap = null;                                                          
            Convertor.ConvertToPicture(sample, ref bitmap);                                
            return bitmap;
        }

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

        public void SetStatus(string status)
        {
            statusText.Text = status;
        }

        public void SetPrompt(string prompt)
        {
            promptText.Text = prompt;
        }

        public void MakeReport(string message)
        {
            statusText.Text += message;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitialiseCapturer();
            StartCapturing();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopCapturing();
        }
    }
}
