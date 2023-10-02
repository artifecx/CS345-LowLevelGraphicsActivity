namespace BenigaLowLevelGraphics
{
    partial class PongSquared
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PongSquared
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(920, 616);
            this.DoubleBuffered = true;
            this.Name = "PongSquared";
            this.Text = "Pong^2";
            this.ResizeBegin += new System.EventHandler(this.PongSquared_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.PongSquared_ResizeEnd);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PongSquared_Paint);
            this.Resize += new System.EventHandler(this.PongSquared_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

