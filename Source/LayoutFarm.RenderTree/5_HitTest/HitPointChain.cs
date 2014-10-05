﻿//2014 Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;

namespace LayoutFarm
{
    public struct HitPoint
    {
        public Point point;
        public RenderElement elem;
        public object externalObject;

        public static readonly HitPoint Empty = new HitPoint();

        public HitPoint(RenderElement elem, Point point)
        {
            this.point = point;
            this.elem = elem;
            this.externalObject = null;
        }
        public HitPoint(object externalObject, Point point)
        {
            this.point = point;
            this.elem = null;
            this.externalObject = externalObject;
        }

        public static bool operator ==(HitPoint pair1, HitPoint pair2)
        {
            return ((pair1.elem == pair2.elem) && (pair1.point == pair2.point));
        }
        public static bool operator !=(HitPoint pair1, HitPoint pair2)
        {
            return ((pair1.elem == pair2.elem) && (pair1.point == pair2.point));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

#if DEBUG
        public override string ToString()
        {
            return elem.ToString();
        }
#endif
    }



    public abstract class HitChain
    {

        protected int globalOffsetX = 0;
        protected int globalOffsetY = 0;

        int startTestX;
        int startTestY;

        protected int testPointX;
        protected int testPointY;

        public HitChain()
        {

        }
        public Point TestPoint
        {
            get
            {
                return new Point(testPointX, testPointY);
            }
        }
        public void GetTestPoint(out int x, out int y)
        {
            x = this.testPointX;
            y = this.testPointY;
        }
        public void SetStartTestPoint(int x, int y)
        {

            testPointX = x;
            testPointY = y;
            startTestX = x;
            startTestY = y;
        }
        public int LastestRootX
        {
            get
            {
                return startTestX;
            }
        }
        public int LastestRootY
        {
            get
            {
                return startTestY;
            }
        }
        public void OffsetTestPoint(int dx, int dy)
        {
            globalOffsetX += dx;
            globalOffsetY += dy;
            testPointX += dx;
            testPointY += dy;
        }
        public void ClearAll()
        {
            globalOffsetX = 0;
            globalOffsetY = 0;
            testPointX = 0;
            testPointY = 0;
            OnClearAll();

        }
        protected abstract void OnClearAll();

        public abstract int Count { get; }
        public abstract HitPoint GetHitPoint(int index);

        public abstract Point PrevHitPoint { get; }
        public abstract RenderElement CurrentHitElement { get; }
        public abstract Point CurrentHitPoint { get; }

        public abstract void AddHit(RenderElement hitElement);
        public abstract void AddExternalHitObject(object hitObject);
        public abstract void RemoveCurrentHitNode();

        public int LastestElementGlobalX
        {
            get
            {
                return globalOffsetX;
            }
        }
        public int LastestElementGlobalY
        {
            get
            {
                return globalOffsetY;
            }
        }
        //-----------------------------------------------------
        //element dragging feature , plan move to another place ?
        public abstract void ClearDragHitElements();
        public abstract void AddDragHitElement(RenderElement element);
        public abstract void RemoveDragHitElement(RenderElement element);
        public abstract IEnumerable<RenderElement> GetDragHitElementIter();
        public abstract int DragHitElementCount { get; }
       

#if DEBUG
        public bool dbugBreak;
#endif

    }

}
