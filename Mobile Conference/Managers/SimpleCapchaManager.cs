using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MobileConference.Interface;

namespace MobileConference.Managers
{
    public class SimpleCapchaManager:ICapchaManager
    {

        public MemoryStream GetCapcha(int width, int height, out string answer)
        {
            var rand = new Random((int)DateTime.Now.Ticks); 
            //generate new question 
            int a = rand.Next(10, 99); 
            int b = rand.Next(0, 9); 
            var captcha = string.Format("{0} + {1} = ?", a, b);
            var mem = new MemoryStream();

            using (var bmp = new Bitmap(width, height))
            using (var gfx = Graphics.FromImage((Image)bmp)) 
            { 
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit; 
                gfx.SmoothingMode = SmoothingMode.AntiAlias; 
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height)); 
 
                //add noise 
                var pen = new Pen(Color.Yellow); 
                for (int i = 1; i < 10; i++) 
                { 
                    pen.Color = Color.FromArgb( 
                    (rand.Next(0, 255)), 
                    (rand.Next(0, 255)), 
                    (rand.Next(0, 255))); 
 
                    int r = rand.Next(0, (width / 3)); 
                    int x = rand.Next(0, width); 
                    int y = rand.Next(0, height); 
 
                    gfx.DrawEllipse(pen, x - r, y - r, r, r); 
                } 
 
                //add question 
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            answer = (a + b).ToString();
           
            return mem;
        }
    }
}