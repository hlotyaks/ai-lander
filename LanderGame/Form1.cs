using System;
using System.Drawing;
using System.Windows.Forms;

namespace LanderGame
{
    public partial class Form1 : Form
    {
        // Object‐oriented game state
        private Lander lander;   // initialized in Form1_Load
        private LandingPad pad = null!;       // set in Form1_Load
        private bool thrusting, rotatingLeft, rotatingRight;
        private bool gameOver;
        private bool landedSuccess;
        private float gravity;
        private const float terrainHeight = 20f;
        private Terrain terrain; // initialized in Form1_Load
        private int terrainSegments = 40;
        private float terrainVariation = 500f;
        private float cameraX;
        private float scrollMargin = 500f;
        private List<(PointF start, PointF end, float vx, float vy)> debris = new();
        private const int blinkIntervalMs = 500;

        public Form1()
        {
            InitializeComponent();
            // Start in full screen
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            // Set initial gravity based on UI selection
            SetGravityFromSelection();
            // Hook up rendering
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
        }

        // Set gravity based on chosen environment
        private void SetGravityFromSelection()
        {
            switch (envComboBox.SelectedIndex)
            {
                case 0: gravity = 0.0005f; break; // Moon
                case 1: gravity = 0.001f; break;  // Earth
                case 2: gravity = 0.00037f; break; // Mars
            }
        }

        private void envComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGravityFromSelection();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Quit on 'X' after game over or successful landing
            if ((gameOver || landedSuccess) && e.KeyCode == Keys.X)
            {
                this.Close();
                return;
            }
            // Restart on 'R' after game over or successful landing
            if ((gameOver || landedSuccess) && e.KeyCode == Keys.R)
            {
                ResetGame();
                return;
            }
            // Start the game on first input
            if (!gameTimer.Enabled && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right))
                gameTimer.Start();

