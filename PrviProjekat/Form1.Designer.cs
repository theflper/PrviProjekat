namespace PrviProjekat
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            undoToolStripMenuItem = new ToolStripMenuItem();
            redoToolStripMenuItem = new ToolStripMenuItem();
            pictureBoxTeren = new PictureBox();
            trackBarN = new TrackBar();
            trackBarM = new TrackBar();
            labelN = new Label();
            labelM = new Label();
            label1 = new Label();
            TerenBtn = new Button();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTeren).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarN).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarM).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(832, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveToolStripMenuItem, saveAsToolStripMenuItem, openToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(44, 24);
            fileToolStripMenuItem.Text = "file";
            fileToolStripMenuItem.Click += fileToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(139, 26);
            saveToolStripMenuItem.Text = "save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(139, 26);
            saveAsToolStripMenuItem.Text = "save as";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(139, 26);
            openToolStripMenuItem.Text = "open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(49, 24);
            editToolStripMenuItem.Text = "edit";
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            undoToolStripMenuItem.Size = new Size(224, 26);
            undoToolStripMenuItem.Text = "undo";
            undoToolStripMenuItem.Click += undoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
            redoToolStripMenuItem.Size = new Size(224, 26);
            redoToolStripMenuItem.Text = "redo";
            redoToolStripMenuItem.Click += redoToolStripMenuItem_Click;
            // 
            // pictureBoxTeren
            // 
            pictureBoxTeren.Location = new System.Drawing.Point(12, 60);
            pictureBoxTeren.Name = "pictureBoxTeren";
            pictureBoxTeren.Size = new Size(500, 500);
            pictureBoxTeren.TabIndex = 1;
            pictureBoxTeren.TabStop = false;
            pictureBoxTeren.Paint += pictureBoxTeren_Paint;
            pictureBoxTeren.MouseClick += pictureBoxTeren_MouseClick;
            // 
            // trackBarN
            // 
            trackBarN.LargeChange = 1;
            trackBarN.Location = new System.Drawing.Point(565, 106);
            trackBarN.Maximum = 25;
            trackBarN.Minimum = 4;
            trackBarN.Name = "trackBarN";
            trackBarN.Size = new Size(130, 56);
            trackBarN.TabIndex = 2;
            trackBarN.Value = 4;
            trackBarN.Scroll += trackBarN_Scroll;
            // 
            // trackBarM
            // 
            trackBarM.LargeChange = 1;
            trackBarM.Location = new System.Drawing.Point(565, 194);
            trackBarM.Maximum = 25;
            trackBarM.Minimum = 4;
            trackBarM.Name = "trackBarM";
            trackBarM.Size = new Size(130, 56);
            trackBarM.TabIndex = 3;
            trackBarM.Value = 4;
            trackBarM.Scroll += trackBarM_Scroll;
            // 
            // labelN
            // 
            labelN.AutoSize = true;
            labelN.Location = new System.Drawing.Point(733, 119);
            labelN.Name = "labelN";
            labelN.Size = new Size(47, 20);
            labelN.TabIndex = 4;
            labelN.Text = "value:";
            // 
            // labelM
            // 
            labelM.AutoSize = true;
            labelM.Location = new System.Drawing.Point(733, 194);
            labelM.Name = "labelM";
            labelM.Size = new Size(47, 20);
            labelM.TabIndex = 5;
            labelM.Text = "value:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(577, 60);
            label1.Name = "label1";
            label1.Size = new Size(230, 20);
            label1.TabIndex = 6;
            label1.Text = "Odaberite dimenzije grid-a M i N";
            label1.Click += label1_Click;
            // 
            // TerenBtn
            // 
            TerenBtn.Location = new System.Drawing.Point(672, 256);
            TerenBtn.Name = "TerenBtn";
            TerenBtn.Size = new Size(108, 43);
            TerenBtn.TabIndex = 7;
            TerenBtn.Text = "Kreiraj teren";
            TerenBtn.UseVisualStyleBackColor = true;
            TerenBtn.Click += TerenBtn_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(832, 603);
            Controls.Add(TerenBtn);
            Controls.Add(label1);
            Controls.Add(labelM);
            Controls.Add(labelN);
            Controls.Add(trackBarM);
            Controls.Add(trackBarN);
            Controls.Add(pictureBoxTeren);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTeren).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarN).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarM).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private PictureBox pictureBoxTeren;
        private TrackBar trackBarN;
        private TrackBar trackBarM;
        private Label labelN;
        private Label labelM;
        private Label label1;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private Button TerenBtn;
    }
}
