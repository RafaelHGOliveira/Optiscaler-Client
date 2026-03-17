using System.Windows;

namespace OptiscalerManager
{
    public partial class GuideWindow : Window
    {
        public GuideWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
