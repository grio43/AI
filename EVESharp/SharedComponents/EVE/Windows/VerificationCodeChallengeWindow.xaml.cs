using System.Windows;

namespace SharedComponents.EVE.Windows
{
    /// <summary>
    ///     Interaction logic for VerificationCodeChallengeWindow.xaml
    /// </summary>
    public partial class VerificationCodeChallengeWindow : Window
    {
        #region Constructors

        public VerificationCodeChallengeWindow(EveAccount account)
        {
            this.DataContext = this;
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

        public string wtf2 = "wtf2";

        public string VerificationCode { get; set; }

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
            VerificationCode = textVerificationCode.Text;
            if (string.IsNullOrEmpty(VerificationCode))
            {
                MessageBox.Show("Please enter a valid Verification Code to continue logging into this EVE Account!");
                return;
            }

            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}