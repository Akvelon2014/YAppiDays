using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MobileConference.Models
{
    public class ImageSizeParams
    {
        public int x1 { get; set; }

        public int x2 { get; set; }

        public int y1 { get; set; }

        public int y2 { get; set; }

        public int Width
        {
            get { return x2 - x1; }
        }

        public int Height
        {
            get { return y2 - y1; }
        }
    }
}