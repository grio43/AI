using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SharedComponents.EVE.Windows
{
    /// <summary>
    ///     Interaction logic for CharacterChallengeWindow.xaml
    /// </summary>
    public partial class CharacterChallengeWindow : Window
    {
        #region Fields

        private readonly EveAccount Account;

        #endregion Fields

        #region Constructors

        public CharacterChallengeWindow(EveAccount account)
        {
            Account = account;
            InitializeComponent();
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

            if (string.IsNullOrEmpty(Account.CharacterName))
            {
                MessageBox.Show("Please enter a valid Character Name to continue logging into this EVE Account!");
                return;
            }

            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}