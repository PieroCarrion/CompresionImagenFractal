using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CompresionFractal {
    public partial class ProgressForm : Form {
        Compresion compresion;
        Descompresion decompression;
        public delegate void ProgressFormMethod();
        //ProgressFormMethod myDelegate;
        public ProgressForm(Compresion c, Descompresion d) {
            InitializeComponent();
            compresion = c;
            decompression = d;
            c.backgroundWorker = backgroundWorker1;
            //myDelegate = new ProgressFormMethod(SetLabelText);
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            compresion.procesarImagen();
            this.Invoke(new ProgressFormMethod(SetLabelText));
            compresion.Comprimir();
        }
        private void ProgressForm_Shown(object sender, EventArgs e) {
            label1.Text = "Preparando...";
            backgroundWorker1.RunWorkerAsync();
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Close();
        }
        private void ProgressForm_FormClosed(object sender, FormClosedEventArgs e) {
            backgroundWorker1.CancelAsync();
        }
        private void SetLabelText() {
            label1.Text = "Comprimiendo...";
        }
    }
}