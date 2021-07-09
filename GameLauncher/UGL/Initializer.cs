using MaterialDesignColors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UGL.Extentions;
using UGL.Helpers;
using UGL.Model;
using UGL.Model.Notifies;
using UGL.View.Controls.FriendList;
using UGL.View.Controls.Slider;
using UGL.View.FriendList;
using UGL.ViewModel;
using UGL.ViewModel.CircleItem_;
using UGL.ViewModel.FriendList_;
using UGL.ViewModel.History_;
using UGL.ViewModel.Slider_;

namespace UGL
{
    public class Initializer
    {
        public static MainWindowViewModel Initialize()
        {
            UserItemNotify userItemNotify = new UserItemNotify() { Status = UserStatuses.Online, NickName = "Weirdo" };
            List<HistoryElementViewModel> histories = CreateHistoryElementViewModels();
            List<FriendItemViewModel> friends = CreateFriendItemViewModels();

            MainWindowViewModel viewModel = new MainWindowViewModel(CreateDefaultFilterFuncFriendItem());
            viewModel.HistoryPreviewBoxViewModel = CreateHistoryPreviewBoxViewModel(histories.First().HistoryItem);
            viewModel.SliderBoxViewModel = CreateSliderBoxViewModel();
            viewModel.SliderControlBoxViewModel = CreateSliderControlBoxViewModel(viewModel.SliderBoxViewModel);
            viewModel.UserItemViewModel = CreateUserItemViewModel(userItemNotify);
            viewModel.SettingsPanelViewModel = CreateSettingsPanelViewModel();
            viewModel.HistoryElementViewModels = histories;
            viewModel.FriendItemViewModels = friends;

            return viewModel;
        }

        private static SliderBoxViewModel CreateSliderBoxViewModel()
        {
            SliderBoxViewModel sliderBoxViewModel = new SliderBoxViewModel(CreateSliderItemViewModels(), CreateDefaultFilterFunc());

            return sliderBoxViewModel;
        }
        private static SliderControlBoxViewModel CreateSliderControlBoxViewModel(SliderBoxViewModel sliderBoxViewModel)
        {
            SliderControlBoxViewModel sliderControlBoxViewModel = new SliderControlBoxViewModel(sliderBoxViewModel);

            return sliderControlBoxViewModel;
        }
        private static HistoryPreviewBoxViewModel CreateHistoryPreviewBoxViewModel(HistoryItem historyItem)
        {
            HistoryPreviewBoxViewModel viewModel = new HistoryPreviewBoxViewModel(historyItem);

            return viewModel;
        }
        private static UserItemViewModel CreateUserItemViewModel(UserItemNotify userItemNotify)
        {
            UserItemViewModel viewModel = new UserItemViewModel(userItemNotify);

            return viewModel;
        }
        private static SettingsPanelViewModel CreateSettingsPanelViewModel()
        {
            SettingsPanelViewModel viewModel = new SettingsPanelViewModel();

            return viewModel;
        }

