using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UGL.Extentions;
using UGL.Helpers;
using UGL.View.Controls;
using UGL.View.Controls.FriendList;
using UGL.View.Controls.History;
using UGL.ViewModel.FriendList_;
using UGL.ViewModel.History_;
using UGL.ViewModel.Slider_;

namespace UGL.ViewModel
{
    public class MainWindowViewModel : BaseNotify
    {
        private SliderControlBoxViewModel _sliderControlBoxViewModel;
        private SliderBoxViewModel _sliderBoxViewModel;
        private HistoryPreviewBoxViewModel _historyPreviewBoxViewModel;
        private UserItemViewModel _userItemViewModel;
        private SettingsPanelViewModel _settingsPanelViewModel;
        private List<HistoryElementViewModel> _historyElementViewModels;
        private List<FriendItemViewModel> _friendItemViewModels;
        private Func<string, IEnumerable<FriendItemViewModel>, IEnumerable<ListBoxItem>> _filterFriendFunc;
        private string _filterFriendText;
        private string _onlineFriendCount = "0";

        public string FilterFriendText
        {
            get => _filterFriendText;
            set
            {
                _filterFriendText = value;

                if (string.IsNullOrWhiteSpace(value))
                    SetAllFriends();
                else
                {
                    FriendList.Clear();
                    FriendList.AddRange(_filterFriendFunc.Invoke(value, FriendItemViewModels));
                }

                OnPropertyChanged();
            }
        }
        public SliderControlBoxViewModel SliderControlBoxViewModel
        {
            get => _sliderControlBoxViewModel;
            set
            {
                _sliderControlBoxViewModel = value;
                OnPropertyChanged();
            }
        }
        public SliderBoxViewModel SliderBoxViewModel
        {
            get => _sliderBoxViewModel;
            set
            {
                _sliderBoxViewModel = value;
                OnPropertyChanged();
            }
        }
        public HistoryPreviewBoxViewModel HistoryPreviewBoxViewModel
        {
            get => _historyPreviewBoxViewModel;
            set
            {
                _historyPreviewBoxViewModel = value;
                OnPropertyChanged();
            }
        }
        public UserItemViewModel UserItemViewModel
        {
            get => _userItemViewModel;
            set
            {
                _userItemViewModel = value;
                OnPropertyChanged();
            }
        }
        public SettingsPanelViewModel SettingsPanelViewModel
        {
            get => _settingsPanelViewModel;
            set
            {
                _settingsPanelViewModel = value;
                OnPropertyChanged();
            }
        }
        public List<HistoryElementViewModel> HistoryElementViewModels
        {
            get => _historyElementViewModels;
            set
            {
                _historyElementViewModels = value;
                OnPropertyChanged();
            }
        }
        public List<FriendItemViewModel> FriendItemViewModels
        {
            get => _friendItemViewModels;
            set
            {
                _friendItemViewModels = value;

                if (string.IsNullOrWhiteSpace(_filterFriendText))
                    SetAllFriends();
                else
                {
                    FriendList.Clear();
                    FriendList.AddRange(_filterFriendFunc.Invoke(_filterFriendText, _friendItemViewModels));
                }

                OnPropertyChanged();
            }
        }

        public List<ListBoxItem> HistoryElements
        {
            get
            {
                if (_historyElementViewModels is null)
                    return new List<ListBoxItem>();

                List<ListBoxItem> historyElements = new List<ListBoxItem>();

                foreach (var viewModel in _historyElementViewModels)
                {
                    historyElements.Add(new ListBoxItem() { Content = new HistoryElement(viewModel), Style = ResourceHelper.FindListBoxStyleByName("Default") });
                }

                return historyElements;
            }
        }
        public ObservableCollection<ListBoxItem> FriendList { get; set; }
        public string OnlineFriendCount
        {
            get => _onlineFriendCount;
            set
            {
                _onlineFriendCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand HistorySelectionChanged
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    HistoryElement historyElement = (parameter as ListBoxItem).Content as HistoryElement;

                    HistoryPreviewBoxViewModel.HistoryItem = (historyElement.DataContext as HistoryElementViewModel).HistoryItem;

                },
                new Func<object, bool>(x => { return x is ListBoxItem boxItem && boxItem.Content is HistoryElement; }));
            }
        }
        public ICommand ShowFriendsPanelCommand
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    Border friendsPanel = parameter as Border;
                    Border bgBlack = (friendsPanel.Parent as Grid).FindName("bgBlack") as Border;
                    bgBlack.Visibility = Visibility.Visible;

                    double fromValue = friendsPanel.ActualWidth != double.NaN ? friendsPanel.ActualWidth : friendsPanel.Width;
                    double toValue = friendsPanel.MaxWidth;

                    DoubleAnimation thicknessAnimation = new DoubleAnimation();
                    thicknessAnimation.Duration = TimeSpan.FromSeconds(0.2);
                    thicknessAnimation.From = fromValue;
                    thicknessAnimation.To = toValue;
                    thicknessAnimation.DecelerationRatio = 0.3;

                    DoubleAnimation opacityAnimation = new DoubleAnimation();
                    opacityAnimation.Duration = TimeSpan.FromSeconds(0.2);
                    opacityAnimation.To = 0.8;

                    friendsPanel.BeginAnimation(FrameworkElement.WidthProperty, thicknessAnimation);
                    bgBlack.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                });
            }
        }
        public ICommand HideFriendsPanelCommand
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    Border friendsPanel = parameter as Border;
                    Border bgBlack = (friendsPanel.Parent as Grid).FindName("bgBlack") as Border;
                    bgBlack.Visibility = Visibility.Hidden;
                    bgBlack.Opacity = 0;

                    double fromValue = friendsPanel.ActualWidth != double.NaN ? friendsPanel.ActualWidth : friendsPanel.Width;
                    double toValue = friendsPanel.MinWidth;

                    DoubleAnimation thicknessAnimation = new DoubleAnimation();
                    thicknessAnimation.Duration = TimeSpan.FromSeconds(0.3);
                    thicknessAnimation.From = fromValue;
                    thicknessAnimation.To = toValue;
                    thicknessAnimation.DecelerationRatio = 0.3;
                    friendsPanel.BeginAnimation(FrameworkElement.WidthProperty, thicknessAnimation);
                });
            }
        }
        public ICommand ExitCommand
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    Application.Current.MainWindow.Close();
                });
            }
        }
        public ICommand LogoutCommand
        {
            get
            {
                return ExitCommand;
            }
        }
        public ICommand OpenSettingsCommand
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    (parameter as SettingsPanel).Visibility = Visibility.Visible;
                });
            }
        }

        public MainWindowViewModel()
        {
            FriendList = new ObservableCollection<ListBoxItem>();
            FriendList.CollectionChanged += FriendList_CollectionChanged;
        }

        public MainWindowViewModel(Func<string, IEnumerable<FriendItemViewModel>, IEnumerable<ListBoxItem>> filterFriendFunc) : this()
        {
            _filterFriendFunc = filterFriendFunc;
        }

        private void SetAllFriends()
        {
            FriendList.Clear();

            foreach (var viewModel in _friendItemViewModels)
            {
                FriendList.Add(new ListBoxItem() { Content = new FriendItem(viewModel), Style = ResourceHelper.FindListBoxStyleByName("Default") });
            }
        }

        private void FriendList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnlineFriendCount = FriendList.Count(x => ((x.Content as FriendItem).DataContext as FriendItemViewModel).FriendItem.Status == Model.UserStatuses.Online).ToString();
        }
    }
}
