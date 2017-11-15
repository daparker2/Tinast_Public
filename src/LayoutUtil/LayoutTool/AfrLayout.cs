namespace DP.Tinast.LayoutTool
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class AfrLayout : ILayout
    {
        Point center;
        int radius;
        int thickness;
        int count;

        public AfrLayout(Point center, int radius, int thickness, int count)
        {
            this.center = center;
            this.radius = radius;
            this.thickness = thickness;
            this.count = count;
        }

        public string GetXaml()
        {
            List<Region> regions = new List<Region>();
            this.LayoutAfr(regions.Add);

            StringBuilder ret = new StringBuilder();
            int idx = 0;
            foreach (Region r in regions)
            {
                ret.AppendFormat(@"<shapes:Ellipse x:Name=""led{0}"" Width=""{3}"" Height=""{3}"" Stroke=""White"" Fill=""White"" StrokeThickness=""1"" Canvas.Top=""{1}"" Canvas.Left=""{2}"" />", idx++, r.Y, r.X, this.thickness);
                ret.AppendLine();
            }

            return ret.ToString();
        }

        public string GetCs()
        {
            List<Region> regions = new List<Region>();
            this.LayoutAfr(regions.Add);

            StringBuilder ret = new StringBuilder();
            ret.AppendLine("Circle[] allLeds = new Circle[]");
            ret.AppendLine("{");
            for (int i = 0; i < regions.Count; ++i)
            {
                ret.AppendFormat("\tthis.led{0},", i);
            }

            ret.AppendLine("};");
            return ret.ToString();
        }

        void LayoutAfr(Action<Region> nextRegion)
        {
            for (int i = 0; i < this.count; ++i)
            {
                int x = (int)(0.5 * (double)radius + 0.5 * (double)radius * Math.Cos(-(.052 + Math.PI / 2.0 + 2.0 * Math.PI * (double)i / (double)this.count)));
                int y = (int)(0.5 * (double)radius + 0.5 * (double)radius * Math.Sin(-(.052 + Math.PI / 2.0 + 2.0 * Math.PI * (double)i / (double)this.count)));
            }
        }

        class Region
        {
            public Region(int y, int x)
            {
                this.Y = y;
                this.X = x;
            }

            public int Y { get; private set; }
            public int X { get; private set; }
        }
    }
}
