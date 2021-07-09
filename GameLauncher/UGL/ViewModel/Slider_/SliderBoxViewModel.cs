using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UGL.Extentions;
using UGL.Helpers;
using UGL.View.Controls;
using UGL.View.Controls.Slider;

namespace UGL.ViewModel.Slider_
{
    public class SliderBoxViewModel : BaseNotify
    {
        private string _serverFiltering;
        private List<SliderItemViewModel> _sliderItemViewModels;
        private Func<string, IEnumerable<SliderItemViewModel>, IEnumerable<ListBoxItem>> _filterFunc;
        private SliderItem _sliderItemSelectedServer;

        public ObservableCollection<ListBoxItem> FilteredServers { get; set; }
        public ObservableCollection<CircleItem> AllCircleServers { get; set; }

        public SliderItem SliderItemSelectedServer
        {
            get => _sliderItemSelectedServer;
            set
            {
                _sliderItemSelectedServer = value;
                OnPropertyChanged();
            }
        }
        public List<SliderItemViewModel> SliderItemViewModels
        {
            get => _sliderItemViewModels;
            set
            {
                _sliderItemViewModels = value;

                AllCircleServers.Clear();
                foreach (var sliderItem in _sliderItemViewModels)
                {
                    var circle = new CircleItem(sliderItem.CircleViewModel);
                    circle.HorizontalAlignment = HorizontalAlignment.Left;
                    circle.Margin = new Thickness(-30, 5, 0, 0);
                    circle.Tag = sliderItem;
                    circle.MouseLeftButtonDown += Circle_MouseLeftButtonDown;
                    circle.MouseEnter += Circle_MouseEnter;
                    circle.MouseLeave += Circle_MouseLeave;

                    AllCircleServers.Add(circle);
                }

                OnPropertyChanged();
            }
        }

        public string ServerFilteringText
        {
            get => _serverFiltering;
            set
            {
                _serverFiltering = value;

                if (string.IsNullOrWhiteSpace(value))
                    ShowAllServers();
                else
                {
                    FilteredServers.Clear();
                    FilteredServers.AddRange(_filterFunc?.Invoke(ServerFilteringText, _sliderItemViewModels));
                }

                OnPropertyChanged();
            }
        }
        public ICommand ShowPanelCommand
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    SliderBox sliderBox = parameter as SliderBox;

                    double fromValue = sliderBox.ActualWidth != double.NaN ? sliderBox.ActualWidth : sliderBox.Width;
                    double toValue = sliderBox.MaxWidth;

                    DoubleAnimation thicknessAnimation = new DoubleAnimation();
                    thicknessAnimation.Duration = TimeSpan.FromSeconds(0.2);
                    thicknessAnimation.From = fromValue;
                    thicknessAnimation.To = toValue;
                    thicknessAnimation.DecelerationRatio = 0.3;
                    sliderBox.BeginAnimation(FrameworkElement.WidthProperty, thicknessAnimation);
                });
            }
        }
        public ICommand HidePanelCommand
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    SliderBox sliderBox = parameter as SliderBox;

                    double fromValue = sliderBox.ActualWidth != double.NaN ? sliderBox.ActualWidth : sliderBox.Width;
                    double toValue = sliderBox.MinWidth;

                    DoubleAnimation thicknessAnimation = new DoubleAnimation();
                    thicknessAnimation.Duration = TimeSpan.FromSeconds(0.3);
                    thicknessAnimation.From = fromValue;
                    thicknessAnimation.To = toValue;
                    thicknessAnimation.DecelerationRatio = 0.3;
                    sliderBox.BeginAnimation(FrameworkElement.WidthProperty, thicknessAnimation);
                });
            }
        }
        public ICommand ServerSelectionChanged
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    SliderItem sliderItem = (parameter as ListBoxItem).Content as SliderItem;

                    _sliderItemSelectedServer.DataContext = sliderItem.DataContext;
                },
                new Func<object, bool>((parameter) => parameter is ListBoxItem listBox && listBox.Content is SliderItem && _sliderItemSelectedServer != null));
            }
        }
        
        public SliderBoxViewModel()
        {
            _sliderItemViewModels = new List<SliderItemViewModel>();
            FilteredServers = new ObservableCollection<ListBoxItem>();
            AllCircleServers = new ObservableCollection<CircleItem>();
        }
        public SliderBoxViewModel(List<SliderItemViewModel> viewModels, Func<string, IEnumerable<SliderItemViewModel>, IEnumerable<ListBoxItem>> filterFunc) : this()
        {
            _sliderItemViewModels = viewModels;
            _filterFunc = filterFunc;
            ShowAllServers();
        }
        public SliderBoxViewModel(List<SliderItemViewModel> viewModels, SliderItem sliderItemMainServer, Func<string, IEnumerable<SliderItemViewModel>, IEnumerable<ListBoxItem>> filterFunc) : this()
        {
            _sliderItemViewModels = viewModels;
            _filterFunc = filterFunc;
            _sliderItemSelectedServer = sliderItemMainServer;
            ShowAllServers();
        }

        private void ShowAllServers()
        {
            FilteredServers.Clear();
            AllCircleServers.Clear();

            foreach (var sliderItemViewModel in _sliderItemViewModels)
            {
                FilteredServers.Add(new ListBoxItem() { Content = new SliderItem(sliderItemViewModel), Style = ResourceHelper.FindListBoxStyleByName("Default") });

                var circle = new CircleItem(sliderItemViewModel.CircleViewModel);
                circle.HorizontalAlignment = HorizontalAlignment.Left;
                circle.Margin = new Thickness(-30, 5, 0, 0);
                circle.Tag = sliderItemViewModel;
                circle.MouseLeftButtonDown += Circle_MouseLeftButtonDown;
                circle.MouseMove += Circle_MouseEnter;
                circle.MouseLeave += Circle_MouseLeave;
                AllCircleServers.Add(circle);
            }
        }

        private void Circle_MouseEnter(object sender, MouseEventArgs e)
        {
            CircleItem circleItem = sender as CircleItem;

            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
            thicknessAnimation.To = new Thickness(-20, 5, 0, 0);
            thicknessAnimation.Duration = TimeSpan.FromSeconds(0.1);
            thicknessAnimation.SpeedRatio = 2;
            circleItem.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
        }
        private void Circle_MouseLeave(object sender, MouseEventArgs e)
        {
            CircleItem circleItem = sender as CircleItem;

            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
            thicknessAnimation.To = new Thickness(-30, 5, 0, 0);
            thicknessAnimation.Duration = TimeSpan.FromSeconds(0.1);
            circleItem.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
        }
        private void Circle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SliderItemSelectedServer.DataContext = (sender as CircleItem).Tag;
        }
    }
}
