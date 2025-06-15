using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace LanderGame
{
    public class TitleScreen
    {
        public void Draw(Graphics g, int clientWidth, int clientHeight, System.Collections.Generic.IReadOnlyList<PointF> stars)
        {
            // Draw star field for title screen
            foreach (var star in stars)
                g.FillRectangle(Brushes.White, star.X, star.Y, 2, 2);

            // Draw moon surface at bottom
            DrawMoonSurface(g, clientWidth, clientHeight);

            // Draw title
            using (var titleFont = new Font("Arial", 48, FontStyle.Bold))
            using (var subtitleFont = new Font("Arial", 24))
            {
                var titleText = "Lunar Invasion";
                var subtitleText = "Press any key to play";

                var titleSize = g.MeasureString(titleText, titleFont);
                var subtitleSize = g.MeasureString(subtitleText, subtitleFont);

                var centerX = clientWidth / 2f;
                var centerY = clientHeight / 2f;

                // Draw title
                g.DrawString(titleText, titleFont, Brushes.White,
                    centerX - titleSize.Width / 2, centerY - titleSize.Height / 2 - 50);

                // Draw subtitle
                g.DrawString(subtitleText, subtitleFont, Brushes.Gray,
                    centerX - subtitleSize.Width / 2, centerY - subtitleSize.Height / 2 + 50);
            }
        }

        /// <summary>
        /// Draws a curved moon surface at the bottom of the title screen with craters
        /// </summary>
        private void DrawMoonSurface(Graphics g, int clientWidth, int clientHeight)
        {
            var surfaceHeight = clientHeight / 4; // Moon surface takes up bottom quarter
            var surfaceBaseY = clientHeight - surfaceHeight;
            // Create points for curved surface
            var curvePoints = new List<PointF>();
            var numPoints = 50; // Number of points to create smooth curve
            
            for (int i = 0; i <= numPoints; i++)
            {
                var x = (float)(i * clientWidth) / numPoints;
                // Create a gentle curve that rises in the middle (inverted sine)
                var curveOffset = (float)(Math.Sin((double)i / numPoints * Math.PI) * surfaceHeight * 0.4); // 40% of surface height
                var y = clientHeight - curveOffset; // Start from bottom and rise up
                curvePoints.Add(new PointF(x, y));
            }
            
            // Add bottom corners to close the shape
            curvePoints.Add(new PointF(clientWidth, clientHeight));
            curvePoints.Add(new PointF(0, clientHeight));

            // Fill the moon area with black to hide stars behind it
            using (var blackBrush = new SolidBrush(Color.Black))
            {
                g.FillPolygon(blackBrush, curvePoints.ToArray());
            }

            // Add some craters as outlines only (no fill)
            using (var craterPen = new Pen(Color.White, 0.4f))
            {
                // Large crater on the left (wide ellipse)
                var crater1X = clientWidth * 0.2f;
                var crater1Y = GetSurfaceYAt(crater1X, curvePoints) + 30;
                var crater1Width = 50;
                var crater1Height = 16; // Even flatter - reduced from 20
                g.DrawEllipse(craterPen, crater1X - crater1Width/2, crater1Y - crater1Height/2, crater1Width, crater1Height);
                
                // Medium crater in the center (wide ellipse on the peak)
                var crater2X = clientWidth * 0.5f;
                var crater2Y = GetSurfaceYAt(crater2X, curvePoints) + 20;
                var crater2Width = 35;
                var crater2Height = 11; // Even flatter - reduced from 14
                g.DrawEllipse(craterPen, crater2X - crater2Width/2, crater2Y - crater2Height/2, crater2Width, crater2Height);
                
                // Small crater on the right (wide ellipse)
                var crater3X = clientWidth * 0.8f;
                var crater3Y = GetSurfaceYAt(crater3X, curvePoints) + 25;
                var crater3Width = 22;
                var crater3Height = 6; // Even flatter - reduced from 8
                g.DrawEllipse(craterPen, crater3X - crater3Width/2, crater3Y - crater3Height/2, crater3Width, crater3Height);
                
                // Additional small craters for detail
                var crater4X = clientWidth * 0.3f;
                var crater4Y = GetSurfaceYAt(crater4X, curvePoints) + 35;
                var crater4Width = 18;
                var crater4Height = 5; // Even flatter - reduced from 7
                g.DrawEllipse(craterPen, crater4X - crater4Width/2, crater4Y - crater4Height/2, crater4Width, crater4Height);
                
                var crater5X = clientWidth * 0.7f;
                var crater5Y = GetSurfaceYAt(crater5X, curvePoints) + 15;
                var crater5Width = 25;
                var crater5Height = 8; // Even flatter - reduced from 10
                g.DrawEllipse(craterPen, crater5X - crater5Width/2, crater5Y - crater5Height/2, crater5Width, crater5Height);
            }
            
            // Add mountain ranges and valleys for surface texture
            DrawMountainRanges(g, curvePoints, clientWidth);
            
            // Add smaller surface features and valleys
            DrawSurfaceDetails(g, curvePoints, clientWidth, clientHeight);
        }
        
        /// <summary>
        /// Draws mountain ranges and ridges on the lunar surface as seen from space
        /// </summary>
        private void DrawMountainRanges(Graphics g, List<PointF> curvePoints, int clientWidth)
        {
            using (var mountainPen = new Pen(Color.Gray, 0.8f))
            {
                var random = new Random(123); // Fixed seed for consistent mountains
                
                // Draw several mountain ridge lines
                for (int range = 0; range < 5; range++)
                {
                    var ridgePoints = new List<PointF>();
                    var startX = clientWidth * (0.05f + range * 0.18f);
                    var endX = startX + clientWidth * (0.15f + random.NextSingle() * 0.1f);
                    
                    for (float x = startX; x <= endX && x <= clientWidth; x += 3)
                    {
                        var baseY = GetSurfaceYAt(x, curvePoints);
                        // Create mountain peaks using multiple sine waves for natural variation
                        var peakHeight = (float)(
                            Math.Sin((x - startX) / 25) * 6 +
                            Math.Sin((x - startX) / 12) * 3 +
                            Math.Sin((x - startX) / 8) * 2
                        );
                        var mountainY = baseY - Math.Abs(peakHeight) - 5; // Always above surface
                        ridgePoints.Add(new PointF(x, mountainY));
                    }
                    
                    if (ridgePoints.Count > 1)
                    {
                        g.DrawLines(mountainPen, ridgePoints.ToArray());
                    }
                }
            }
            
            // Add secondary ridge lines for depth
            using (var secondaryPen = new Pen(Color.DarkGray, 0.5f))
            {
                var random = new Random(456); // Different seed for secondary features
                
                for (int range = 0; range < 8; range++)
                {
                    var ridgePoints = new List<PointF>();
                    var startX = clientWidth * (0.02f + range * 0.12f);
                    var endX = startX + clientWidth * (0.08f + random.NextSingle() * 0.06f);
                    
                    for (float x = startX; x <= endX && x <= clientWidth; x += 4)
                    {
                        var baseY = GetSurfaceYAt(x, curvePoints);
                        var ridgeHeight = (float)(Math.Sin((x - startX) / 15) * 3 + Math.Sin((x - startX) / 6) * 1.5);
                        var ridgeY = baseY - Math.Abs(ridgeHeight) - 2;
                        ridgePoints.Add(new PointF(x, ridgeY));
                    }
                    
                    if (ridgePoints.Count > 1)
                    {
                        g.DrawLines(secondaryPen, ridgePoints.ToArray());
                    }
                }
            }
        }
        
        /// <summary>
        /// Adds smaller surface details like valleys, small hills, and surface texture
        /// </summary>
        private void DrawSurfaceDetails(Graphics g, List<PointF> curvePoints, int clientWidth, int clientHeight)
        {
            var random = new Random(789); // Fixed seed for consistent details
            
            // Add valley lines (appear as darker indentations)
            using (var valleyPen = new Pen(Color.DimGray, 0.6f))
            {
                for (int valley = 0; valley < 6; valley++)
                {
                    var valleyPoints = new List<PointF>();
                    var startX = clientWidth * (0.1f + valley * 0.15f);
                    var endX = startX + clientWidth * (0.08f + random.NextSingle() * 0.05f);
                    
                    for (float x = startX; x <= endX && x <= clientWidth; x += 2)
                    {
                        var baseY = GetSurfaceYAt(x, curvePoints);
                        var valleyDepth = (float)(Math.Sin((x - startX) / 20) * 2 + Math.Sin((x - startX) / 8) * 1);
                        var valleyY = baseY + Math.Abs(valleyDepth) + 3; // Below surface
                        valleyPoints.Add(new PointF(x, valleyY));
                    }
                    
                    if (valleyPoints.Count > 1)
                    {
                        g.DrawLines(valleyPen, valleyPoints.ToArray());
                    }
                }
            }
            
            // Add small surface features (rocks, small hills)
            using (var featurePen = new Pen(Color.Gray, 0.7f))
            {
                for (int i = 0; i < 25; i++)
                {
                    var x = random.Next(0, clientWidth);
                    var baseY = GetSurfaceYAt(x, curvePoints);
                    var featureY = baseY + random.Next(8, 25);
                    
                    if (featureY < clientHeight)
                    {
                        var featureSize = random.Next(2, 8);
                        var featureHeight = random.Next(1, 4);
                        
                        // Draw small irregular features
                        for (int j = 0; j < featureSize; j++)
                        {
                            var featureX = x + j - featureSize / 2;
                            var featureYOffset = (float)(Math.Sin(j * 0.8) * featureHeight);
                            if (featureX >= 0 && featureX < clientWidth)
                            {
                                g.DrawLine(featurePen, featureX, featureY + featureYOffset, featureX, featureY + featureYOffset + 1);
                            }
                        }
                    }
                }
            }
            
            // Add subtle surface texture lines
            using (var texturePen = new Pen(Color.FromArgb(120, Color.Gray), 0.8f))
            {
                for (int i = 0; i < 15; i++)
                {
                    var startX = random.Next(0, clientWidth - 30);
                    var endX = startX + random.Next(10, 30);
                    var baseY = GetSurfaceYAt(startX, curvePoints);
                    var textureY = baseY + random.Next(5, 20);
                    
                    if (textureY < clientHeight && endX < clientWidth)
                    {
                        var endY = textureY + random.Next(-3, 3);
                        g.DrawLine(texturePen, startX, textureY, endX, endY);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to get the Y coordinate of the curved surface at a given X position
        /// </summary>
        private float GetSurfaceYAt(float x, List<PointF> curvePoints)
        {
            // Find the closest point in the curve
            var closestPoint = curvePoints.OrderBy(p => Math.Abs(p.X - x)).First();
            return closestPoint.Y;
        }
    }
}
