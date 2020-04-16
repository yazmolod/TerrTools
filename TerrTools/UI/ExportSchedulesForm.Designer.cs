namespace TerrTools.UI
{
    partial class ExportSchedulesForm
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OneSheetRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.MultipleSheetRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox1.Size = new System.Drawing.Size(267, 381);
            this.listBox1.Sorted = true;
            this.listBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(391, 371);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Экспорт";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(285, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(181, 353);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Опции";
            // 
            // OneSheetRadioButton
            // 
            this.OneSheetRadioButton.AutoSize = true;
            this.OneSheetRadioButton.Checked = true;
            this.OneSheetRadioButton.Location = new System.Drawing.Point(3, 3);
            this.OneSheetRadioButton.Name = "OneSheetRadioButton";
            this.OneSheetRadioButton.Size = new System.Drawing.Size(85, 17);
            this.OneSheetRadioButton.TabIndex = 0;
            this.OneSheetRadioButton.TabStop = true;
            this.OneSheetRadioButton.Text = "В один лист";
            this.OneSheetRadioButton.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.MultipleSheetRadioButton);
            this.panel1.Controls.Add(this.OneSheetRadioButton);
            this.panel1.Location = new System.Drawing.Point(6, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(169, 52);
            this.panel1.TabIndex = 1;
            // 
            // MultipleSheetRadioButton
            // 
            this.MultipleSheetRadioButton.AutoSize = true;
            this.MultipleSheetRadioButton.Location = new System.Drawing.Point(4, 27);
            this.MultipleSheetRadioButton.Name = "MultipleSheetRadioButton";
            this.MultipleSheetRadioButton.Size = new System.Drawing.Size(128, 17);
            this.MultipleSheetRadioButton.TabIndex = 1;
            this.MultipleSheetRadioButton.TabStop = true;
            this.MultipleSheetRadioButton.Text = "Разбивать на листы";
            this.MultipleSheetRadioButton.UseVisualStyleBackColor = true;
            // 
            // ExportSchedulesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 406);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Name = "ExportSchedulesForm";
            this.ShowIcon = false;
            this.Text = "Экспорт спецификаций";
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton MultipleSheetRadioButton;
        private System.Windows.Forms.RadioButton OneSheetRadioButton;
    }
}