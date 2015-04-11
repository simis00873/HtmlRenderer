﻿//BSD 2014 ,WinterDev 
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;

namespace LayoutFarm.HtmlBoxes
{

    public class SelectionRange
    {
        //start line

        CssLineBox startHitHostLine;
        List<CssLineBox> selectedLines;
        bool isValid = true;
        CssBoxHitChain tmpStartChain = null;

        int startLineBeginSelectionAtPixel;

        public SelectionRange(CssBoxHitChain startChain,
            CssBoxHitChain endChain,
            IFonts ifonts)
        {
            if (IsOnTheSameLine(startChain, endChain))
            {
                //on the same line
                if (endChain.RootGlobalX < startChain.RootGlobalX)
                {
                    //swap
                    var tmp = endChain;
                    endChain = startChain;
                    startChain = tmp;
                }
            }
            else
            {
                //across line 
                if (endChain.RootGlobalY < startChain.RootGlobalY)
                {    //swap
                    var tmp = endChain;
                    endChain = startChain;
                    startChain = tmp;
                }
            }


            //1.
            this.SetupStartHitPoint(startChain, ifonts);
            //2. 
            if (this.startHitHostLine == null)
            {
                this.isValid = false;
                return;
            }

            this.tmpStartChain = startChain;
            this.SetupEndHitPoint(endChain, ifonts);
        }

        public bool IsValid
        {
            get { return this.isValid; }
        }

        public void ClearSelectionStatus()
        {

            if (this.selectedLines != null)
            {
                for (int i = selectedLines.Count - 1; i >= 0; --i)
                {
                    this.selectedLines[i].LineSelectionWidth = 0;
                }
                this.selectedLines.Clear();
            }
            else
            {
                if (this.startHitHostLine != null)
                {
                    this.startHitHostLine.LineSelectionWidth = 0;
                }

            }
        }

