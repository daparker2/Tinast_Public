namespace DP.Tinast.LayoutTool
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class BoostLayout : ILayout
    {
        Point p1;
        Point p2;
        Point p3;
        int margin;
        int thickness;

        public BoostLayout(Point p1, Point p2, Point p3, int margin, int thickness)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.margin = margin;
            this.thickness = thickness;
        }

        public string GetXaml()
        {
            List<Region> regions = new List<Region>();
            this.LayoutVertical(regions.Add);
            this.LayoutHorizontal(regions.Add);

            StringBuilder ret = new StringBuilder();
            int idx = 0;
            foreach (Region r in regions)
            {
                ret.AppendFormat(@"<shapes:Polygon x:Name=""led{0}"" Stroke=""White"" StrokeThickness=""1"" Fill=""White"" Points=""{1} {2} {3} {4} {5} {6} {7} {8}"" />", idx++, (int)r.X0, (int)r.Y0, (int)r.X1, (int)r.Y1, (int)r.X2, (int)r.Y2, (int)r.X3, (int)r.Y3);
                ret.AppendLine();
            }

            return ret.ToString();
        }

        public string GetCs()
        {
            List<Region> regions = new List<Region>();
            this.LayoutVertical(regions.Add);
            this.LayoutHorizontal(regions.Add);

            StringBuilder ret = new StringBuilder();
            ret.AppendLine("Polygon[] allLeds = new Polygon[]");
            ret.AppendLine("{");
            for (int i = 0; i < regions.Count; ++i)
            {
                ret.AppendFormat("\tthis.led{0},", i);
                ret.AppendLine();
            }

            ret.AppendLine("};");
            return ret.ToString();
        }

        void LayoutVertical(Action<Region> nextRegion)
        {
            // Start from bottom to p2.height, adding thickness + margin height each time
            int y0 = p1.Y;
            while (y0 - this.thickness - this.margin >= p2.Y)
            {
                int y1 = y0 - this.thickness;
                int x0 = p1.X + (y0 - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y); 
                int x1 = p1.X + (y1 - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y);
                nextRegion(new Region(0, y0, x0, y0, x1, y1, 0, y1));
                y0 -= this.thickness + this.margin;
            }
        }

        void LayoutHorizontal(Action<Region> nextRegion)
        {
            // Start from bottom to p2.height, adding thickness + margin height each time
            int x0 = p2.X;
            while (x0 + this.thickness + this.margin <= p3.X)
            {
                int x1 = x0 + this.thickness;
                int y0 = p2.Y + (x0 - p2.X) * (p3.Y - p2.Y) / (p3.X - p2.X);
                int y1 = p2.Y + (x1 - p2.X) * (p3.Y - p2.Y) / (p3.X - p2.X);
                nextRegion(new Region(x0, 0, x1, 0, x1, y1, x0, y0));
                x0 += this.thickness + this.margin;
            }
        }

        class Region
        {
            public Region(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
            {
                this.X0 = x0;
                this.Y0 = y0;
                this.X1 = x1;
                this.Y1 = y1;
                this.X2 = x2;
                this.Y2 = y2;
                this.X3 = x3;
                this.Y3 = y3;
            }

            public int X0 { get; private set; }
            public int Y0 { get; private set; }
            public int X1 { get; private set; }
            public int Y1 { get; private set; }
            public int X2 { get; private set; }
            public int Y2 { get; private set; }
            public int X3 { get; private set; }
            public int Y3 { get; private set; }
        }
    }
}
