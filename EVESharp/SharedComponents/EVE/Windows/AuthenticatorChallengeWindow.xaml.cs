using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SharedComponents.EVE.Windows
{
    /// <summary>
    ///     Interaction logic for AuthenticatorChallengeWindow.xaml
    /// </summary>
    public partial class AuthenticatorChallengeWindow : Window
    {
        #region Constructors

        public AuthenticatorChallengeWindow(EveAccount account)
        {
            Account = account;
            InitializeComponent();
        }

        #endregion Constructors

        #region Fields

        private readonly EveAccount Account;

        #endregion Fields

        #region Properties

        public string AccountName
        {
            get { return Account.AccountName; }
            set { }
        }

        public string AuthenticatorCode { get; set; }

        #endregion Properties

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
            AuthenticatorCode = textAuthenticatorCode.Text;
            if (string.IsNullOrEmpty(AuthenticatorCode))
            {
                MessageBox.Show("Please enter a valid Authenticator Code to continue logging into this EVE Account!");
                return;
            }

            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}