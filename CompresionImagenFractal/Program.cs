using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.ComponentModel;

namespace CompresionFractal {
    // 8 tipos 
    // 4 primeros son los cuadrantes de un plano i->j 
    // El resto son su inversa j->i
    public enum tipoTransformacion { Deg0, Deg90, Deg180, Deg270, Deg0Sim, Deg90Sim, Deg180Sim, Deg270Sim }
    public enum metodoComparacion { Cmp2x2, Cmp4x4, Cmp8x8 }
    static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
    public class funcionIterativa {
        public int iRan, jRan;
        public tipoTransformacion tipo;
        public float q;
        public funcionIterativa(int iRan, int jRan, tipoTransformacion tipo, float q) {
            this.iRan = iRan;
            this.jRan = jRan;
            this.tipo = tipo;
            this.q = q;
        }
    }
    public class Compresion {
        //Imagen en diferentes dimensiones
        Bitmap image512, image256, image128, image64;
        //Lienzos de las imágenes para manipular
        float[,] im512, im256, im128, im64;

        float[,] promBrillRan8x8, promBrillDom8x8, promBrillRan4x4, promBrillDom4x4, promBrillRan2x2, promBrillDom2x2;

        const int iRanMax = 32, jRanMax = 32, iDomMax = 64, jDomMax = 64;
        //Sistema de funciones iterativas 
        public funcionIterativa[] SFI;
        // 8x8, 4x4, 2x2
        Array tiposTransformacion;

        public bool bool_Cmp2x2, bool_Cmp4x4, bool_Cmp8x8, linearCriterion;
        public BackgroundWorker backgroundWorker;


