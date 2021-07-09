using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using UGL.Helpers;
using UGL.ViewModel;

namespace UGL.Model.Notifies
{
    public class UserItemNotify : BaseUserItemNotify
    {
        public static UserItemNotify Default { get => new UserItemNotify() { NickName = "DefaultUser", Status = UserStatuses.Offline }; }
    }
}
