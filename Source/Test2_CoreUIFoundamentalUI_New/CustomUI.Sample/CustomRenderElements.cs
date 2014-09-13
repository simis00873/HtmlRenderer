﻿//2014 Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using LayoutFarm.Text;
using LayoutFarm.UI;

namespace LayoutFarm.SampleControls
{
    class CustomRenderElement : RenderElement
    {
        public CustomRenderElement(int w, int h)
            : base(w, h)
        {

        }
        public override void CustomDrawToThisPage(Canvas canvasPage, InternalRect updateArea)
        {

            canvasPage.FillRectangle(Brushes.Green, new Rectangle(0, 0, this.Width, this.Height));
        }
    }
    class CustomRenderBox : RenderBoxBase
    {
        public CustomRenderBox(int width, int height)
            : base(width, height)
        {
        }
        public override void ClearAllChildren()
        {
        }
        public override void CustomDrawToThisPage(Canvas canvasPage, InternalRect updateArea)
        {
            //sample bg
            canvasPage.FillRectangle(Brushes.LightGray, new Rectangle(0, 0, this.Width, this.Height));
            if (this.Layers != null)
            {
                this.Layers.LayersDrawContent(canvasPage, updateArea);
            }
        }
    }
}