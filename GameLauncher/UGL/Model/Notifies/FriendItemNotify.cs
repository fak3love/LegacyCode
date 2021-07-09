using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UGL.Helpers;
using UGL.Model.Notifies;
using UGL.ViewModel;

namespace UGL.Model
{
    public class FriendItemNotify : BaseUserItemNotify
    {
        public static FriendItemNotify Default { get => new FriendItemNotify() { NickName = "DefaultUser", Status = UserStatuses.Offline }; }
    }
}
