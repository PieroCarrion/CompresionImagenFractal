using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CompresionFractal {
    public partial class MainForm : Form {
        Compresion compresion;
        Descompresion descompresion;
        Bitmap imagen = null;
        
        public MainForm() {
            InitializeComponent();
            compresion = new Compresion();
            descompresion = new Descompresion();
            descompresion.repeticiones = 20;
        }
        private void toolStripTextBox1_TextChanged(object sender, EventArgs e) {
            try {
                uint i = uint.Parse(toolStripTextBox1.Text);
                if (i > 1000) { throw new Exception(); }
                descompresion.repeticiones= (int)i;
                toolStripTextBox1.Text = i.ToString();
            } catch {
                toolStripTextBox1.Text = "20";
                descompresion.repeticiones = 20;
            }
        }
        private void compresiónToolStripMenuItem_Click(object sender, EventArgs e) {
            if (imagen != null) {
                try {
                    saveFileDialog1.Filter = "Archivo (*.frc)|*.frc";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK) {

                        ComprSettingsForm comprSettingsForm = new ComprSettingsForm();

                        if (comprSettingsForm.ShowDialog() != DialogResult.OK) return;
                        //Verifica método de comprensión
                        compresion.bool_Cmp2x2 = comprSettingsForm.checkBox1.Checked;
                        compresion.bool_Cmp4x4 = comprSettingsForm.checkBox2.Checked;
                        compresion.bool_Cmp8x8 = comprSettingsForm.checkBox3.Checked;
                        // 
                        compresion.dimensionarImagen(imagen);
                        //
                        if (comprSettingsForm.radioButton1.Checked == true) {
                            compresion.linearCriterion = false;
                        } else {
                            compresion.linearCriterion = true;
                        }

                        ProgressForm progressForm = new ProgressForm(compresion, descompresion);
                        progressForm.ShowDialog();
                        //Guardar imagenn descomprimida
                        FileStream stream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write);
                        BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);
                        compresion.guardarImagenComprimida(writer);
                        stream.Close();
                        //Descomprimir
                        descompresion.SFI = compresion.SFI;
                        descompresion.Descomprimir();
                        Image im = descompresion.ObtenerImagen();
                        //Imprimir
                        ResultForm resultForm = new ResultForm();
                        resultForm.Text = "Resultado de Compresión";
                        resultForm.label1.Image = im;
                        resultForm.Show();
                    }
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Error de compresión");
                }
            } else {
                MessageBox.Show("Seleccione una imagen");
            }
        }

        private void descomprensiónToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                descompresion.Descomprimir();
                Image imagen = descompresion.ObtenerImagen();
                ResultForm resultForm = new ResultForm();
                resultForm.label1.Image= imagen;
                resultForm.Show();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error de descompresión");
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                openFileDialog1.Filter = "Todos los Archivos (*.*)|*.*";
                if (openFileDialog1.ShowDialog() != DialogResult.OK) { return; }
                imagen = new Bitmap(openFileDialog1.FileName, false);
                imagen = new Bitmap(imagen, 512, 512);
                label1.Image = imagen;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error al abrir el archivo");
            }
        }

        private void archivoToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                openFileDialog1.Filter = "Archivos (*.frc)|*.frc";
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                FileStream stream = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);

                descompresion.abrirImagenComprimida(reader);
                stream.Close();
                descompresion.Descomprimir();

                ResultForm resultForm = new ResultForm();
                resultForm.label1.Image = descompresion.ObtenerImagen();
                resultForm.Show();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error al abrir el archivo");
            }
        }
    }
}