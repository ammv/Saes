using Avalonia.Controls;
using QRCoder;
using System.IO;

namespace Saes.AvaloniaMvvmClient.Views.Authentication.User
{
    public partial class QrCodeOtpWindow : Window
    {
        public QrCodeOtpWindow()
        {
            InitializeComponent();

            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            //PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            //byte[] qrCodeImage = qrCode.GetGraphic(20);
        }

        public void SetQrCode(string data)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            Stream stream = new MemoryStream(qrCodeImage);

            var image = new Avalonia.Media.Imaging.Bitmap(stream);

            QrCodeImage.Source = image;
        }
    }
}
