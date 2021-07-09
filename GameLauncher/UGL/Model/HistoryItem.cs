using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace UGL.Model
{
    public class HistoryItem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public ImageSource ImageSource { get; set; }

        public static HistoryItem Default { get => new HistoryItem() { Title = "Empty", Author = "Empty", Message = "Empty" }; }
    }
}
