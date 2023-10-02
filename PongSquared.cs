using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BenigaLowLevelGraphics
{
    public partial class PongSquared : Form
    {
        private int leftScore = 0;
        private int rightScore = 0;

        private int platformSpeed = 10;
        private int platformHeight = 50;
        private int platformWidth = 10;

        private int leftPlatformPositionY;
        private int leftPlatformVelocity = 0;
        private int leftPlatformOffset = 100;

        private int rightPlatformPositionY;
        private int rightPlatformVelocity = 0;
        private int rightPlatformOffset = 100;

        private int ballSize = 20;
        private int ballX;
        private int ballY;
        private int ballSpeed = 5;
        private int ballDirectionX;
        private int ballDirectionY;

        private bool isPaused = false;
        private bool isManuallyPaused = false;
        private Timer timer;

        private PictureBox pausePlayButton;
        private PictureBox resetButton;

        public PongSquared()
        {
            InitializeComponent();

            this.Width = 1200;
            this.Height = 800;
            this.Paint += PongSquared_Paint;
            this.Resize += PongSquared_Resize;
            this.KeyDown += PongSquared_KeyDown;

            // Create buttons
            pausePlayButton = CreateButton(Properties.Resources.PlayIcon, PausePlayButton_MouseDown, 0.96, 0.02);
            resetButton = CreateButton(Properties.Resources.ResetIcon, ResetButton_MouseDown, 0.96, 0.10);

            // Set up initial positions of platforms
            leftPlatformPositionY = (this.ClientRectangle.Height - platformHeight) / 2;
            rightPlatformPositionY = (this.ClientRectangle.Height - platformHeight) / 2;

            // Set up the initial position and direction of the ball
            ResetBall();

            // Set up the timer for updating the platform position
            timer = new Timer { Interval = 16 };
            timer.Tick += UpdateGame;
            timer.Start();

            SetButtonPositionAndSize();
        }

        private PictureBox CreateButton(Image image, MouseEventHandler handler, double xRatio, double yRatio)
        {
            var button = new PictureBox
            {
                Image = image,
                Size = new Size(50, 50),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            button.MouseDown += handler;
            this.Controls.Add(button);

            button.Location = new Point((int)(this.Width * xRatio - button.Width), (int)(this.Height * yRatio));

            return button;
        }

        private void SetButtonPositionAndSize()
        {
            // Initial button positions and size
            if (pausePlayButton != null)
            {
                pausePlayButton.Location = new Point((int)(this.Width * 0.96 - pausePlayButton.Width), (int)(this.Height * 0.02));
            }

            if (resetButton != null)
            {
                resetButton.Location = new Point((int)(this.Width * 0.96 - resetButton.Width), (int)(this.Height * 0.10));
            }
        }

        private void PongSquared_Paint(object sender, PaintEventArgs e)
        {
            DrawCenterLine(e.Graphics);
            DrawScores(e.Graphics);
        }

        private void PongSquared_Resize(object sender, EventArgs e)
        {
            // Update the position of the button when the form is resized
            SetButtonPositionAndSize();

            // Minimum form size
            if (this.Width < 1000)
            {
                this.Width = 1000;
            }

            if (this.Height < 600)
            {
                this.Height = 600;
            }

            // Adjust the positions of the platforms and the ball
            leftPlatformPositionY = (this.ClientRectangle.Height - platformHeight) / 2;
            rightPlatformPositionY = (this.ClientRectangle.Height - platformHeight) / 2;

            // Calculate the new position of the ball based on the change in form dimensions
            float xRatio = (float)this.ClientRectangle.Width / this.ClientSize.Width;
            float yRatio = (float)this.ClientRectangle.Height / this.ClientSize.Height;

            ballX = (int)(ballX * xRatio);
            ballY = (int)(ballY * yRatio);

            // Ensure the ball stays within the form when resizing
            ballX = Math.Max(0, Math.Min(this.ClientRectangle.Width - ballSize, ballX));
            ballY = Math.Max(0, Math.Min(this.ClientRectangle.Height - ballSize, ballY));

            this.Invalidate();
        }

        private void DrawCenterLine(Graphics g)
        {
            int middleX = this.Width / 2;

            Pen dashedPen = new Pen(Color.White);
            dashedPen.Width = 4;
            dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            dashedPen.DashPattern = new float[] { 2f, 3f }; // { dash length, distance to next }

            g.DrawLine(dashedPen, new Point(middleX, 0), new Point(middleX, this.Height));
        }

        private void DrawScores(Graphics g)
        {
            // Adjusts font size on form size
            float fontSize = Math.Min(this.Width / 10, this.Height / 10); 

            Font counterFont = new Font("Agency FB", fontSize);
            Brush counterBrush = Brushes.White;

            int middleX = this.Width / 2;

            // Calculate font width to offset center
            int leftScoreWidth = (int)g.MeasureString(leftScore.ToString(), counterFont).Width;
            int rightScoreWidth = (int)g.MeasureString(rightScore.ToString(), counterFont).Width;

            // Calculate the middle point for the left and right counter
            int p1ScoreMiddleX = middleX - (middleX / 2) - leftScoreWidth / 2;
            int p2ScoreMiddleX = middleX + (middleX / 2) - rightScoreWidth / 2;

            // Draw the left and right counter centered to respective middle points
            g.DrawString(leftScore.ToString(), counterFont, counterBrush, new PointF(p1ScoreMiddleX, 20));
            g.DrawString(rightScore.ToString(), counterFont, counterBrush, new PointF(p2ScoreMiddleX, 20));
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            // If neither platform is moving, do not move ball
            bool isLeftPlatformMoving = leftPlatformVelocity != 0;
            bool isRightPlatformMoving = rightPlatformVelocity != 0;
            if (!isLeftPlatformMoving && !isRightPlatformMoving)
            {
                ResetBall();
                //return;
            }

            if (!isPaused)
            {
                // Update platform positions based on velocity
                leftPlatformPositionY += leftPlatformVelocity;
                rightPlatformPositionY += rightPlatformVelocity;

                // Bounce away from the top and bottom borders
                if (leftPlatformPositionY < 0 || leftPlatformPositionY > this.ClientRectangle.Height - platformHeight)
                {
                    leftPlatformVelocity = -leftPlatformVelocity;
                }
                if (rightPlatformPositionY < 0 || rightPlatformPositionY > this.ClientRectangle.Height - platformHeight)
                {
                    rightPlatformVelocity = -rightPlatformVelocity;
                }

                // Ensure the platforms stay within the form
                leftPlatformPositionY = Math.Max(0, Math.Min(this.ClientRectangle.Height - platformHeight, leftPlatformPositionY));
                rightPlatformPositionY = Math.Max(0, Math.Min(this.ClientRectangle.Height - platformHeight, rightPlatformPositionY));

                // Update ball position based on direction
                ballX += ballSpeed * ballDirectionX;
                ballY += ballSpeed * ballDirectionY;

                // Check collisions with platforms
                if (ballDirectionX == -1 && // Ball is moving towards the left platform
                    ballX <= 50 + platformWidth + leftPlatformOffset && // Ball is to the left of the left platform
                    ballX + ballSize >= 50 + leftPlatformOffset && // Ball is to the right of the left platform
                    ballY + ballSize >= leftPlatformPositionY && ballY <= leftPlatformPositionY + platformHeight) // Ball is within the vertical bounds of the left platform
                {
                    ballDirectionX = 1; // Bounce back
                }
                else if (ballDirectionX == 1 && // Ball is moving towards the right platform
                         ballX + ballSize >= this.ClientRectangle.Width - 50 - platformWidth - rightPlatformOffset && // Ball is to the right of the right platform
                         ballX <= this.ClientRectangle.Width - 50 - platformWidth - rightPlatformOffset && // Ball is to the left of the right platform
                         ballY + ballSize >= rightPlatformPositionY && ballY <= rightPlatformPositionY + platformHeight) // Ball is within the vertical bounds of the right platform
                {
                    ballDirectionX = -1; // Bounce back
                }
                else if (ballX <= 0 || ballX + ballSize >= this.ClientRectangle.Width) // Check if ball went out of bounds
                {
                    if (ballX < 0)
                    {
                        rightScore++;
                    }
                    else if (ballX + ballSize > this.ClientRectangle.Width)
                    {
                        leftScore++;
                    }

                    ResetBall();
                }
                else if (ballY <= 0 || ballY + ballSize >= this.ClientRectangle.Height) // Check collisions with top and bottom
                {
                    ballDirectionY = -ballDirectionY; // Bounce back
                }
            }

            // Update positions by redrawing the form
            this.Invalidate();
        }

        private void PausePlayButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPaused = !isPaused;
                isManuallyPaused = !isManuallyPaused;
                UpdatePausePlayButtonImage();
            }
        }

        private void ResetButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPaused = false;
                UpdatePausePlayButtonImage();

                leftScore = 0;
                rightScore = 0;

                ResetBall();
                ResetPlatforms();
            }
        }

        private void ResetBall()
        {
            ballX = this.ClientRectangle.Width / 2 - ballSize / 2;
            ballY = this.ClientRectangle.Height / 2 - ballSize / 2;

            Random random = new Random();
            ballDirectionX = (random.Next(2) == 0) ? -1 : 1; // Randomly set initial direction left or right
            ballDirectionY = (random.Next(2) == 0) ? -1 : 1; // Randomly set initial direction up or down
        }

        private void ResetPlatforms()
        {
            leftPlatformPositionY = (this.ClientRectangle.Height - platformHeight) / 2;
            rightPlatformPositionY = (this.ClientRectangle.Height - platformHeight) / 2;

            leftPlatformVelocity = 0;
            rightPlatformVelocity = 0;
        }

        private void UpdatePausePlayButtonImage()
        {
            if (isPaused)
            {
                pausePlayButton.Image = Properties.Resources.PauseIcon;
            }
            else
            {
                pausePlayButton.Image = Properties.Resources.PlayIcon;
            }
        }

        private void PongSquared_KeyDown(object sender, KeyEventArgs e)
        {
            // Ensures no funny business with the ball's position when paused
            if (isPaused) return;

            // Left platform controls
            if (e.KeyCode == Keys.W)
            {
                leftPlatformVelocity = -platformSpeed;
            }
            else if (e.KeyCode == Keys.S)
            {
                leftPlatformVelocity = platformSpeed;
            }

            // Right platform controls
            if (e.KeyCode == Keys.Up)
            {
                rightPlatformVelocity = -platformSpeed;
            }
            else if (e.KeyCode == Keys.Down)
            {
                rightPlatformVelocity = platformSpeed;
            }
        }

        private void PongSquared_ResizeBegin(object sender, EventArgs e)
        {
            isPaused = true;
            UpdatePausePlayButtonImage();
        }

        private void PongSquared_ResizeEnd(object sender, EventArgs e)
        {
            if (!isManuallyPaused)
            {
                isPaused = false;
                UpdatePausePlayButtonImage();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the left and right platforms
            e.Graphics.FillRectangle(Brushes.White, 50 + leftPlatformOffset, leftPlatformPositionY, platformWidth, platformHeight);
            e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle.Width - platformWidth - 50 - rightPlatformOffset, rightPlatformPositionY, platformWidth, platformHeight);

            // Draw the ball in the center if not moving
            if (leftPlatformVelocity == 0 && rightPlatformVelocity == 0)
            {
                int ballCenterX = (this.ClientRectangle.Width / 2) - (ballSize / 2) + 7; // +7 to align the ball with the dashed line
                int ballCenterY = (this.ClientRectangle.Height / 2) - (ballSize / 2);
                e.Graphics.FillEllipse(Brushes.White, ballCenterX, ballCenterY, ballSize, ballSize);
            }
            else
            {
                e.Graphics.FillEllipse(Brushes.White, ballX, ballY, ballSize, ballSize);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            timer.Dispose();
        }
    }
}
