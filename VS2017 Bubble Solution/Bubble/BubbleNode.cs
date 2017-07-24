using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace root9B_Bubble_Challenge
{
    class BubbleNode
    {
        public float radius;
        public double mass;
        public System.Windows.Vector center;
        public System.Windows.Media.Color labelColor;
        public System.Windows.Media.Color nodeColor;
        public System.Windows.Media.Color outlineColor;
        public System.Windows.Controls.Border shape;
        public System.Windows.Controls.TextBlock label;

        // Random generator seeded with unique value
        private Random rnd = new Random(Guid.NewGuid().GetHashCode());

        public void randomColors()
        {
            // Node Colors
            byte r = Convert.ToByte(rnd.Next(0, 256));
            byte g = Convert.ToByte(rnd.Next(0, 256));
            byte b = Convert.ToByte(rnd.Next(0, 256));

            nodeColor = System.Windows.Media.Color.FromArgb(r, g, b, 0);

            // Outline Color
            if ((r + g + b) / 3 > 128)
            {
                // Shade (Darken)
                outlineColor = System.Windows.Media.Color.FromArgb(
                    Convert.ToByte(Convert.ToInt32(Math.Round(r * .75, 0))),
                    Convert.ToByte(Convert.ToInt32(Math.Round(g * .75, 0))),
                    Convert.ToByte(Convert.ToInt32(Math.Round(b * .75, 0))),
                    0
                );

                // Label Color
                labelColor = System.Windows.Media.Color.FromRgb(10, 10, 10);
            }
            else
            {
                // Tint (Brighten)
                outlineColor = System.Windows.Media.Color.FromArgb(
                    Convert.ToByte(Convert.ToInt32(Math.Round(r + (255 - r) * .25, 0))),
                    Convert.ToByte(Convert.ToInt32(Math.Round(g + (255 - g) * .25, 0))),
                    Convert.ToByte(Convert.ToInt32(Math.Round(b + (255 - b) * .25, 0))),
                    0
                );

                // Label Color
                labelColor = System.Windows.Media.Color.FromRgb(245, 245, 245);
            }
        }
    }
}
