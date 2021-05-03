using System;
using System.Windows.Forms;

namespace CompresionFractal {
    public partial class ComprSettingsForm : Form {
        public ComprSettingsForm() {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e) {
            if (checkBox1.Checked == false && checkBox2.Checked == false && checkBox3.Checked == false) {
                MessageBox.Show("Debe seleccionar al menos un método de comparación", " Error");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        private void button2_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}