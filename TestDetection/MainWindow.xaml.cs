using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Win32;
using TestDetectionl;
using TestTargetDetectionS;

namespace TestDetection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        string fileName;
        string filter;
        private VideoCapture videoCapture;


        private VideoInfo videoInfo;

        private Timer myTimer;
        private TimeSpan totalTime;
        private TimeSpan currtenTime;
        private bool IsStartPlay;
        public MainWindow()
        {
            InitializeComponent();
            videoInfo = new VideoInfo();
            myTimer = new Timer();
            myTimer.Elapsed += MyTimerElapsed;
        }

        private void BtnLocalFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                RestoreDirectory = true,
                FilterIndex = 1,
                Filter = "视频录像文件(flv,mp4,avi,mkv,264,hevc,mov)|*.flv;*.mp4;*.avi;*.mkv;*.264;*.hevc;*.mov"
            };
           
            bool? showDialog = dlg.ShowDialog();
            if (showDialog != null && showDialog.Value)
            {
                this.tbFilePath.Text = dlg.FileName;
                this.tbFilePath.ToolTip = dlg.FileName;
                this.fileName = dlg.SafeFileName.Split('.')[0];
            }
        }
        private bool StartPlay(string videoPath)
        {
            bool result = true;
            try
            {
                videoCapture = new VideoCapture(videoPath);
                if (videoCapture.IsOpened)
                {
                    videoInfo.Filename = fileName;
                    videoInfo.Width = (int) videoCapture.GetCaptureProperty(CapProp.FrameWidth);
                    videoInfo.Height = (int) videoCapture.GetCaptureProperty(CapProp.FrameHeight);
                    videoInfo.FrameCount = (int) videoCapture.GetCaptureProperty(CapProp.FrameCount);
                    videoInfo.Fps = (int) videoCapture.GetCaptureProperty(CapProp.Fps);
                    videoInfo.CurrentFrame = 0;
                    myTimer.Interval = videoInfo.Fps == 0 ? 300 : 1000 / videoInfo.Fps;
                    IsStartPlay = true;
                    myTimer.Start();
                    totalTime = TimeSpan.FromSeconds(videoInfo.FrameCount / videoInfo.Fps);
                    this.tbTotalTimeLength.Text = $"{totalTime.Hours:00}:{totalTime.Minutes:00}:{totalTime.Seconds:00}";
                    this.process.Minimum = 0;
                    this.process.Maximum = totalTime.TotalSeconds;
                    this.process.SmallChange = 1;
                }
                else
                {
                    MessageBox.Show("视频源异常");
                    result = false;
                }
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }
        private  readonly object LockHelper = new object();
        private void MyTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsStartPlay)
            {
                lock (LockHelper)
                {
                    var frame = videoCapture.QueryFrame();
                    if (frame != null)
                    {
                        videoInfo.CurrentFrame = (int)videoCapture.GetCaptureProperty(CapProp.PosFrames);
                        this.SetVideoCapture(frame);
                        videoInfo.PlaySate = PlaySate.Playing;
                    }
                }
            }
        }

        private void StopPlay()
        {
            IsStartPlay = false;
        }
        private void SetVideoCapture(Mat frame)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                currtenTime = TimeSpan.FromSeconds(videoInfo.CurrentFrame / videoInfo.Fps);
                if (videoInfo.CurrentFrame >= videoInfo.FrameCount)
                {
                    this.process.Value = totalTime.TotalSeconds;
                    this.StopPlay();
                    return;
                }

                var bitImage = frame.Clone();
                this.process.Value = currtenTime.TotalSeconds;
                this.VideoShow.Source = BitmapSourceConvert.ToBitmapSource(bitImage);
            });
        }
        private async void SetFramePos(int value)
        {
            if (videoCapture == null)
                return;
            IsStartPlay = false;
            await Task.Delay(100);
            var result = videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, value);
            if (result)
            {
                videoInfo.CurrentFrame = value;
            }
            var frame = videoCapture.QueryFrame();
            if (frame != null)
            {
                this.SetVideoCapture(frame);
                IsStartPlay = true;
            }

            this.BtnPlay.IsEnabled = true;

        }

        private void process_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var value = (int)this.process.Value * videoInfo.Fps;
            SetFramePos(value);
        }

        private void process_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var value = (int)this.process.Value * videoInfo.Fps;
            SetFramePos(value);
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tbFilePath.Text) || !File.Exists(this.tbFilePath.Text))
            {
                MessageBox.Show("请上传视频文件!");
                return;
            }

            if(videoInfo.PlaySate==PlaySate.None || videoInfo.PlaySate==PlaySate.Stop)
                this.StartPlay(this.tbFilePath.Text);
            if (videoInfo.PlaySate == PlaySate.Pause)
                IsStartPlay = true;
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            StopPlay();
            videoInfo.PlaySate = PlaySate.Pause;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopPlay();
            videoInfo.PlaySate = PlaySate.Stop;
        }

        private void BtnShot_Click(object sender, RoutedEventArgs e)
        {
            var frame = videoCapture.QueryFrame();
            if (frame != null)
            {
                frame.Save("1.jpg");
            }
        }

        private void process_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            IsStartPlay = false;
        }
    }
}
