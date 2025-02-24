using System.Windows;

namespace SharedComponents.EVE.Windows
{
    /// <summary>
    ///     Interaction logic for EVEEULAWindow.xaml
    /// </summary>
    public partial class EVEEULAWindow : Window
    {
        #region Constructors

        public EVEEULAWindow(string EULABody)
        {
            InitializeComponent();

            int startIndex = EULABody.IndexOf("<div class=\"eula\">");
            int endIndex = EULABody.IndexOf("<div class=\"submit\">");

            string header = @"<head>
    <meta charset=""utf-8"" />
    <title>License Agreement Update</title>
</head>
";
            string eulaOnly = EULABody.Substring(startIndex, endIndex - startIndex);

            webBrowser.NavigateToString(header + eulaOnly);
        }

        #endregion Constructors

        #region Methods

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            DialogResult = false;
            Close();
        }

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}