using SharedComponents.EVE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    public partial class PatternManagerForm : Form
    {

        private EveAccount _eveAccount;

        public PatternManagerForm(EveAccount eA)
        {
            InitializeComponent();
            this._eveAccount = eA;
            this.Text = $"Pattern Manager [{eA.CharacterName}]";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                var res = PatternManager.Instance.GenerateNewPattern(_eveAccount.PatternManagerHoursPerWeek, _eveAccount.PatternManagerDaysOffPerWeek, _eveAccount.PatternManagerExcludedHours);
                textBoxExampleResults.Text = res;
                textBoxExampleResultsFilled.Text = PatternEval.GenerateOutput(res);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void PatternManagerForm_Shown(object sender, EventArgs e)
        {
            this.textBoxDaysOffPerWeek.Text = _eveAccount.PatternManagerDaysOffPerWeek.ToString();
            this.checkBox1.Checked = _eveAccount.PatternManagerEnabled;
            this.textBoxHoursPerWeek.Text = _eveAccount.PatternManagerHoursPerWeek.ToString();

            try
            {
                this.textBoxExcludedHours.Text = string.Join(",", _eveAccount.PatternManagerExcludedHours);
                this.textBoxCurrentPattern.Text = _eveAccount.Pattern;
            }
            catch { }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _eveAccount.PatternManagerEnabled = this.checkBox1.Checked;
        }

        private void textBoxHoursPerWeek_TextChanged(object sender, EventArgs e)
        {
            _eveAccount.PatternManagerHoursPerWeek = Convert.ToInt32(this.textBoxHoursPerWeek.Text);
        }

        private void textBoxDaysOffPerWeek_TextChanged(object sender, EventArgs e)
        {
            _eveAccount.PatternManagerDaysOffPerWeek = Convert.ToInt32(this.textBoxDaysOffPerWeek.Text);
        }


        private void textBoxExcludedHours_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _eveAccount.PatternManagerExcludedHours = this.textBoxExcludedHours.Text.Split(',').Select(n => Convert.ToInt32(n)).ToList();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /**
            if (textBoxExampleResultsFilled.Text.Length == 0)
            {
                Debug.WriteLine("Filled pattern is empty.");
                return;
            }



            var b = (Button)sender;
            Thread t = new Thread(() =>
            {
                b.Invoke(new Action(() => { b.Enabled = false; }));
                try
                {

                    var pattern = string.Empty;

                    textBoxExampleResultsFilled.Invoke(new Action(() =>
                    {
                        pattern = textBoxExampleResultsFilled.Text;
                    }));

                    Debug.WriteLine($"Pattern [{pattern}]");

                    Dictionary<DayOfWeek, int> dayOfWeekMinutes = new Dictionary<DayOfWeek, int>();
                    DateTime dt = DateTime.UtcNow;
                    dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);

                    // Interate over every minute for a week
                    var totalMinutes = 0;
                    for (int i = 0; i < 60 * 24 * 7; i++)
                    {

                        if (PatternEval.IsAnyPatternMatchingDatetime(pattern, dt))
                        {
                            if (!dayOfWeekMinutes.ContainsKey(dt.DayOfWeek))
                                dayOfWeekMinutes[dt.DayOfWeek] = 0;

                            dayOfWeekMinutes[dt.DayOfWeek]++;
                            totalMinutes++;
                        }
                        dt = dt.AddMinutes(1);

                    }
                    var testResultOutput = "";
                    foreach (var d in dayOfWeekMinutes)
                    {
                        testResultOutput += $"[{d.Key}] [{Math.Round((double)d.Value / 60, 2)}h] | ";
                    }

                    Debug.WriteLine($"TotalMinutes {totalMinutes}");
                    testResultOutput += $"[Total] [{Math.Round((double)totalMinutes / 60, 2)}h]";

                    textBoxTestResults.Invoke(new Action(() =>
                    {
                        textBoxTestResults.Text = testResultOutput;
                    }));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    b.Invoke(new Action(() => { b.Enabled = true; }));
                }
            });
            t.Start();
            **/

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this._eveAccount.Pattern = textBoxExampleResults.Text;
            this.textBoxCurrentPattern.Text = _eveAccount.Pattern;
        }

        private void textBoxCurrentPattern_TextChanged(object sender, EventArgs e)
        {
            _eveAccount.Pattern = this.textBoxCurrentPattern.Text;
        }
    }
}
