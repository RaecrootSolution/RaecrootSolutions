using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ICSI_UDIN.Models
{ 
    public class UDIN_Captcha       
    {
      
       
        public byte[] CreateCaptchaImage(int? isNew)
        {

            byte[] imageByteData = null;
            string code = string.Empty;
            if (isNew == 1)
            {
                code = GetRandomText();
                System.Web.HttpContext.Current.Session["UDINCaptchaCode"] = code;
            }
            else
            {
                code = System.Web.HttpContext.Current.Session["UDINCaptchaCode"].ToString();
            }

          
            int height = 30;
            int width = 100;

            using (Bitmap bmp = new Bitmap(100, 30))
            {
                RectangleF rectf = new RectangleF(10, 5, 0, 0);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.WhiteSmoke);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    Pen pen = new Pen(Color.White);
                    Font font = new Font("Thaoma", 12, FontStyle.Italic);


                    g.DrawString(code, font, Brushes.Green, rectf);
                    g.DrawRectangle(pen, 1, 1, width - 2, height - 2);
                    g.Flush();

                    MemoryStream stream = new MemoryStream();
                    bmp.Save(stream, ImageFormat.Gif);
                    imageByteData = stream.ToArray();

                    g.Dispose();
                    bmp.Dispose();
                    return imageByteData;

                }
            }
        }
       
        private string GetRandomText()
        {
            StringBuilder randomText = new StringBuilder();
            string alphabets = "AB3C4DEFG6HJKLM5NPQR2STUV7WX8YZ9abcdefghjkmnpqrstuvxyz";
            Random rndom = new Random();
            for (int j = 0; j <= 5; j++)
            {
                randomText.Append(alphabets[rndom.Next(alphabets.Length)]);
            }
            System.Web.HttpContext.Current.Session["UDINCaptchaCode"] = randomText.ToString();
            return System.Web.HttpContext.Current.Session["UDINCaptchaCode"] as String;

           
        }
    }
}