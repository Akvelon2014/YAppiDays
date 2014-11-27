using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MobileConference.Enums;

namespace MobileConference.Models
{
    public class Tip
    {
        public string Content { get; set; }
        public string RelatedElemenetId { get; set; }
        public int Width { get; set; }
        public int TriangleOffset { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public TipType Type { get; set; }
        public int Number { get; set; }

        public Tip(string content, string relatedElementId)
        {
            Content = content;
            RelatedElemenetId = relatedElementId;
            Width = 150;
            TriangleOffset = 15;
            OffsetX = OffsetY = 0;
            Type = TipType.Top;
        }

        public override string ToString()
        {
            return string.Format("<div class='tip {0}' id='tip{1}' data-width='{2}' data-for='{3}' data-offset='{4}' " +
                                 "data-offset-x='{5}' data-offset-y='{6}'>{7}</div>"
                                 ,Type.GetClass()           //0
                                 ,Number.ToString()         //1
                                 ,Width.ToString()          //2
                                 ,RelatedElemenetId         //3
                                 ,TriangleOffset.ToString() //4
                                 , OffsetX.ToString()       //5
                                 , OffsetY.ToString()       //6
                                 ,Content                   //7
                                 );
        }
    }
}