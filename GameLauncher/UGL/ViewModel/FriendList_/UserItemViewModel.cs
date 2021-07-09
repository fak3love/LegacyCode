using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using UGL.Model.Notifies;
using UGL.View.Controls;

namespace UGL.ViewModel.FriendList_
{
    public class UserItemViewModel : BaseNotify
    {
        private UserItemNotify _userItem;

        public UserItemNotify UserItem
        {
            get => _userItem;
            set
            {
                _userItem = value;
                OnPropertyChanged();
            }
        }

        public UserItemViewModel()
        {
            _userItem = UserItemNotify.Default;
        }
        public UserItemViewModel(UserItemNotify userItem)
        {
            _userItem = userItem;
        }
    }
}
