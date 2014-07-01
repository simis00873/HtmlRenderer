﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using HtmlRenderer.Parse;


namespace HtmlRenderer.Dom
{
    partial class CssBox
    {

        float _actualLineHeight;
        float _actualWordSpacing;
        float _actualTextIndent;

        float _actualBorderSpacingHorizontal;
        float _actualBorderSpacingVertical;

        /// <summary>
        /// Gets the line height
        /// </summary>
        public float ActualLineHeight
        {
            get
            {
                return _actualLineHeight;

                //if ((this._prop_pass_eval & CssBoxBaseAssignments.LINE_HEIGHT) == 0)
                //{
                //    this._prop_pass_eval |= CssBoxBaseAssignments.LINE_HEIGHT;
                //    _actualLineHeight = .9f * CssValueParser.ParseLength(LineHeight, this.SizeHeight, this);
                //}
                //return _actualLineHeight;
            }
        }

        /// <summary>
        /// Gets the text indentation (on first line only)
        /// </summary>
        public float ActualTextIndent
        {
            get
            {
                if ((this._prop_pass_eval & CssBoxBaseAssignments.TEXT_INDENT) == 0)
                {
                    this._prop_pass_eval |= CssBoxBaseAssignments.TEXT_INDENT;
                    _actualTextIndent = CssValueParser.ParseLength(TextIndent, this.SizeWidth, this);
                }
                return _actualTextIndent;
            }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public float ActualWordSpacing
        {
            get { return _actualWordSpacing; }
        }
        protected float MeasureWordSpacing(LayoutVisitor lay)
        {
            if ((this._prop_pass_eval & CssBoxBaseAssignments.WORD_SPACING) == 0)
            {
                this._prop_pass_eval |= CssBoxBaseAssignments.WORD_SPACING;

                _actualWordSpacing = lay.MeasureWhiteSpace(this);

                if (!this.WordSpacing.IsNormalWordSpacing)
                {
                    _actualWordSpacing += CssValueParser.ParseLength(this.WordSpacing, 1, this);
                }
            }
            return this._actualWordSpacing;
        }
        /// <summary>
        /// Gets the actual horizontal border spacing for tables
        /// </summary>
        public float ActualBorderSpacingHorizontal
        {
            get
            {
                if ((this._prop_pass_eval & CssBoxBaseAssignments.BORDER_SPACING_H) == 0)
                {
                    this._prop_pass_eval |= CssBoxBaseAssignments.BORDER_SPACING_H;
                    _actualBorderSpacingHorizontal = this.boxSpec.BorderSpacingHorizontal.Number;
                }
                return _actualBorderSpacingHorizontal;
            }
        }

        /// <summary>
        /// Gets the actual vertical border spacing for tables
        /// </summary>
        public float ActualBorderSpacingVertical
        {
            get
            {
                if ((this._prop_pass_eval & CssBoxBaseAssignments.BORDER_SPACING_V) == 0)
                {
                    this._prop_pass_eval |= CssBoxBaseAssignments.BORDER_SPACING_V;
                    _actualBorderSpacingVertical = this.boxSpec.BorderSpacingVertical.Number;
                }
                return _actualBorderSpacingVertical;
            }
        }
    }
}