using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;


namespace root9B_Bubble_Challenge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Class Variables
        private double zoomScale = 1.1;
        private System.Windows.Point prvMousePos;

        private bool dragging = false;

        private int fontSize = 10;
        private int nodePadding = 0;
        private int nodeMargin = 0;
        private int outlineThickness = 3;
        private int distGrowth = 1;
        
        private List<BubbleNode> nodes = new List<BubbleNode>();

        public MainWindow()
        {
            InitializeComponent();
        }

        public delegate void UpdateProgressBarCallback(int percentage, bool visible);
        public delegate void UpdateNodeCanvasCallback();

        private void UpdateProgressBar(int percentage, bool visible)
        {
            statusProgressBar.Value = percentage;
            if(visible)
            {
                statusProgressBar.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                statusProgressBar.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void UpdateNodeCanvas()
        {
            // Arrange the nodes on the screen
            foreach (BubbleNode node in nodes)
            {
                Canvas.SetTop(node.shape, node.center.Y);
                Canvas.SetLeft(node.shape, node.center.X);
            }
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        //Returns true if the circles are touching, or false if they are not
        private bool circlesColliding(BubbleNode b1, BubbleNode b2)
        {
            double distance = (b1.center - b2.center).Length;
            if (distance < b1.radius + b2.radius)
            {
                return true;
            }
            return false;
        }
        
        private void arrangeNodes()
        {
            // Check for nodes to arrange
            if(nodes.Count == 0)
            {
                return;
            }

            // Sort nodes largest -> smallest radius
            nodes = nodes.OrderByDescending(o => o.radius).ToList();

            // Status Message
            Console.WriteLine("[*] Canvas Size (" + nodeArea.ActualHeight + ", " + nodeArea.ActualWidth + ")");

            // Process each node, skipping the first (largest and root node)
            for (int curIndex = 1; curIndex < nodes.Count; curIndex += 1)
            {
                // Status Bar update
                float percentage = (float)(curIndex) / nodes.Count * 100;
                statusProgressBar.Dispatcher.Invoke(
                    new UpdateProgressBarCallback(this.UpdateProgressBar),
                    new object[] {(int)percentage, true}
                );

                // Current (minimum) distance needed from center
                float curDist = nodes[0].radius + nodes[curIndex].radius + nodePadding;
                
                // Search distance will keep expanding until spot is found
                bool searching = true;
                while (searching)
                {
                    nodes[curIndex].center.X = 0;
                    nodes[curIndex].center.Y = 0;

                    // Test all angles for empty spot
                    for (double angle = 0; angle < 360 && searching; angle += 0.1)
                    {
                        // Get Slope
                        nodes[curIndex].center.X = curDist * (float)Math.Cos(DegreeToRadian(angle));
                        nodes[curIndex].center.Y = curDist * (float)Math.Sin(DegreeToRadian(angle));
                        
                        // Collision detection with all other nodes
                        bool validSpot = true;
                        for (int chkIndex = 0; chkIndex < nodes.Count; chkIndex += 1)
                        {
                            // Skip over self
                            if (chkIndex == curIndex)
                            {
                                continue;
                            }

                            // Check collision
                            if (circlesColliding(nodes[curIndex], nodes[chkIndex]) == true)
                            {
                                validSpot = false;
                                break;
                            }
                        }

                        // Checked for any collision, all good
                        if (validSpot)
                        {
                            System.Console.WriteLine("[P] Placing Node " + curIndex + "(" + nodes[curIndex].center.X + ", " + nodes[curIndex].center.Y + ") R[" + nodes[curIndex].radius + "] D[" + curDist + "]");
                            searching = false;
                        }
                    }
                    curDist += distGrowth;
                }
                // Update the canvas
                nodeArea.Dispatcher.Invoke(
                    new UpdateNodeCanvasCallback(this.UpdateNodeCanvas)
                );
            }

            // Update and hide progress bar
            statusProgressBar.Dispatcher.Invoke(
                new UpdateProgressBarCallback(this.UpdateProgressBar),
                new object[] {100, false}
            );
        }

        private void loadNodeList(string fileName)
        {
            // Cleanup any old nodes
            nodes.Clear();

            // Total node area required to represent data
            double nodeAreaTotal = 0;

            // Read the file
            string[] filelines = System.IO.File.ReadAllLines(fileName);

            // Loaded file with no data
            if (filelines.Length < 1)
            {
                throw new System.MissingFieldException("Contains no network data.");
            }

            // Parse each line and create node
            for (int index = 0; index < filelines.Length; index++)
            {
                BubbleNode node = new BubbleNode();
                string[] lineparts = filelines[index].Split(',');

                // File input validation
                if (lineparts.Length != 2)
                {
                    throw new System.FormatException("Node " + (index + 1) + " should have exactly two fields.");
                }

                // Radius
                if (!float.TryParse(lineparts[0], out node.radius))
                {
                    throw new System.FormatException("Node " + (index + 1) + " radius field must be a number.");
                }

                // Label length
                if (lineparts[1].Length < 1)
                {
                    throw new System.FormatException("Node " + (index + 1) + " label field must contain text");
                }

                // Text
                node.label = new TextBlock();
                node.label.Text = lineparts[1].Trim();
                node.label.TextAlignment = System.Windows.TextAlignment.Center;
                node.label.VerticalAlignment = VerticalAlignment.Center;
                node.label.HorizontalAlignment = HorizontalAlignment.Center;
                node.label.FontFamily = new FontFamily("Helvetica");
                node.label.TextWrapping = TextWrapping.Wrap;
                node.label.FontSize = fontSize;
                
                // Location
                nodeAreaTotal += Math.PI * (node.radius * node.radius);
                node.center.X = 0;
                node.center.Y = 0;

                // Generate node colors
                node.randomColors();

                // Ellipse
                node.shape = new Border();
                node.shape.Width = node.radius * 2;
                node.shape.Height = node.radius * 2;
                node.shape.CornerRadius = new System.Windows.CornerRadius(node.radius);
                node.shape.Margin = new System.Windows.Thickness(nodeMargin);
                node.shape.Padding = new System.Windows.Thickness(nodePadding);
                node.shape.Background = new System.Windows.Media.SolidColorBrush(node.nodeColor);
                node.shape.BorderBrush = new System.Windows.Media.SolidColorBrush(node.outlineColor);
                node.shape.BorderThickness = new System.Windows.Thickness(outlineThickness);
                node.shape.Opacity = 100;
                node.shape.Child = node.label;

                // Move the origin of the shape from the corner to center
                node.shape.RenderTransformOrigin = new Point(0.5, 0.5);

                TranslateTransform translate = new TranslateTransform();
                translate.X = -node.radius;
                translate.Y = -node.radius;

                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(translate);
                node.shape.RenderTransform = transformGroup;
                
                // Add to node list
                nodes.Add(node);
                
                // Add the node shape to the scene
                nodeArea.Children.Add(node.shape);
            }

            // Arrange all nodes
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                arrangeNodes();
            }).Start();

            // Status Messages
            if (nodes.Count == 1)
            {
                statusTextarea.Text = nodes.Count + " node";
            }
            else
            {
                statusTextarea.Text = nodes.Count + " nodes";
            }
            statusFileName.Text = fileName;
        }

        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            System.IO.Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            openFileDialog1.InitialDirectory = homePath;
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            loadNodeList(openFileDialog1.FileName);
                            statusFileName.Text = openFileDialog1.FileName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Cleanup any old nodes
                    nodes.Clear();

                    // Show status messages
                    string usage = "\nRecords are formatted as such: <radius>,<label>\n" +
                        "Example:\n" +
                        "  20, mama\n" +
                        "  130, llama\n" +
                        "  42, drama";
                    MessageBox.Show("Error: " + ex.Message + usage);
                    statusTextarea.Text = "Error reading file";
                }
            }
        }

        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
              "Bubble Challenge v1.0\n\u00a9 2017 - Jeremiah Bergkvist All rights reserved.",
              "Bubble Challenge",
              MessageBoxButton.OK,
              MessageBoxImage.Asterisk
            );
        }

        private void showLabels_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (BubbleNode node in nodes)
            {
                node.shape.Child = null;
            }
        }

        private void showLabels_Checked(object sender, RoutedEventArgs e)
        {
            foreach (BubbleNode node in nodes)
            {
                node.shape.Child = node.label;
            }
        }

        // Dragging Started
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
            prvMousePos = e.GetPosition(drawArea);
        }

        // Dragging Ended
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                System.Windows.Point curMousePos = e.GetPosition(drawArea);
                foreach (BubbleNode node in nodes)
                {
                    node.center += curMousePos - prvMousePos;
                    Canvas.SetTop(node.shape, node.center.Y);
                    Canvas.SetLeft(node.shape, node.center.X);
                }
                prvMousePos = curMousePos;
            }
        }

        private void nodeArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                nodeAreaZoom.ScaleX *= zoomScale;
                nodeAreaZoom.ScaleY *= zoomScale;
            }
            else
            {
                nodeAreaZoom.ScaleX /= zoomScale;
                nodeAreaZoom.ScaleY /= zoomScale;
            }
        }
    }
}
