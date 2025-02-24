using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SharedComponents.EVEAccountCreator.Curl
{
    public partial class CaptchaResponseForm : Form
    {
        #region Fields

        private readonly byte[] ImgBytes;

        #endregion Fields

        #region Properties

        public string GetCaptchaResponse => textBox1.Text;

        #endregion Properties

        #region Constructors

        public CaptchaResponseForm()
        {
            InitializeComponent();
        }

        public CaptchaResponseForm(byte[] imgBytes)
        {
            InitializeComponent();
            ImgBytes = imgBytes;
        }

        #endregion Constructors

        #region Methods

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CaptchaResponseForm_Shown(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = (Bitmap) new ImageConverter().ConvertFrom(ImgBytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion Methods
    }
}