            if (e.KeyCode == Keys.Up) thrusting = true;
            if (e.KeyCode == Keys.Left) rotatingLeft = true;
            if (e.KeyCode == Keys.Right) rotatingRight = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) thrusting = false;
            if (e.KeyCode == Keys.Left) rotatingLeft = false;
            if (e.KeyCode == Keys.Right) rotatingRight = false;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // initialize lander and terrain now that ClientSize is set
            lander = new Lander(ClientSize.Width/2, 50);
            terrain = new Terrain(terrainSegments, terrainVariation, ClientSize.Height - terrainHeight);
             // Generate jagged terrain and instantiate landing pad
            var rng = new Random();
            // Build terrain
            float segW = ClientSize.Width / (float)terrainSegments;
            terrain.Generate(rng, ClientSize.Width, ClientSize.Height);
             // Choose pad segment count and location
             int padSegs = Math.Clamp((int)Math.Round(60f / segW), 1, terrainSegments);
             int startIdx = rng.Next(0, terrainSegments - padSegs + 1);
            // Flatten terrain under pad
            terrain.Flatten(startIdx, padSegs);
            int endIdx = startIdx + padSegs;
            float padY = (terrain.Points[startIdx].Y + terrain.Points[endIdx].Y) / 2;
            // Instantiate landing pad
            float padXCoord = startIdx * segW;
            float padW = padSegs * segW;
            pad = new LandingPad(padXCoord, padW, padY, 500);
            // Pause simulation until first input
            gameTimer.Stop();
        }

        private float GetTerrainYAt(float xPos)
        {
            return terrain.GetHeightAt(xPos, ClientSize.Width);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            // Delegate physics update to lander
            float delta = gameTimer.Interval;
            lander.Update(delta, thrusting, rotatingLeft, rotatingRight, gravity);
            float x = lander.X, y = lander.Y, vx = lander.Vx, vy = lander.Vy;
            // Terrain collision
            float wrap = ClientSize.Width; float modX = (x % wrap + wrap) % wrap;
            float terrainY = GetTerrainYAt(modX);
            if (!gameOver && !landedSuccess && y + 20 >= terrainY)
            {
                // Check pad exists before landing
                if (pad != null && Math.Abs(vx) <= 0.5f && Math.Abs(vy) <= 0.5f && modX >= pad.X && modX <= pad.X + pad.Width)
                 {
                     gameTimer.Stop(); landedSuccess = true; pad.StopBlinking();
                 }
                else
                {
                    gameOver = true;
                    var rng2 = new Random(); int pieces=30;
                    for(int i=0;i<pieces;i++) { var a2=(float)(rng2.NextDouble()*2*Math.PI);
                        var start=new PointF(x,y);
                        var end=new PointF(x+(float)Math.Cos(a2)*10,y+(float)Math.Sin(a2)*10);
                        debris.Add((start,end,(end.X-start.X)*0.1f,(end.Y-start.Y)*0.1f)); }
                }
            }
            // Camera follow
            if (x-cameraX<scrollMargin) cameraX=Math.Max(0,x-scrollMargin);
            else if (x-cameraX>ClientSize.Width-scrollMargin) cameraX=x-(ClientSize.Width-scrollMargin);
            Invalidate();

            // Update debris pieces
            if (debris.Count > 0)
            {
                for (int i = debris.Count - 1; i >= 0; i--)
                {
                    var (start, end, vx0, vy0) = debris[i];
                    // update positions
                    start.X += vx0 * delta;
                    start.Y += vy0 * delta;
                    end.X += vx0 * delta;
                    end.Y += vy0 * delta;
                    // apply gravity to vy
                    vy0 += gravity * delta;
                    debris[i] = (start, end, vx0, vy0);
                }
            }
        }  // end of gameTimer_Tick

        // Reset game state for new play
        private void ResetGame()
        {
            lander.Reset(ClientSize.Width/2,50);
            thrusting=rotatingLeft=rotatingRight=false;
            gameOver=landedSuccess=false; cameraX=0f; debris.Clear();
            // regenerate terrain and pad
            var rng=new Random(); float segW=ClientSize.Width/terrainSegments;
            int padSegs=Math.Clamp(1,(int)Math.Round(60/segW),terrainSegments);
            int start=rng.Next(0,terrainSegments-padSegs+1);
            terrain.Generate(rng, ClientSize.Width, ClientSize.Height);
            terrain.Flatten(start, padSegs);
            int endIdx2=start+padSegs; float padY2=(terrain.Points[start].Y+terrain.Points[endIdx2].Y)/2;
            for(int i=start;i<=endIdx2;i++)terrain.Points[i].Y=padY2;
            pad=new LandingPad(start*segW,padSegs*segW,padY2,blinkIntervalMs);
            SetGravityFromSelection();
            gameTimer.Stop();
            // Force redraw so screen resets immediately
            Invalidate();
        }
        
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            // Apply camera offset for world rendering
            g.ResetTransform();
            g.TranslateTransform(-cameraX, 0);
            float wrapWidth = ClientSize.Width; // width used for terrain and pad tiling

            // Draw terrain
            terrain.Draw(g, cameraX, wrapWidth);

            // Draw landing pad if initialized
            if (pad != null)
                pad.Draw(g);

            // Draw debris explosion on crash
            if (gameOver && debris.Count > 0)
            {
                using var debrisPen = new Pen(Color.Orange, 2);
                foreach (var (start, end, _, _) in debris)
                    g.DrawLine(debrisPen, start, end);
            }

            // Draw lander
            if(!gameOver) lander.Draw(g,thrusting);

            // Reset transform for HUD (screen space)
            g.ResetTransform();

            // HUD: velocity, altitude, fuel next to ship (only when not crashed)
            if (!gameOver)
            {
                float craftHalfW = 10f, craftHalfH = 20f, hudMargin = 10f;
                float screenX = lander.X - cameraX, screenY = lander.Y;
                float hudX = screenX + craftHalfW + hudMargin;
                float hudY = screenY - craftHalfH;
                var velText = $"Vx:{lander.Vx:0.00} Vy:{lander.Vy:0.00}";
                g.DrawString(velText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                var altText = $"Alt:{ClientSize.Height - lander.Y:0.00}";
                g.DrawString(altText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                var fuelText = $"Fuel:{lander.Fuel:0.00}";
                g.DrawString(fuelText, this.Font, Brushes.White, hudX, hudY);
            }

            // Draw game over or success message
            if (gameOver)
            {
                using (var font = new Font("Arial", 24))
                using (var brush = new SolidBrush(Color.Red))
                {
                    g.DrawString("Game Over", font, brush, ClientSize.Width / 2 - 60, ClientSize.Height / 2 - 30);
                    // Restart and quit prompts
                    g.DrawString("Press R to restart", font, Brushes.White,
                        ClientSize.Width / 2 - 100, ClientSize.Height / 2);
                    g.DrawString("Press X to quit", font, Brushes.White,
                        ClientSize.Width / 2 - 80, ClientSize.Height / 2 + 30);
                }
            }
            else if (landedSuccess)
            {
                using (var font = new Font("Arial", 24))
                using (var brush = new SolidBrush(Color.Lime))
                {
                    g.DrawString("Landed Successfully!", font, brush, ClientSize.Width / 2 - 120, ClientSize.Height / 2 - 30);
                    // Restart and quit prompts
                    g.DrawString("Press R to restart", font, Brushes.White, ClientSize.Width / 2 - 100, ClientSize.Height / 2);
                    g.DrawString("Press X to quit", font, Brushes.White, ClientSize.Width / 2 - 80, ClientSize.Height / 2 + 30);
                }
            }
        } // end Form1_Paint
    }
}
