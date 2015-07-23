namespace QvEventLogConnectorElaborate
{
    partial class Standalone
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Standalone));
            this.sqlStatementTextBox = new System.Windows.Forms.TextBox();
            this.targetPathBox = new System.Windows.Forms.GroupBox();
            this.setPathButton = new System.Windows.Forms.Button();
            this.sqlStatementBox = new System.Windows.Forms.GroupBox();
            this.generateQvxBox = new System.Windows.Forms.GroupBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.connectBox = new System.Windows.Forms.GroupBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.doneButton = new System.Windows.Forms.Button();
            this.targetPathBox.SuspendLayout();
            this.sqlStatementBox.SuspendLayout();
            this.generateQvxBox.SuspendLayout();
            this.connectBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sqlStatementTextBox
            // 
            this.sqlStatementTextBox.Location = new System.Drawing.Point(6, 19);
            this.sqlStatementTextBox.Multiline = true;
            this.sqlStatementTextBox.Name = "sqlStatementTextBox";
            this.sqlStatementTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sqlStatementTextBox.Size = new System.Drawing.Size(261, 181);
            this.sqlStatementTextBox.TabIndex = 1;
            // 
            // targetPathBox
            // 
            this.targetPathBox.Controls.Add(this.setPathButton);
            this.targetPathBox.Location = new System.Drawing.Point(12, 71);
            this.targetPathBox.Name = "targetPathBox";
            this.targetPathBox.Size = new System.Drawing.Size(273, 53);
            this.targetPathBox.TabIndex = 3;
            this.targetPathBox.TabStop = false;
            this.targetPathBox.Text = "2. Set QVX target path";
            // 
            // setPathButton
            // 
            this.setPathButton.Location = new System.Drawing.Point(75, 19);
            this.setPathButton.Name = "setPathButton";
            this.setPathButton.Size = new System.Drawing.Size(122, 23);
            this.setPathButton.TabIndex = 4;
            this.setPathButton.Text = "Set path";
            this.setPathButton.UseVisualStyleBackColor = true;
            this.setPathButton.Click += new System.EventHandler(this.setPathButton_Click);
            // 
            // sqlStatementBox
            // 
            this.sqlStatementBox.Controls.Add(this.sqlStatementTextBox);
            this.sqlStatementBox.Location = new System.Drawing.Point(12, 130);
            this.sqlStatementBox.Name = "sqlStatementBox";
            this.sqlStatementBox.Size = new System.Drawing.Size(273, 206);
            this.sqlStatementBox.TabIndex = 5;
            this.sqlStatementBox.TabStop = false;
            this.sqlStatementBox.Text = "3. Enter SQL statement";
            // 
            // generateQvxBox
            // 
            this.generateQvxBox.Controls.Add(this.generateButton);
            this.generateQvxBox.Location = new System.Drawing.Point(12, 342);
            this.generateQvxBox.Name = "generateQvxBox";
            this.generateQvxBox.Size = new System.Drawing.Size(273, 53);
            this.generateQvxBox.TabIndex = 5;
            this.generateQvxBox.TabStop = false;
            this.generateQvxBox.Text = "4. Generate QVX file";
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(75, 19);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(122, 23);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // connectBox
            // 
            this.connectBox.Controls.Add(this.connectButton);
            this.connectBox.Location = new System.Drawing.Point(12, 12);
            this.connectBox.Name = "connectBox";
            this.connectBox.Size = new System.Drawing.Size(273, 53);
            this.connectBox.TabIndex = 5;
            this.connectBox.TabStop = false;
            this.connectBox.Text = "1. Connect";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(75, 19);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(122, 23);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.logTextBox);
            this.groupBox1.Location = new System.Drawing.Point(319, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(227, 383);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(6, 19);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(215, 358);
            this.logTextBox.TabIndex = 1;
            // 
            // doneButton
            // 
            this.doneButton.Location = new System.Drawing.Point(242, 413);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(75, 23);
            this.doneButton.TabIndex = 7;
            this.doneButton.Text = "Done";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // Standalone
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 447);
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.connectBox);
            this.Controls.Add(this.generateQvxBox);
            this.Controls.Add(this.sqlStatementBox);
            this.Controls.Add(this.targetPathBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Standalone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Standalone QVX Connector";
            this.targetPathBox.ResumeLayout(false);
            this.sqlStatementBox.ResumeLayout(false);
            this.sqlStatementBox.PerformLayout();
            this.generateQvxBox.ResumeLayout(false);
            this.connectBox.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox sqlStatementTextBox;
        private System.Windows.Forms.GroupBox targetPathBox;
        private System.Windows.Forms.Button setPathButton;
        private System.Windows.Forms.GroupBox sqlStatementBox;
        private System.Windows.Forms.GroupBox generateQvxBox;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.GroupBox connectBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Button doneButton;
    }
}