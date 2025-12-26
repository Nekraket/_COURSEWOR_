using System.Windows;
using System.Windows.Controls;

namespace Курсовая.View.UserControls
{
    public partial class StartNavigation : UserControl
    {
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(StartNavigation), new PropertyMetadata(""));

        public static readonly DependencyProperty DescriptionTextProperty = DependencyProperty.Register("DescriptionText", typeof(string), typeof(StartNavigation), new PropertyMetadata(""));

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(string), typeof(StartNavigation), new PropertyMetadata("/Sourse/Images/Default_Image.png"));
        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        public string DescriptionText
        {
            get { return (string)GetValue(DescriptionTextProperty); }
            set { SetValue(DescriptionTextProperty, value); }
        }

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public StartNavigation()
        {
            InitializeComponent();
        }
    }
}
