namespace SemaphoreApp
{
    partial class Form1
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
            this.listBoxCreated = new System.Windows.Forms.ListBox();
            this.listBoxWaiting = new System.Windows.Forms.ListBox();
            this.listBoxWorking = new System.Windows.Forms.ListBox();
            this.btnCreateThread = new System.Windows.Forms.Button();
            this.numericUpDownSlots = new System.Windows.Forms.NumericUpDown();
            this.lblCreated = new System.Windows.Forms.Label();
            this.lblWaiting = new System.Windows.Forms.Label();
            this.lblWorking = new System.Windows.Forms.Label();
            this.lblSlots = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSlots)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxCreated
            // 
            this.listBoxCreated.FormattingEnabled = true;
            this.listBoxCreated.ItemHeight = 15;
            this.listBoxCreated.Location = new System.Drawing.Point(12, 35);
            this.listBoxCreated.Name = "listBoxCreated";
            this.listBoxCreated.Size = new System.Drawing.Size(200, 349);
            this.listBoxCreated.TabIndex = 0;
            this.listBoxCreated.DoubleClick += new System.EventHandler(this.listBoxCreated_DoubleClick);
            // 
            // listBoxWaiting
            // 
            this.listBoxWaiting.FormattingEnabled = true;
            this.listBoxWaiting.ItemHeight = 15;
            this.listBoxWaiting.Location = new System.Drawing.Point(230, 35);
            this.listBoxWaiting.Name = "listBoxWaiting";
            this.listBoxWaiting.Size = new System.Drawing.Size(200, 349);
            this.listBoxWaiting.TabIndex = 1;
            // 
            // listBoxWorking
            // 
            this.listBoxWorking.FormattingEnabled = true;
            this.listBoxWorking.ItemHeight = 15;
            this.listBoxWorking.Location = new System.Drawing.Point(448, 35);
            this.listBoxWorking.Name = "listBoxWorking";
            this.listBoxWorking.Size = new System.Drawing.Size(200, 349);
            this.listBoxWorking.TabIndex = 2;
            this.listBoxWorking.DoubleClick += new System.EventHandler(this.listBoxWorking_DoubleClick);
            // 
            // btnCreateThread
            // 
            this.btnCreateThread.Location = new System.Drawing.Point(12, 400);
            this.btnCreateThread.Name = "btnCreateThread";
            this.btnCreateThread.Size = new System.Drawing.Size(200, 35);
            this.btnCreateThread.TabIndex = 3;
            this.btnCreateThread.Text = "Create Thread";
            this.btnCreateThread.UseVisualStyleBackColor = true;
            this.btnCreateThread.Click += new System.EventHandler(this.btnCreateThread_Click);
            // 
            // numericUpDownSlots
            // 
            this.numericUpDownSlots.Location = new System.Drawing.Point(230, 410);
            this.numericUpDownSlots.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSlots.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSlots.Name = "numericUpDownSlots";
            this.numericUpDownSlots.Size = new System.Drawing.Size(120, 23);
            this.numericUpDownSlots.TabIndex = 4;
            this.numericUpDownSlots.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownSlots.ValueChanged += new System.EventHandler(this.numericUpDownSlots_ValueChanged);
            // 
            // lblCreated
            // 
            this.lblCreated.AutoSize = true;
            this.lblCreated.Location = new System.Drawing.Point(12, 17);
            this.lblCreated.Name = "lblCreated";
            this.lblCreated.Size = new System.Drawing.Size(90, 15);
            this.lblCreated.TabIndex = 5;
            this.lblCreated.Text = "Created Threads";
            // 
            // lblWaiting
            // 
            this.lblWaiting.AutoSize = true;
            this.lblWaiting.Location = new System.Drawing.Point(230, 17);
            this.lblWaiting.Name = "lblWaiting";
            this.lblWaiting.Size = new System.Drawing.Size(90, 15);
            this.lblWaiting.TabIndex = 6;
            this.lblWaiting.Text = "Waiting Threads";
            // 
            // lblWorking
            // 
            this.lblWorking.AutoSize = true;
            this.lblWorking.Location = new System.Drawing.Point(448, 17);
            this.lblWorking.Name = "lblWorking";
            this.lblWorking.Size = new System.Drawing.Size(95, 15);
            this.lblWorking.TabIndex = 7;
            this.lblWorking.Text = "Working Threads";
            // 
            // lblSlots
            // 
            this.lblSlots.AutoSize = true;
            this.lblSlots.Location = new System.Drawing.Point(230, 392);
            this.lblSlots.Name = "lblSlots";
            this.lblSlots.Size = new System.Drawing.Size(95, 15);
            this.lblSlots.TabIndex = 8;
            this.lblSlots.Text = "Semaphore Slots:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 450);
            this.Controls.Add(this.lblSlots);
            this.Controls.Add(this.lblWorking);
            this.Controls.Add(this.lblWaiting);
            this.Controls.Add(this.lblCreated);
            this.Controls.Add(this.numericUpDownSlots);
            this.Controls.Add(this.btnCreateThread);
            this.Controls.Add(this.listBoxWorking);
            this.Controls.Add(this.listBoxWaiting);
            this.Controls.Add(this.listBoxCreated);
            this.Icon = new System.Drawing.Icon("app.ico");
            this.Name = "Form1";
            this.Text = "Semaphore Thread Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSlots)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox listBoxCreated;
        private System.Windows.Forms.ListBox listBoxWaiting;
        private System.Windows.Forms.ListBox listBoxWorking;
        private System.Windows.Forms.Button btnCreateThread;
        private System.Windows.Forms.NumericUpDown numericUpDownSlots;
        private System.Windows.Forms.Label lblCreated;
        private System.Windows.Forms.Label lblWaiting;
        private System.Windows.Forms.Label lblWorking;
        private System.Windows.Forms.Label lblSlots;
    }
}

