using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BitPatternLogic
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        TextBox[] bits1 = new TextBox[16];
        TextBox[] bits2 = new TextBox[16];
        ComboBox opBox;
        Label resultLabel;
        Button calcButton;

        public MainForm()
        {
            Text = "RetroHack SimpleLogic and Conversion";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            UpdateResult();
        }

        void InitializeComponent()
        {
            var table = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 16,
                AutoSize = true,
                Location = new Point(10, 10),
            };
            for (int c = 0; c < 16; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            for (int i = 0; i < 16; i++)
            {
                var t1 = NewBitBox();
                t1.TabIndex = i;
                t1.TextChanged += (s, e) => UpdateResult();
                bits1[i] = t1;
                table.Controls.Add(t1, i, 0);

                var t2 = NewBitBox();
                t2.TabIndex = i + 16;
                t2.TextChanged += (s, e) => UpdateResult();
                bits2[i] = t2;
                table.Controls.Add(t2, i, 1);
            }

            opBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "AND", "OR", "XOR", "NOT" },
                SelectedIndex = 0,
                Location = new Point(10, table.Bottom + 10),
                TabIndex = 32
            };
            opBox.SelectedIndexChanged += (s, e) => UpdateResult();

            resultLabel = new Label
            {
                AutoSize = false,
                BorderStyle = BorderStyle.Fixed3D,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 16 * 25,
                Height = 25,
                Location = new Point(opBox.Right + 10, table.Bottom + 10),
                TabStop = false
            };

            calcButton = new Button
            {
                Text = "Open 16‑Bit Converter",
                AutoSize = true,
                Location = new Point(10, opBox.Bottom + 10),
                TabIndex = 33
            };
            calcButton.Click += (s, e) => new CalculatorForm().Show();

            Controls.AddRange(new Control[] { table, opBox, resultLabel, calcButton });
        }

        TextBox NewBitBox() =>
            new TextBox
            {
                Width = 25,
                MaxLength = 1,
                TextAlign = HorizontalAlignment.Center,
                TabStop = true
            };

        void UpdateResult()
        {
            int[] a = bits1.Select(t => t.Text == "1" ? 1 : 0).ToArray();
            int[] b = bits2.Select(t => t.Text == "1" ? 1 : 0).ToArray();
            int[] r = new int[16];
            string op = opBox.SelectedItem.ToString();

            for (int i = 0; i < 16; i++)
            {
                switch (op)
                {
                    case "AND": r[i] = a[i] & b[i]; break;
                    case "OR": r[i] = a[i] | b[i]; break;
                    case "XOR": r[i] = a[i] ^ b[i]; break;
                    case "NOT": r[i] = 1 - a[i]; break;
                }
            }

            resultLabel.Text = string.Concat(r.Select(bit => bit.ToString()));
        }
    }

    public class CalculatorForm : Form
    {
        TextBox txtDec, txtHex, txtOct, txtBin;
        Button btnConvert;
        TextBox activeBox;

        public CalculatorForm()
        {
            Text = "16‑Bit Base Converter";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
        }

        void InitializeComponent()
        {
            int y = 10;

            txtDec = AddRow("Decimal (0–65535):", y, 0);
            txtDec.MaxLength = 5;
            y += 30;

            txtHex = AddRow("Hex (0000–FFFF):", y, 1);
            txtHex.MaxLength = 4;
            y += 30;

            txtOct = AddRow("Octal (000000–177777):", y, 2);
            txtOct.MaxLength = 6;
            y += 30;

            txtBin = AddRow("Binary (16‑bit):", y, 3);
            txtBin.MaxLength = 16;
            y += 30;

            btnConvert = new Button
            {
                Text = "Convert",
                AutoSize = true,
                Location = new Point(150, y),
                TabIndex = 4
            };
            btnConvert.Click += BtnConvert_Click;
            Controls.Add(btnConvert);
        }

        TextBox AddRow(string labelText, int y, int tabIndex)
        {
            var lbl = new Label
            {
                Text = labelText,
                AutoSize = true,
                Location = new Point(10, y + 3),
                TabStop = false
            };
            var tb = new TextBox
            {
                Width = 120,
                Location = new Point(150, y),
                TabIndex = tabIndex
            };
            // track last-focused box
            tb.GotFocus += (s, e) => activeBox = tb;

            Controls.Add(lbl);
            Controls.Add(tb);
            return tb;
        }

        void BtnConvert_Click(object sender, EventArgs e)
        {
            // if nothing has been focused yet, default to decimal
            if (activeBox == null)
                activeBox = txtDec;

            int @base = activeBox == txtHex ? 16
                       : activeBox == txtOct ? 8
                       : activeBox == txtBin ? 2
                       : 10;

            int val;
            try
            {
                string text = activeBox.Text.Trim();
                val = string.IsNullOrEmpty(text)
                    ? 0
                    : ParseBase(text, @base);
            }
            catch
            {
                MessageBox.Show("Invalid number format.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                activeBox.Focus();
                return;
            }

            val &= 0xFFFF;

            txtDec.Text = val.ToString();
            txtHex.Text = val.ToString("X4");
            txtOct.Text = Convert.ToString(val, 8).PadLeft(6, '0');
            txtBin.Text = Convert.ToString(val, 2).PadLeft(16, '0');

            // restore focus
            activeBox.Focus();
            activeBox.SelectionStart = activeBox.Text.Length;
        }

        int ParseBase(string s, int b)
        {
            s = s.Trim();
            if (b == 16 && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2);
            return Convert.ToInt32(s, b);
        }
    }
}
