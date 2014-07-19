﻿//BSD 2014,
//ArthurHub

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;
using HtmlRenderer.Drawing;

namespace HtmlRenderer.Boxes
{
    /// <summary>
    /// CSS box for image element.
    /// </summary>
    sealed class CssBoxImage : CssBox
    {
        #region Fields and Consts
        /// <summary>
        /// the image word of this image box
        /// </summary>
        readonly CssImageRun _imageWord;
        ImageBinder _imgBinder;
        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxImage(CssBox parent, object tag, Css.BoxSpec boxSpec, ImageBinder binder)
            : base(parent, tag, boxSpec)
        {

            this._imageWord = new CssImageRun();
            this._imageWord.SetOwner(this);
            this.SetContentRuns(new List<CssRun>() { _imageWord }, false);
            this._imgBinder = binder;
        }

        /// <summary>
        /// Get the image of this image box.
        /// </summary>
        public Image Image
        {
            get { return this._imgBinder.Image; }
        }
        //void OnImageBinderLoadingComplete()
        //{
        //    //when binder state changed 
        //    switch (_imgBinder.State)
        //    {
        //        case ImageBinderState.Loaded:
        //            {
        //                //get image from binder
        //                //-----------------------
        //                var img = _imgBinder.Image;
        //                _imageWord.Image = img;
        //                _imageWord.ImageRectangle = new Rectangle(0, 0, img.Width, img.Height);
        //                _imageLoadingComplete = true;
        //                this.RunSizeMeasurePass = false;
        //                //-----------------------

        //                //if (_imageLoadingComplete && image == null)
        //                //{
        //                //    SetErrorBorder();
        //                //} 
        //                //if (!HtmlContainer.AvoidImagesLateLoading || async)
        //                //{
        //                //    var width = this.Width;//new CssLength(Width);
        //                //    var height = this.Height;// new CssLength(Height);
        //                //    var layout = (width.Number <= 0 || width.Unit != CssUnit.Pixels) || (height.Number <= 0 || height.Unit != CssUnit.Pixels);
        //                //    HtmlContainer.RequestRefresh(layout);
        //                //}

        //            } break;
        //    }
        //}
        internal void PaintImage(IGraphics g, RectangleF rect, PaintVisitor p)
        {

            PaintBackground(p, rect, true, true);

            if (this.HasSomeVisibleBorder)
            {
                p.PaintBorders(this, rect, true, true);
            }
            //--------------------------------------------------------- 
            RectangleF r = _imageWord.Rectangle;

            r.Height -= ActualBorderTopWidth + ActualBorderBottomWidth + ActualPaddingTop + ActualPaddingBottom;
            r.Y += ActualBorderTopWidth + ActualPaddingTop;
            r.X = (float)Math.Floor(r.X);
            r.Y = (float)Math.Floor(r.Y);

            bool tryLoadOnce = false;

        EVAL_STATE:
            switch (_imgBinder.State)
            {
                case ImageBinderState.Unload:
                    {
                        //async request image
                        if (!tryLoadOnce)
                        {
                            _imageWord.ImageBinder = this._imgBinder;
                            p.RequestImageAsync(_imgBinder, this._imageWord, this);
                            //retry again
                            tryLoadOnce = true;
                            goto EVAL_STATE;
                        }
                    } break;
                case ImageBinderState.Loading:
                    {
                        //RenderUtils.DrawImageLoadingIcon(g, r);
                    } break;
                case ImageBinderState.Loaded:
                    {

                        System.Drawing.Image img;
                        if ((img = _imgBinder.Image) != null)
                        {

                            if (_imageWord.ImageRectangle == Rectangle.Empty)
                            {
                                g.DrawImage(img,
                                    new RectangleF(r.Left, r.Top,
                                        img.Width, img.Height));

                               // g.DrawImage(img, Rectangle.Round(r));
                            }
                            else
                            {
                                //
                                g.DrawImage(img, _imageWord.ImageRectangle);
                                //g.DrawImage(_imageWord.Image, Rectangle.Round(r), _imageWord.ImageRectangle);
                            }
                        }
                        else
                        {
                            RenderUtils.DrawImageLoadingIcon(g, r);
                            if (r.Width > 19 && r.Height > 19)
                            {
                                g.DrawRectangle(Pens.LightGray, r.X, r.Y, r.Width, r.Height);
                            }
                        }
                    } break;
                case ImageBinderState.NoImage:
                    {

                    } break;
                case ImageBinderState.Error:
                    {
                        RenderUtils.DrawImageErrorIcon(g, r);
                    } break;
            }

            //p.PopLocalClipArea();

        }
        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">the device to draw to</param>
        protected override void PaintImp(IGraphics g, PaintVisitor p)
        {
            // load image iff it is in visible rectangle  
            //1. single image can't be splited  
            PaintImage(g, new RectangleF(0, 0, this.SizeWidth, this.SizeHeight), p);
        }

        /// <summary>
        /// Assigns words its width and height
        /// </summary>
        /// <param name="g">the device to use</param>
        internal override void MeasureRunsSize(LayoutVisitor lay)
        {
            if (!this.RunSizeMeasurePass)
            {   
                this.RunSizeMeasurePass = true;
            }
            CssLayoutEngine.MeasureImageSize(_imageWord, lay);
        } 
        #region Private methods

        ///// <summary>
        ///// Set error image border on the image box.
        ///// </summary>
        //private void SetErrorBorder()
        //{

        //    this.SetAllBorders(
        //        CssBorderStyle.Solid, CssLength.MakePixelLength(2),
        //        System.Drawing.Color.FromArgb(0xA0, 0xA0, 0xA0));

        //    BorderRightColor = BorderBottomColor = System.Drawing.Color.FromArgb(0xE3, 0xE3, 0xE3);// "#E3E3E3";
        //}



        #endregion
    }
}