        void SetupStartHitPoint(CssBoxHitChain startChain, IFonts ifonts)
        {

            //find global location of start point
            HitInfo startHit = startChain.GetLastHit();
            //-----------------------------
            switch (startHit.hitObjectKind)
            {
                case HitObjectKind.Run:
                    {
                        CssRun run = (CssRun)startHit.hitObject;
                        //-------------------------------------------------------
                        int sel_index;
                        int sel_offset;

                        run.FindSelectionPoint(ifonts,
                             startHit.localX,
                             out sel_index,
                             out sel_offset);


                        //modify hitpoint
                        this.startHitHostLine = (CssLineBox)startChain.GetHitInfo(startChain.Count - 2).hitObject;
                        this.startLineBeginSelectionAtPixel = (int)(run.Left + sel_offset);

                    } break;
                case HitObjectKind.LineBox:
                    {

                        this.startHitHostLine = (CssLineBox)startHit.hitObject;
                        this.startLineBeginSelectionAtPixel = startHit.localX;
                        //make global            
                    } break;
                case HitObjectKind.CssBox:
                    {
                        CssBox box = (CssBox)startHit.hitObject;
                        //find first nearest line at point   
                        CssLineBox startHitLine = FindNearestLine(box, startChain.RootGlobalY, 5);

                        this.startLineBeginSelectionAtPixel = 0;
                        if (startHitLine != null)
                        {
                            this.startHitHostLine = startHitLine;
                        }
                        else
                        {
                            //if not found?
                            this.startHitHostLine = null;
                        }
                    } break;
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }


        void SetupEndHitPoint(CssBoxHitChain endChain, IFonts ifonts)
        {

            //find global location of end point 
            HitInfo endHit = endChain.GetLastHit();
            int xposOnEndLine = 0;
            CssLineBox endline = null;
            int run_sel_offset = 0;


            switch (endHit.hitObjectKind)
            {
                default:
                    {
                        throw new NotSupportedException();
                    }
                case HitObjectKind.Run:
                    {

                        CssRun endRun = (CssRun)endHit.hitObject;
                        //if (endRun.Text != null && endRun.Text.Contains("222b"))
                        //{
                        //}

                        int run_sel_index;
                        endRun.FindSelectionPoint(ifonts,
                             endHit.localX,
                             out run_sel_index,
                             out run_sel_offset);

                        //1 . find endline 
                        endline = endRun.HostLine;
                        xposOnEndLine = (int)(endRun.Left + run_sel_offset);

                    } break;
                case HitObjectKind.LineBox:
                    {
                        //1. find endline
                        endline = (CssLineBox)endHit.hitObject;
                        xposOnEndLine = endHit.localX;
                    } break;
                case HitObjectKind.CssBox:
                    {
                        CssBox hitBox = (CssBox)endHit.hitObject;
                        endline = FindNearestLine(hitBox, endChain.RootGlobalY, 5);
                        xposOnEndLine = endHit.localX;

                    } break;
            }
            //----------------------------------
            this.selectedLines = new List<CssLineBox>();
            if (startHitHostLine == endline)
            {
                this.selectedLines.Add(endline);
                startHitHostLine.Select(startLineBeginSelectionAtPixel, xposOnEndLine - startLineBeginSelectionAtPixel);
                return; //early exit here ***
            }

            //---------------------------------- 
            //select on different line 
            LineWalkVisitor lineWalkVisitor = null;
            int breakAtLevel;
            if (FindCommonGround(this.tmpStartChain, endChain, out breakAtLevel))
            {
                var hitBlockRun = endChain.GetHitInfo(breakAtLevel).hitObject as CssBlockRun;
                //multiple select 
                //1. first part        
                startHitHostLine.Select(startLineBeginSelectionAtPixel, (int)hitBlockRun.Left);
                selectedLines.Add(this.startHitHostLine);
                lineWalkVisitor = new LineWalkVisitor(hitBlockRun);
            }
            else
            {
                startHitHostLine.SelectPartialStart(startLineBeginSelectionAtPixel);
                selectedLines.Add(this.startHitHostLine);
                lineWalkVisitor = new LineWalkVisitor(startHitHostLine);
            }

            lineWalkVisitor.SetWalkTargetPosition(endChain.RootGlobalX, endChain.RootGlobalY);
            //----------------------------------  
            lineWalkVisitor.Walk(endline, (lineCoverage, linebox) =>
            {
                switch (lineCoverage)
                {
                    case LineCoverage.EndLine:
                        {   //found end line  
                            linebox.SelectPartialEnd(xposOnEndLine);
                            selectedLines.Add(linebox);
                        } break;
                    case LineCoverage.PartialLine:
                        {
                            //explore all run in this line 
                            int j = linebox.RunCount;
                            bool isOK = false;
                            for (int i = 0; i < j && !isOK; ++i)
                            {
                                var run3 = linebox.GetRun(i) as CssBlockRun;
                                if (run3 == null) continue;
                                //recursive here 

                                foreach (var line2 in GetLineWalkDownIter(lineWalkVisitor, run3.ContentBox))
                                {
                                    if (line2 == endline)
                                    {
                                        //found here!
                                        //add line outter lnie 
                                        if (i > 0)
                                        {
                                            linebox.SelectPartialEnd((int)linebox.GetRun(i - 1).Right);
                                            selectedLines.Add(linebox);
                                        }
                                        line2.SelectPartialEnd(run_sel_offset);
                                        selectedLines.Add(line2);
                                        isOK = true;
                                        break; //break foreach
                                    }
                                    else
                                    {
                                        line2.SelectFull();
                                        selectedLines.Add(line2);
                                    }
                                }
                            }



                        } break;
                    case LineCoverage.FullLine:
                        {
                            //check if hitpoint is in the line area
                            linebox.SelectFull();
                            selectedLines.Add(linebox);
                        } break;
                }
            });
        }
        static bool FindCommonGround(CssBoxHitChain startChain, CssBoxHitChain endChain, out int breakAtLevel)
        {

            //find common ground of startChain and endChain
            int startChainCount = startChain.Count;
            int endChainCount = endChain.Count;
            int lim = Math.Min(startChainCount, endChainCount);
            //from root to leave
            breakAtLevel = 0;
            for (int i = 0; i < lim; ++i)
            {
                var startHitInfo = startChain.GetHitInfo(i);
                var endHitInfo = endChain.GetHitInfo(i);
                if (startHitInfo.hitObject != endHitInfo.hitObject)
                {
                    //found diff here
                    breakAtLevel = i;
                    break;
                }
            }
            //----------------------------
            //check  
            //return isDeepDown = endChainCount > startChainCount && (breakAtLevel == startChainCount - 1);
            return endChainCount > startChainCount && (breakAtLevel == startChainCount - 1);
        }

        static bool IsOnTheSameLine(CssBoxHitChain startChain, CssBoxHitChain endChain)
        {
            CssLineBox startLineBox = GetLine(startChain.GetLastHit());
            CssLineBox endLineBox = GetLine(endChain.GetLastHit());
            return startLineBox != null && startLineBox == endLineBox;
        }
        static CssLineBox GetLine(HitInfo hitInfo)
        {
            switch (hitInfo.hitObjectKind)
            {
                default:
                case HitObjectKind.Unknown:
                    {
                        return null;
                        throw new NotSupportedException();
                    }
                case HitObjectKind.LineBox:
                    {
                        return (CssLineBox)hitInfo.hitObject;
                    }
                case HitObjectKind.Run:
                    {
                        return ((CssRun)hitInfo.hitObject).HostLine;
                    }
                case HitObjectKind.CssBox:
                    {
                        return null;
                    }
            }
        }
        static CssLineBox FindNearestLine(CssBox startBox, int globalY, int yRange)
        {
            CssLineBox latestLine = null;
            CssBox latestLineBoxOwner = null;
            float latestLineBoxGlobalYPos = 0;

            foreach (CssLineBox lineBox in BoxHitUtils.GetDeepDownLineBoxIter(startBox))
            {
                if (lineBox.CacheLineHeight == 0)
                {
                    continue;
                }

                if (latestLineBoxOwner != lineBox.OwnerBox)
                {
                    //find global position of box
                    latestLineBoxOwner = lineBox.OwnerBox;

                    //TODO: review here , duplicate GetGlobalLocation 
                    float gx, gy;
                    latestLineBoxOwner.GetElementGlobalLocation(out gx, out gy);
                    latestLineBoxGlobalYPos = gy;
                }

                float lineGlobalBottom = lineBox.CachedLineBottom + latestLineBoxGlobalYPos;

                if (lineGlobalBottom <= globalY)
                {
                    latestLine = lineBox;
                }
                else
                {
                    latestLine = lineBox;
                    break;
                }
            }
            return latestLine;
        }


        //======================================================================================
        class LineWalkVisitor
        {
            readonly CssBlockRun startBlockRun;
            readonly CssLineBox startLineBox;

            public float globalX;
            public float globalY;


            CssLineBox currentVisitLineBox;

            float targetX;
            float targetY;

            public LineWalkVisitor(CssLineBox startLineBox)
            {
                this.startLineBox = startLineBox;

                float endElemX = 0, endElemY = 0;
                startLineBox.OwnerBox.GetElementGlobalLocation(out endElemX, out endElemY);
                this.globalX = endElemX;
                this.globalY = endElemY + startLineBox.CachedLineTop;
            }
            public LineWalkVisitor(CssBlockRun startBlockRun)
            {
                float endElemX = 0, endElemY = 0;
                startBlockRun.ContentBox.GetElementGlobalLocation(out endElemX, out endElemY);
                this.globalX = endElemX;
                this.globalY = endElemY;
                this.startBlockRun = startBlockRun;
            }
            public void SetWalkTargetPosition(float x, float y)
            {
                this.targetX = x;
                this.targetY = y;
            }
            public void Walk(CssLineBox endLineBox, VisitLineDelegate del)
            {
                //2 cases , 

                IEnumerable<CssLineBox> lineIter =
                    (startBlockRun != null) ?
                        GetLineWalkDownIter(this, startBlockRun.ContentBox) :
                        GetLineWalkDownAndUpIter(this, startLineBox);

                foreach (var ln in lineIter)
                {
                    this.currentVisitLineBox = ln;
                    if (ln == endLineBox)
                    {
                        del(LineCoverage.EndLine, ln);
                        //found endline 
                        return;
                    }
                    else if (this.IsWalkTargetInCurrentLineArea())
                    {
                        del(LineCoverage.PartialLine, ln);
                    }
                    else
                    {
                        del(LineCoverage.FullLine, ln);
                    }
                }
            }
            public bool IsWalkTargetInCurrentLineArea()
            {
                return targetY >= this.globalY &&
                        targetY < this.globalY + currentVisitLineBox.CacheLineHeight &&
                        targetX >= this.globalX &&
                        targetX < this.globalX + currentVisitLineBox.CachedLineContentWidth;
            }
            /// walk down and up
            /// </summary>
            /// <param name="startLine"></param>
            /// <returns></returns>
            static IEnumerable<CssLineBox> GetLineWalkDownAndUpIter(LineWalkVisitor visitor, CssLineBox startLine)
            {

                float sx, sy;
                startLine.OwnerBox.GetElementGlobalLocation(out sx, out sy);
                CssLineBox curLine = startLine;
                //walk up and down the tree
                CssLineBox nextline = curLine.NextLine;
                while (nextline != null)
                {
                    visitor.globalY = sy + startLine.CachedLineTop;
                    yield return nextline;
                    nextline = nextline.NextLine;
                }
                //--------------------
                //no next line 
                //then walk up  ***
                CssBox curBox = startLine.OwnerBox;

            RETRY:
                CssBox level1Sibling = BoxHitUtils.GetNextSibling(curBox);

                while (level1Sibling != null)
                {

                    level1Sibling.GetElementGlobalLocation(out sx, out sy);
                    visitor.globalY = sy;

                    //walk down
                    foreach (var line in GetLineWalkDownIter(visitor, level1Sibling))
                    {
                        yield return line;
                    }

                    level1Sibling = BoxHitUtils.GetNextSibling(level1Sibling);
                }
                //--------------------
                //other further sibling
                //then step to parent of lineOwner
                if (curBox.ParentBox != null)
                {
                    //if has parent    
                    //walk up***
                    curBox = curBox.ParentBox;
                    goto RETRY;
                }
            }
        }

        delegate void VisitLineDelegate(LineCoverage lineCoverage, CssLineBox linebox);

        enum LineCoverage
        {
            EndLine,
            FullLine,
            PartialLine
        }

        static IEnumerable<CssLineBox> GetLineWalkDownIter(LineWalkVisitor visitor, CssBox box)
        {
            //recursive
            float y = visitor.globalY;

            if (box.LineBoxCount > 0)
            {
                foreach (var linebox in box.GetLineBoxIter())
                {
                    visitor.globalY = y + linebox.CachedLineTop;
                    yield return linebox;
                }
            }
            else
            {
                //element based
                foreach (var childbox in box.GetChildBoxIter())
                {
                    visitor.globalY = y + childbox.LocalY;

                    //recursive
                    foreach (var linebox in GetLineWalkDownIter(visitor, childbox))
                    {
                        yield return linebox;
                    }
                }
            }

            visitor.globalY = y;
        }

    }

    static class CssLineBoxExtension
    {
        public static void SelectFull(this CssLineBox lineBox)
        {
            //full line selection 
            lineBox.LineSelectionStart = 0;
            lineBox.LineSelectionWidth = (int)lineBox.CachedLineContentWidth;
        }
        public static void SelectPartialStart(this CssLineBox lineBox, int startAt)
        {
            //from startAt to end of line
            lineBox.LineSelectionStart = startAt;
            lineBox.LineSelectionWidth = (int)lineBox.CachedLineContentWidth - startAt;
        }
        public static void SelectPartialEnd(this CssLineBox lineBox, int endAt)
        {
            //from start of line to endAt              
            lineBox.LineSelectionStart = 0;
            lineBox.LineSelectionWidth = endAt;
        }
        public static void Select(this CssLineBox lineBox, int startAt, int endAt)
        {
            //from start of line to endAt
            lineBox.LineSelectionStart = startAt;
            lineBox.LineSelectionWidth = endAt - startAt;
        }
    }

}