        public void dimensionarImagen(Bitmap imagen) {
            image512 = new Bitmap(imagen, 512, 512);
            image256 = new Bitmap(imagen, 256, 256);
            image128 = new Bitmap(imagen, 128, 128);
            image64 = new Bitmap(imagen, 64, 64);
        }
        public void procesarImagen() {
            im512 = new float[512, 512];
            im256 = new float[256, 256];
            im128 = new float[128, 128];
            im64 = new float[64, 64];

            promBrillRan8x8 = new float[iRanMax, jRanMax];
            promBrillDom8x8 = new float[iDomMax, jDomMax];

            promBrillRan4x4 = new float[iRanMax, jRanMax];
            promBrillDom4x4 = new float[iDomMax, jDomMax];

            promBrillRan2x2 = new float[iRanMax, jRanMax];
            promBrillDom2x2 = new float[iDomMax, jDomMax];

            tiposTransformacion = Enum.GetValues(typeof(tipoTransformacion));

            // Asigna brillo a los lienzos 512p, 256p, 128p, 64p
            for (int x = 0; x < image512.Width; x++)
                for (int y = 0; y < image512.Height; y++)
                    im512[x, y] = (image512.GetPixel(x, y).GetBrightness() * 255);

            for (int x = 0; x < image256.Width; x++)
                for (int y = 0; y < image256.Height; y++)
                    im256[x, y] = (image256.GetPixel(x, y).GetBrightness() * 255);

            for (int x = 0; x < image128.Width; x++)
                for (int y = 0; y < image128.Height; y++)
                    im128[x, y] = (image128.GetPixel(x, y).GetBrightness() * 255);

            for (int x = 0; x < image64.Width; x++)
                for (int y = 0; y < image64.Height; y++)
                    im64[x, y] = (image64.GetPixel(x, y).GetBrightness() * 255) ;

            // Brillo promedio de las áreas de Rango
            for (int i = 0; i < iRanMax; i++) {
                for (int j = 0; j < jRanMax; j++) {
                    float sum = 0;
                    //Suma el brillo de todos los pixeles en un area de 8x8p 
                    for (int x = i * 8; x < (i + 1) * 8; x++)
                        for (int y = j * 8; y < (j + 1) * 8; y++) {
                            sum += (im512[x, y]) / 4.0f;
                            //Asigna a cada pixel el promedio de dicha suma
                            promBrillRan8x8[i, j] = sum / 64.0f;
                        }
                    //Suma el brillo de todos los pixeles en un area de 4x4p
                    sum = 0;
                    for (int x = i * 4; x < (i + 1) * 4; x++)
                        for (int y = j * 4; y < (j + 1) * 4; y++) {
                            sum += im128[x, y];
                            promBrillRan4x4[i, j] = sum / 16.0f;
                        }
                    //Suma el brillo de todos los pixeles en un area de 2x2p
                    sum = 0;
                    for (int x = i * 2; x < (i + 1) * 2; x++)
                        for (int y = j * 2; y < (j + 1) * 2; y++) {
                            sum += im64[x, y];
                            promBrillRan2x2[i, j] = sum / 4.0f;
                        }
                }
            }
            // El Brillo promedio de las áreas de dominio
            for (int i = 0; i < iDomMax; i++) {
                for (int j = 0; j < jDomMax; j++) {
                    float sum = 0;
                    //Suma el brillo de todos los pixeles en un area de 8x8p 
                    for (int x = i * 8; x < (i + 1) * 8; x++)
                        for (int y = j * 8; y < (j + 1) * 8; y++)
                            sum += im512[x, y];
                    //Asigna a cada pixel el promedio de dicha suma
                    promBrillDom8x8[i, j] = sum / 64.0f;

                    //Suma el brillo de todos los pixeles en un area de 4x4p 
                    sum = 0;
                    for (int x = i * 4; x < (i + 1) * 4; x++)
                        for (int y = j * 4; y < (j + 1) * 4; y++)
                            sum += im256[x, y];
                    promBrillDom4x4[i, j] = sum / 16.0f;

                    //Suma el brillo de todos los pixeles en un area de 2x2p 
                    sum = 0;
                    for (int x = i * 2; x < (i + 1) * 2; x++)
                        for (int y = j * 2; y < (j + 1) * 2; y++)
                            sum += im128[x, y];
                    promBrillDom2x2[i, j] = sum / 4.0f;
                }
            }
        }
        public void Comprimir() {
            ArrayList funcionIterativaRan = new ArrayList();
            //Recorrido de cada dominio de la imagen
            for (int iDom = 0; iDom < iDomMax; iDom++) {
                for (int jDom = 0; jDom < jDomMax; jDom++) {
                    float min2x2, min4x4, min8x8, qBest = 0;
                    int iBestRan = 0, jBestRan = 0;
                    tipoTransformacion tBest;
                    min2x2 = min4x4 = min8x8 = float.MaxValue;
                    tBest = tipoTransformacion.Deg0;
                    /*Compara todos los rangos posibles con el dominio en cuestión, para encontrar las similitudes y luego guardarlas */
                    for (int iRan = 0; iRan < iRanMax; iRan++) {
                        for (int jRan = 0; jRan < jRanMax; jRan++) {
                            //El brillo (blanco <-> negro) más cercano entre el dominio y el rango
                            float q8x8 = (float)Math.Round(promBrillDom8x8[iDom, jDom] - 0.75 * promBrillRan8x8[iRan, jRan]);
                            float q4x4 = (float)Math.Round(promBrillDom4x4[iDom, jDom] - 0.75 * promBrillRan4x4[iRan, jRan]);
                            float q2x2 = (float)Math.Round(promBrillDom2x2[iDom, jDom] - 0.75 * promBrillRan2x2[iRan, jRan]);
                            //
                            //Recorre cada Rango en todas sus transformaciones (0°,90°, etc) para encontrar el más parecido al dominio
                            foreach (tipoTransformacion type in tiposTransformacion) {
                                float cur2x2, cur4x4, cur8x8;
                                cur2x2 = cur4x4 = cur8x8 = float.MaxValue;
                                //Compara todos los cuadros para encontrar el más adecuado
                                if (bool_Cmp2x2) {
                                    cur2x2 = d(iDom, jDom, iRan, jRan, type, metodoComparacion.Cmp2x2, q2x2);
                                    if (cur2x2 >= min2x2 ) { continue; }
                                }
                                if (bool_Cmp4x4) {
                                    cur4x4 = d(iDom, jDom, iRan, jRan, type, metodoComparacion.Cmp4x4, q4x4);
                                    if (cur4x4 >= min4x4) { continue; }
                                }
                                if (bool_Cmp8x8) {
                                    cur8x8 = d(iDom, jDom, iRan, jRan, type, metodoComparacion.Cmp8x8, q8x8);
                                    if (cur8x8 >= min8x8) { continue; }
                                }
                                iBestRan = iRan;
                                jBestRan = jRan;
                                tBest = type;

                                if (bool_Cmp8x8) qBest = q8x8;
                                else if (bool_Cmp4x4) qBest = q4x4;
                                else if (bool_Cmp2x2) qBest = q2x2;

                                min2x2 = cur2x2;
                                min4x4 = cur4x4;
                                min8x8 = cur8x8;
                            }
                        }
                    }
                    //Almacena los rangos encontrados
                    funcionIterativaRan.Add(new funcionIterativa(iBestRan, jBestRan, tBest, qBest));
                }
                //Hallar el porcentaje (Barra de progreso)
                backgroundWorker.ReportProgress(iDom * 100 / iDomMax);
                //Cancelar
                if (backgroundWorker.CancellationPending) return;
            }

            SFI = (funcionIterativa[])(funcionIterativaRan).ToArray(typeof(funcionIterativa));

            foreach (funcionIterativa func in SFI) {
                if (func.q < -255) func.q = -255;
                if (func.q > 255) func.q = 255;
            }
        }
        float d(int iDom, int jDom, int iRan, int jRan, tipoTransformacion tTipo, metodoComparacion cTipo, float q) {
            // xRanStart Punto de inicio para el recorrido del Rango en el Eje X
            // xRanStep Direccion del Rango en Eje X
            // yRanStep Punto de inicio para el recorrido del Rango en el Eje Y
            // yRanStep Direccion del Rango en Eje Y
            int xRanStart, xRanStep, yRanStart, yRanStep, longitud;
            // imDom es la porcion de imagen
            // imRan es la proyeccion a menor escala de la imDom
            float[,] imRan, imDom;
            float res = 0;
            xRanStart = xRanStep = yRanStart = yRanStep = longitud = 0;
            imRan = imDom = null;

            switch (cTipo) {
                case metodoComparacion.Cmp2x2:
                    imRan = im64;
                    imDom = im128;
                    longitud = 2;
                    break;
                case metodoComparacion.Cmp4x4:
                    imRan = im128;
                    imDom = im256;
                    longitud = 4;
                    break;
                case metodoComparacion.Cmp8x8:
                    imRan = im256;
                    imDom = im512;
                    longitud = 8;
                    break;
            }
            //iRan es la posicion actual del recorrido en el eje X
            //jRan es la posicion actual del recorrido en el eje Y
            switch (tTipo) {
                case tipoTransformacion.Deg0:
                    xRanStart = iRan * longitud;
                    xRanStep = 1;
                    yRanStart = jRan * longitud;
                    yRanStep = 1;
                    break;
                case tipoTransformacion.Deg90:
                    xRanStart = jRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = iRan * longitud;
                    yRanStep = 1;
                    break;
                case tipoTransformacion.Deg180:
                    xRanStart = iRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = jRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg270:
                    xRanStart = jRan * longitud;
                    xRanStep = 1;
                    yRanStart = iRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;

                case tipoTransformacion.Deg0Sim:
                    xRanStart = iRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = jRan * longitud;
                    yRanStep = 1;
                    break;
                case tipoTransformacion.Deg90Sim:
                    xRanStart = jRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = iRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg180Sim:
                    xRanStart = iRan * longitud;
                    xRanStep = 1;
                    yRanStart = jRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg270Sim:
                    xRanStart = jRan * longitud;
                    xRanStep = 1;
                    yRanStart = iRan * longitud;
                    yRanStep = 1;
                    break;
            }

            int xDomMax = longitud * (iDom + 1);
            int yDomMax = longitud * (jDom + 1);
            //xRan e yRan son posiciones temporales para recorrer 
            int xRan = xRanStart, yRan;
            //res es el promedio de brillos de un fractal
            for (int xDom = iDom * longitud; xDom < xDomMax; xDom++) {
                yRan = yRanStart;
                for (int yDom = jDom * longitud; yDom < yDomMax; yDom++) {
                    float delta;
                    
                    delta = imRan[xRan, yRan] * 0.80f + q - imDom[xDom, yDom];
                    
                    if (linearCriterion) {
                      if (delta >= 0) res += delta;
                      else res -= delta;
                    }
                    else {
                        res += (delta * delta);
                    }
                    yRan += yRanStep;
                }
                xRan += xRanStep;
            }
            //
            return res;
        }
        //Guarda el fractal utilizando archivos binarios para reducir su peso
        public void guardarImagenComprimida(BinaryWriter writer) {
            foreach (funcionIterativa func in SFI) {
                uint x = 0;
                x += (uint)func.iRan;
                x = x << 5;
                x += (uint)func.jRan;
                x = x << 3;
                x += (uint)func.tipo;
                x = x << 9;
                x += (uint)(func.q + 255);
                x = x << 10;
                writer.Write(x);
            }
        }
    }
    public class Descompresion {
        float[,] im;
        float[,] tempIm;
        public int repeticiones;
        public funcionIterativa[] SFI;
        const int iRanMax = 32, jRanMax = 32, iDomMax = 64, jDomMax = 64;
        public void Descomprimir() {
            //lienzo de 512 x 512
            im = new float[512, 512];
            tempIm = new float[512, 512];
            for (int i = 0; i < 512; i++) {
                for (int j = 0; j < 512; j++) {
                    im[i, j] = 255;
                }
            }
            //
            for (int k = 0; k < repeticiones; k++) {
                for (int iDom = 0; iDom < iDomMax; iDom++) {
                    for (int jDom = 0; jDom < jDomMax; jDom++) {
                        int funcIndex = (iDom * jDomMax) + jDom;
                        funcionIterativa func = SFI[funcIndex];
                        dibujarBloqueDominio(iDom, jDom, func.iRan, func.jRan, func.tipo, func.q);
                    }
                }
                tempIm = im;
            }
        }
        void dibujarBloqueDominio(int iDom, int jDom, int iRan, int jRan, tipoTransformacion tipo, float q) {
            // xRanStart Punto de inicio para el recorrido del Rango en el Eje X
            // xRanStep Direccion del Rango en Eje X
            // yRanStep Punto de inicio para el recorrido del Rango en el Eje Y
            // yRanStep Direccion del Rango en Eje Y
            int xRanStart, xRanStep, yRanStart, yRanStep, longitud = 8;
            xRanStart = xRanStep = yRanStart = yRanStep = 0;

            switch (tipo) {
                case tipoTransformacion.Deg0:
                    xRanStart = iRan * longitud;
                    xRanStep = 1;
                    yRanStart = jRan * longitud;
                    yRanStep = 1;
                    break;
                case tipoTransformacion.Deg90:
                    xRanStart = jRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = iRan * longitud;
                    yRanStep = 1;
                    break;
                case tipoTransformacion.Deg180:
                    xRanStart = iRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = jRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg270:
                    xRanStart = jRan * longitud;
                    xRanStep = 1;
                    yRanStart = iRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg0Sim:
                    xRanStart = iRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = jRan * longitud;
                    yRanStep = 1;
                    break;
                case tipoTransformacion.Deg90Sim:
                    xRanStart = jRan * longitud + longitud - 1;
                    xRanStep = -1;
                    yRanStart = iRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg180Sim:
                    xRanStart = iRan * longitud;
                    xRanStep = 1;
                    yRanStart = jRan * longitud + longitud - 1;
                    yRanStep = -1;
                    break;
                case tipoTransformacion.Deg270Sim:
                    xRanStart = jRan * longitud;
                    xRanStep = 1;
                    yRanStart = iRan * longitud;
                    yRanStep = 1;
                    break;
            }
            // xDomMax limite del recorrido en el eje X
            // yDomMax limite del recorrido en el eje Y
            int xDomMax = longitud * (iDom + 1);
            int yDomMax = longitud * (jDom + 1);
            int xRan = xRanStart * 2, yRan;
            //Asigna el brillo a cada pixel del lienzo en blanco
            for (int xDom = iDom * longitud; xDom < xDomMax; xDom++) {
                yRan = yRanStart * 2;
                for (int yDom = jDom * longitud; yDom < yDomMax; yDom++) {
                    float ranPixelim = (im[xRan, yRan] + im[xRan + 1, yRan] + im[xRan, yRan + 1] + im[xRan + 1, yRan + 1]) / 4.0f;
                    tempIm[xDom, yDom] = ranPixelim * 0.75f + q;
                    yRan += 2 * yRanStep;
                }
                xRan += 2 * xRanStep;
            }
        }
        public Bitmap ObtenerImagen() {
            Bitmap image = new Bitmap(512, 512);
            for (int x = 0; x < image.Width; x++) {
                for (int y = 0; y < image.Height; y++) {
                    if (tempIm[x, y] < 0) {
                        //color negro
                        tempIm[x, y] = 0;
                    } else if (tempIm[x, y] > 255) {
                        //color blanco
                        tempIm[x, y] = 255;
                    }
                    Color c = Color.FromArgb(253,(int)tempIm[x, y], (int)tempIm[x, y], (int)tempIm[x, y]);
                    image.SetPixel(x, y, c);
                }
            }
            return image;
        }
        public void abrirImagenComprimida(BinaryReader reader) {
            // 4096 = iDomMax * jDomMax
            SFI = new funcionIterativa[4096];
            for (int i = 0; i < SFI.Length; i++) {
                uint x = reader.ReadUInt32();
                int iRan = (int)(x >> 27);
                int jRan = (int)(x << 5 >> 27);
                tipoTransformacion type = (tipoTransformacion)(x << 10 >> 29);
                float q = (int)(x << 13 >> 23) - 255;
                SFI[i] = new funcionIterativa(iRan, jRan, type, q);
            }
        }
    }
}