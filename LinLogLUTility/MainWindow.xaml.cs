using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace LinLogLUTility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            allInitialized = true;
            ReCalculate();
        }

        int inputBitDepth = 12;
        int outputBitDepth = 8;
        bool allInitialized = false;
        bool calculatedSuccessfully = false;
        double calculatedParameterA = 0.0000;

        private void ReCalculate()
        {
            if(!allInitialized)
            {
                return;
            }

            bool parseOK = int.TryParse(txtInputBitDepth.Text,out inputBitDepth);
            parseOK &= int.TryParse(txtOutputBitDepth.Text,out outputBitDepth);

            if (!parseOK)
            {
                // Nada.
                return;
            }
            
            if(inputBitDepth <= outputBitDepth)
            {
                // Nope! That will lead to an infinite loop in finding Parameter A
                return;
            }

            double parameterA = Helpers.findParameter(Math.Pow(2,inputBitDepth) - 1, Math.Pow(2,outputBitDepth) - 1, TransferFunctionV1.LinToLog);
            txtCalculatedAParameter.Text = parameterA.ToString("N6");
            calculatedParameterA = parameterA;
            calculatedSuccessfully = true;

        }

        private void BitDepthValuesChanged(object sender, TextChangedEventArgs e)
        {
            calculatedSuccessfully = false;
            ReCalculate();
        }

        // Generate
        private void GenerateLUTs()
        {
            if (!calculatedSuccessfully)
            {
                return; // something wong here.
            }

            int i = 0;
            double maxInputValue = Math.Pow(2, inputBitDepth) - 1;
            double maxOutputValue = Math.Pow(2, outputBitDepth) - 1;
            int startValue = 0;

            // Calculate Lin to Log LUT data
            int stepAmount = (int)Math.Pow(2, inputBitDepth);
            double[] linLogValues = new double[stepAmount];
            double[] linLogSRGBValues = new double[stepAmount];
            double tmp;
            for(i = startValue; i <= maxInputValue; i++)
            {
                tmp = TransferFunctionV1.LinToLog(i, calculatedParameterA) / maxOutputValue;
                linLogValues[i] = tmp;
                linLogSRGBValues[i] = ColorHelpers.applyInverseSRGBTransferFunction(tmp); // When the final log footage is saved in sRGB (converted from the true log interpreted as linear after working in linear space), we need to apply inverse sRGB transfer function first to get the correct amount of steps across the entire 8 bits in the final saved file.
            }

            // Calculate Log to Lin LUT data
            stepAmount = (int)Math.Pow(2, outputBitDepth);
            double[] logLinValues = new double[stepAmount];
            double[] logSRGBLinValues = new double[stepAmount];
            double sRGBPreProc;
            for (i = startValue; i <= maxOutputValue; i++)
            {
                sRGBPreProc = ColorHelpers.applySRGBTransferFunction(i / maxOutputValue)*maxOutputValue; // If it was saved in an sRGB format using the sRGB compensated LUT and is reconverted back to Linear, it needs the correct counterpart to get correct values again
                logLinValues[i] = TransferFunctionV1.LogToLin(i, calculatedParameterA) / maxInputValue;
                logSRGBLinValues[i] = TransferFunctionV1.LogToLin(sRGBPreProc, calculatedParameterA) / maxInputValue;
            }

            // Generate LUTS
            string linLogLUT = CubeLUT1D.ArrayToLUT1DString(linLogValues, "Linear to Log LUT by LinLogLUTitlity, Transfer Function V1, y = log[a+1,a*x+1], reversal x = ((a + 1)^y - 1)/a, linear bit depth " + inputBitDepth+", log bit depth "+outputBitDepth+", parameter A is "+calculatedParameterA);
            string linLogSRGBLUT = CubeLUT1D.ArrayToLUT1DString(linLogSRGBValues, "Linear to sRGB compensated Log LUT by LinLogLUTitlity, Transfer Function V1, y = log[a+1,a*x+1], reversal x = ((a + 1)^y - 1)/a, linear bit depth " + inputBitDepth+", log bit depth "+outputBitDepth+", parameter A is "+calculatedParameterA);
            string logLinLUT = CubeLUT1D.ArrayToLUT1DString(logLinValues, "Log to Linear Reversal LUT by LinLogLUTitlity, Transfer Function V1, y = log[a+1,a*x+1], reversal x = ((a + 1)^y - 1)/a, linear bit depth " + inputBitDepth+ ", log bit depth " + outputBitDepth+", parameter A is "+calculatedParameterA);
            string logSRGBLinLUT = CubeLUT1D.ArrayToLUT1DString(logSRGBLinValues, "sRGB compensated Log to Linear Reversal LUT by LinLogLUTitlity, Transfer Function V1, y = log[a+1,a*x+1], reversal x = ((a + 1)^y - 1)/a, linear bit depth " + inputBitDepth+ ", log bit depth " + outputBitDepth+", parameter A is "+calculatedParameterA);

            string linLogFilename = "Lin2Log_TF1_lin" + inputBitDepth + "bit_log" + outputBitDepth + "bit.cube";
            string linLogSRGBFilename = "Lin2Log(sRGBcompensated)_TF1_lin" + inputBitDepth + "bit_log" + outputBitDepth + "bit.cube";
            string logLinFilename = "Log2Lin_TF1_lin" + inputBitDepth + "bit_log" + outputBitDepth + "bit.cube";
            string logSRGBLinFilename = "Log(sRGBcompensated)2Lin_TF1_lin" + inputBitDepth + "bit_log" + outputBitDepth + "bit.cube";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = linLogFilename;
            sfd.Title = "Choose place to save Linear To Log .cube LUT.";
            sfd.Filter = "cube LUT (*.cube)|*.cube";
            if(sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, linLogLUT);
            }

            sfd = new SaveFileDialog();
            sfd.FileName = linLogSRGBFilename;
            sfd.Title = "Choose place to save Linear To sRGB compensated Log .cube LUT.";
            sfd.Filter = "cube LUT (*.cube)|*.cube";
            if(sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, linLogSRGBLUT);
            }

            sfd = new SaveFileDialog();
            sfd.FileName = logLinFilename;
            sfd.Title = "Choose place to save Log to Linear .cube LUT.";
            sfd.Filter = "cube LUT (*.cube)|*.cube";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, logLinLUT);
            }

            sfd = new SaveFileDialog();
            sfd.FileName = logSRGBLinFilename;
            sfd.Title = "Choose place to save sRGB compensated Log to Linear .cube LUT.";
            sfd.Filter = "cube LUT (*.cube)|*.cube";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, logSRGBLinLUT);
            }

            string instructions = "LinLogLUTility LUT Pack \n" +
                "\n" +
                "Transfer Function V1, y = log[a + 1, a * x + 1], reversal x = ((a + 1) ^ y - 1) / a, linear bit depth " + inputBitDepth+", log bit depth "+outputBitDepth+", parameter A is "+calculatedParameterA+"\n" +
                "\n" +
                "If the bit depths of your linear footage or of your archival format do not match "+inputBitDepth+" bit and "+outputBitDepth+" respectively, generate a new LUT pack using LinLogLUTility. \n" +
                "\n" +
                "Usage instructions for After Effects:\n" +
                "\n" +
                "Usage of Lin2Log LUT:\n" +
                "1. Import raw linear "+inputBitDepth+" bit scans in After Effects\n" +
                "2. Make sure you are working in a linearized sRGB space in 16 bit\n" +
                "3.1. If your output will be in the same color space as your composition (sRGB linear), use the normal lin2log 3D LUT and apply it to your footage\n"+
                "3.2. If your output will be in sRGB but no change of RGB data occurs, aka 'mistagging' (not recommended), also use the normal lin2log 3D LUT and apply it to your footage\n"+
                "3.3. If your output will be in sRGB converted from the linear color space interpreted as linear color space, use the lin2log(sRGBcompensated) LUT instead. It has an inverse sRGB transfer function applied so that the correct spacing of values is achieved after conversion from linear sRGB to normal sRGB. This will be what you need if you are just doing a normal export from After Effects, for example to ProRes, since the output color space is by default set to sRGB/Rec709.\n" +
                "4. Taking into account your choice of point 3, save to your desired "+outputBitDepth+" bit archival format\n" +
                "5. Take notes of which approach you chose in point 3 and save it in a text file next to the archival video or image sequence. If you share the Log footage with anyone, include said text file so they know how to properly decode the footage. Also include this LUT pack with the footage in case the algorithm changes or the program becomes unavailable for unpredictable reasons.\n" +
                "\n" +
                "\n" +
                "Usage of Log2Lin LUT:\n" +
                "1. Load your archival format into After Effects\n" +
                "2. If your archival format is in 'mistagged' sRGB color space as mentioned in point 3.2 (not recommended) of the Lin2Log instructions, make sure to explicitly interpret the footage as linear sRGB (Google if you don't know what interpreting footage means).\n" +
                "3. Make sure you are working in a linearized sRGB space in 16 bit\n"+
                "4.1. If your archival format is in linear color space, use the normal Log2Lin LUT to get back to your raw linear data\n" +
                "4.2. If your archival format is in 'mistagged' sRGB color space (not recommended) that is now correctly interpreted as linear color space, also use the normal Log2Lin LUT\n" +
                "4.3. If your archival format is in sRGB color space that has been converted from Linear color space (like in 3.3 of the Lin2Log instructions), use the Log(sRGBcompensated)2Lin LUT to get back the correct data\n" +
                "5. Do whatever you want with your restored footage!\n" +
                "\n" +
                "\n" +
                "\n" +
                "Notes:\n" +
                "- Do not apply any color corrections whatsoever to the image that was converted to Log, otherwise the reversal LUT won't be able to perfectly reverse the Linear to Log conversion.\n" +
                "- If you don't keep track of the color spaces you are using, you will likely get unpredictable and incorrect results.\n" +
                "- If you apply the non-sRGB compensated Lin2Log to footage that is being converted to sRGB during export, you will waste bandwidth in the shadow areas of the image and have less bandwidth left for the brighter parts of the image, increasing the probability of banding\n" +
                "- If you apply the sRGB compensated Lin2Log to footage that isn't actually being converted to sRGB, you will lose shadow detail!\n"+
                "- If you apply the sRGB compensated Log2Lin to footage that was saved in linear color space, the image will be too bright and washed out\n" +
                "- If you apply the normal Log2Lin to footage that was saved in sRGB color space with the sRGB compensated Lin2Log, the resulting image will be too dark and contrasty.\n" +
                "- If you set the output color space for the export to linear sRGB, but end up with a file that has the Rec709 color space (for example with ProRes exporters that only support Rec709), you have a 'mistagged' sRGB file on your hands that is actually linear data. In this case, you will need to interpret it as linear sRGB every time you import it.\n" +
                "- The importance of the two different sets of LUTS (sRGB compensated and not) stems from the fact that these LUTs are calculated with high precision so that the raw value of 0 and 1 in the input correspond to the raw value of 0 and 1 in the output. The sRGB conversion applies gamma and multiplies dark values with 12.92, as a result you are stretching a value that can only be 0 or 1 to the range of 0-13, which is obviously a waste of bandwidth. The sRGB compensated Lin2Log applies the inverse sRGB curve to all image data so that once the sRGB conversion happens, you end up with the correct values 0 and 1 in the actual 8 bit file.\n" +
                "\n" +
                "Enjoy!\n" +
                "\n";

            sfd = new SaveFileDialog();
            sfd.FileName = "LinLogLUTility_LUT_instructions_lin" + inputBitDepth + "bit_log" + outputBitDepth + "bit.txt";
            sfd.Title = "Choose place to save instructions for usage of the LUTs.";
            sfd.Filter = "Instructions (*.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, instructions);
            }


        }

        private void BtnGenerateLUTs_Click(object sender, RoutedEventArgs e)
        {
            if (calculatedSuccessfully)
            {
                GenerateLUTs();
            }
        }
    }
}