        private static List<HistoryElementViewModel> CreateHistoryElementViewModels()
        {
            List<HistoryElementViewModel> historyElementViewModels = new List<HistoryElementViewModel>();
            historyElementViewModels.Add(new HistoryElementViewModel(new HistoryItem() { Title = "Info1", Author = "Author1", Message = "Test v3", ImageSource = new BitmapImage(new Uri(@"\Assets\Images\History\28.png", UriKind.Relative).ToAbsolute()) }));
            historyElementViewModels.Add(new HistoryElementViewModel(new HistoryItem() { Title = "Info2", Author = "Author2", Message = "Test v3", ImageSource = new BitmapImage(new Uri(@"\Assets\Images\History\32.png", UriKind.Relative).ToAbsolute()) }));
            historyElementViewModels.Add(new HistoryElementViewModel(new HistoryItem() { Title = "Info3", Author = "Author3", Message = "Test v3", ImageSource = new BitmapImage(new Uri(@"\Assets\Images\History\7.png", UriKind.Relative).ToAbsolute()) }));

            return historyElementViewModels;
        }
        private static List<SliderItemViewModel> CreateSliderItemViewModels()
        {
            List<SliderItemViewModel> sliderItemViewModels = new List<SliderItemViewModel>();

            sliderItemViewModels.Add(new SliderItemViewModel("4ancient", new CircleServerItemViewModel(ServerStates.PatchPending, new BitmapImage(new Uri(@"\Assets\Images\Servers\4ancient.webp", UriKind.Relative).ToAbsolute()))));
            sliderItemViewModels.Add(new SliderItemViewModel("4Retro", new CircleServerItemViewModel(ServerStates.Offline, new BitmapImage(new Uri(@"\Assets\Images\Servers\4retro.webp", UriKind.Relative).ToAbsolute()))));
            sliderItemViewModels.Add(new SliderItemViewModel("4Retro", new CircleServerItemViewModel(ServerStates.Online, new BitmapImage(new Uri(@"\Assets\Images\Servers\4retro.webp", UriKind.Relative).ToAbsolute()))));
            return sliderItemViewModels;
        }
        private static List<FriendItemViewModel> CreateFriendItemViewModels()
        {
            List<FriendItemViewModel> friends = new List<FriendItemViewModel>();

            friends.Add(new FriendItemViewModel(new FriendItemNotify() { NickName = "DefaultUser1", Status = UserStatuses.Online, LogoSource = new BitmapImage(new Uri(@"\Assets\Images\Users\userLogo1.jpg", UriKind.Relative).ToAbsolute()) }));
            friends.Add(new FriendItemViewModel(new FriendItemNotify() { NickName = "DefaultUser2", Status = UserStatuses.Offline, LogoSource = new BitmapImage(new Uri(@"\Assets\Images\Users\userLogo2.jpg", UriKind.Relative).ToAbsolute()) }));
            friends.Add(new FriendItemViewModel(new FriendItemNotify() { NickName = "DefaultUser3", Status = UserStatuses.Away, LogoSource = new BitmapImage(new Uri(@"\Assets\Images\Users\userLogo3.jpg", UriKind.Relative).ToAbsolute()) }));
            friends.Add(new FriendItemViewModel(new FriendItemNotify() { NickName = "DefaultUser4", Status = UserStatuses.Busy, LogoSource = new BitmapImage(new Uri(@"\Assets\Images\Users\userLogo4.jpg", UriKind.Relative).ToAbsolute()) }));
            friends.Add(new FriendItemViewModel(new FriendItemNotify() { NickName = "DefaultUser5", Status = UserStatuses.Busy, LogoSource = new BitmapImage(new Uri(@"\Assets\Images\Users\userLogo5.jpg", UriKind.Relative).ToAbsolute()) }));

            return friends;
        }
        private static Func<string, IEnumerable<SliderItemViewModel>, IEnumerable<ListBoxItem>> CreateDefaultFilterFunc()
        {
            return new Func<string, IEnumerable<SliderItemViewModel>, IEnumerable<ListBoxItem>>((x, y) =>
            {
                List<ListBoxItem> sliderItems = new List<ListBoxItem>();
                foreach (var item in y)
                {
                    if (item.ServerName.StartsWith(x))
                        sliderItems.Add(new ListBoxItem() { Content = new SliderItem(item), Style = ResourceHelper.FindListBoxStyleByName("Default") });
                }

                return sliderItems;
            });
        }

        private static Func<string, IEnumerable<FriendItemViewModel>, IEnumerable<ListBoxItem>> CreateDefaultFilterFuncFriendItem()
        {
            return new Func<string, IEnumerable<FriendItemViewModel>, IEnumerable<ListBoxItem>>((x, y) =>
            {
                List<ListBoxItem> sliderItems = new List<ListBoxItem>();
                foreach (var item in y)
                {
                    if (item.FriendItem.NickName.StartsWith(x))
                        sliderItems.Add(new ListBoxItem() { Content = new FriendItem(item), Style = ResourceHelper.FindListBoxStyleByName("Default") });
                }

                return sliderItems;
            });
        }
    }
}
