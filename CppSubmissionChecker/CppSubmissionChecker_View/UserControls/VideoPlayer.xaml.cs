using CppSubmissionChecker_ViewModel.Viewmodels.FilePreview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        MediaFile_VM? _viewmodel;
        private bool _abortTimerRequested = false;
        public VideoPlayer()
        {
            InitializeComponent();
            this.DataContextChanged += VideoPlayer_DataContextChanged;
            this.IsVisibleChanged += VideoPlayer_IsVisibleChanged;
        }

        private void VideoPlayer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!e.NewValue.Equals(true))
            {
                if (_viewmodel != null) _viewmodel.IsPlaying = false;
                this.DataContext = null;
            }

        }

        private void VideoPlayer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnRegisterEvents();
            _viewmodel = e.NewValue as MediaFile_VM;
            RegisterEvents();
            if (_viewmodel != null)
                _viewmodel.IsPlaying = true;
        }

        void RegisterEvents()
        {
            if (_viewmodel == null) return;
            _viewmodel.PropertyChanged += _viewmodel_PropertyChanged;
        }



        void UnRegisterEvents()
        {
            if (_viewmodel == null) return;
            _viewmodel.PropertyChanged -= _viewmodel_PropertyChanged;
            _viewmodel = null;
        }

        private void _viewmodel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (_viewmodel == null) return;
            if (sender != _viewmodel) return;

            switch (e.PropertyName)
            {
                case nameof(_viewmodel.IsPlaying):
                    if (_viewmodel.IsPlaying)
                    {
                 
                        myMediaElement.Play();
                        myMediaElement.Position = TimeSpan.FromMilliseconds(_viewmodel.CurrentMilliseconds);
                        _abortTimerRequested = false;
                        Task.Run(UpdateTimer);
                    }
                    else
                    {
                        myMediaElement.Pause();
                        _abortTimerRequested = true;
                    }
                    break;
                case nameof(_viewmodel.CurrentMilliseconds):
                    double diffcurrent = Math.Abs(myMediaElement.Position.TotalMilliseconds - _viewmodel.CurrentMilliseconds);
                    if (diffcurrent > 500)
                    {
                        myMediaElement.Position = TimeSpan.FromMilliseconds(_viewmodel.CurrentMilliseconds);
                    }
                    break;

            }
        }

        async Task UpdateTimer()
        {
            while (_viewmodel != null && _viewmodel.IsPlaying && !_abortTimerRequested)
            {
                await Task.Delay(100);
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (_viewmodel != null)
                        {
                            _viewmodel.CurrentMilliseconds = (int)myMediaElement.Position.TotalMilliseconds;

                        }
                    });

                }
                catch (Exception e)
                {

                }
            }

        }


        // Change the volume of the media.
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            myMediaElement.Volume = (double)volumeSlider.Value;
        }

        // Change the speed of the media.
        //private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        //{
        //    myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        //}

        // When the media opens, initialize the "Seek To" slider maximum value
        // to the total number of miliseconds in the length of the media clip.
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            if (_viewmodel != null)
            {
                if (myMediaElement.NaturalDuration.HasTimeSpan)
                {
                    _viewmodel.TotalMilliseconds = (int)Math.Ceiling(myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                }
                else
                {
                    _viewmodel.TotalMilliseconds = 300000;
                }
            
                _viewmodel.CurrentMilliseconds = 0;
                _viewmodel.IsPlaying = true;
            }
        }

        // When the media playback is finished. Stop() the media to seek to media start.
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Stop();
        }

        // Jump to different parts of the media (seek to).
        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            int SliderValue = (int)timelineSlider.Value;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, milliseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            myMediaElement.Position = ts;
        }

        void InitializePropertyValues()
        {
            // Set the media's starting Volume and SpeedRatio to the current value of the
            // their respective slider controls.
            myMediaElement.Volume = (double)volumeSlider.Value;
            //myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        }

        private void timelineSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int SliderValue = (int)timelineSlider.Value;
            if (_viewmodel != null)
            {
                _viewmodel.CurrentMilliseconds = SliderValue;
                _abortTimerRequested = true;
                myMediaElement.Pause();
            }
        }
        private void timelineSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewmodel == null) return;
            if (_viewmodel.IsPlaying)
            {
                int SliderValue = (int)timelineSlider.Value;
                _viewmodel.CurrentMilliseconds = SliderValue;

                myMediaElement.Play();
                _abortTimerRequested = false;
                Task.Run(UpdateTimer);
            }
        }
    }
}
