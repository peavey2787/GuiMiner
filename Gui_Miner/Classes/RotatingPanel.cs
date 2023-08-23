namespace Gui_Miner.Classes
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using static System.Net.Mime.MediaTypeNames;
    using Image = System.Drawing.Image;

    public class RotatingPanel : Panel
    {
        CancellationTokenSource cts;
        private double rotationAngle;
        public Image Image { get; set; }
        public double RotationAngle
        {
            get { return rotationAngle; }
            set
            {
                rotationAngle = value;
                Invalidate(); // Trigger a repaint when the rotation angle changes
            }
        }

        public RotatingPanel()
        {
            DoubleBuffered = true; // Enable double buffering
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Image == null) return;

            // Rotate the image and draw it on the panel
            using (Image rotatedImage = RotateImage(Image, (float)rotationAngle, ClientRectangle.Size))
            {
                e.Graphics.DrawImage(rotatedImage, Point.Empty);
            }
        }

        // Function to rotate and resize an image by a specified angle while maintaining aspect ratio
        private Image RotateImage(Image image, float angle, Size newSize)
        {            
            // Calculate the new dimensions while maintaining the aspect ratio
            int newWidth, newHeight;
            float aspectRatio = (float)image.Width / image.Height;

            if (newSize.Width / aspectRatio <= newSize.Height)
            {
                // Lock to height and calculate width based on aspect ratio
                newHeight = newSize.Height;
                newWidth = (int)(newHeight * aspectRatio);
            }
            else
            {
                // Lock to width and calculate height based on aspect ratio
                newWidth = newSize.Width;
                newHeight = (int)(newWidth / aspectRatio);
            }

            Bitmap rotatedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias; // Enable anti-aliasing
                g.TranslateTransform(newWidth / 2, newHeight / 2); // Set the rotation point at the center of the image
                g.RotateTransform(angle);
                g.TranslateTransform(-newWidth / 2, -newHeight / 2); // Reset the translation

                g.InterpolationMode = InterpolationMode.HighQualityBicubic; // Set interpolation mode

                g.DrawImage(image, new RectangleF(Point.Empty, new Size(newWidth, newHeight)), new Rectangle(Point.Empty, image.Size), GraphicsUnit.Pixel);
            }
            return rotatedImage;
        }

        public static RotatingPanel Create()
        {
            var rotatingPanel = new RotatingPanel();
            rotatingPanel.Location = new Point(0, 55);
            rotatingPanel.Size = new Size(429, 420);
            rotatingPanel.BackColor = Color.Transparent;
            rotatingPanel.Dock = DockStyle.Fill;
            rotatingPanel.Visible = true;
            return rotatingPanel;
        }

        public void Start()
        {
            // Start the rotation
            cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (true) // Run indefinitely
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    // Increment the rotation angle (adjust the speed by changing the increment)
                    RotationAngle += 0.5; // Adjust the angle increment for desired speed

                    // Apply the rotation
                    Invalidate();

                    // Sleep to control the rotation speed
                    await Task.Delay(50); // Adjust the interval for rotation speed (milliseconds)
                }
            }, cts.Token);
        }

        public void Stop()
        {
            // Stop the rotation
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose(); 
                cts = null; 
            }
        }
    }
}
