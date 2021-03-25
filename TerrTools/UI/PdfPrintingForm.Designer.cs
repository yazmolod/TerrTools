namespace TerrTools.UI
{
    partial class PdfPrintingForm
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
            this.printButton = new System.Windows.Forms.Button();
            this.printerComboBox = new System.Windows.Forms.ComboBox();
            this.setComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.currentRadioButton = new System.Windows.Forms.RadioButton();
            this.selectionRadioButton = new System.Windows.Forms.RadioButton();
            this.setRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // printButton
            // 
            this.printButton.Location = new System.Drawing.Point(150, 196);
            this.printButton.Name = "printButton";
            this.printButton.Size = new System.Drawing.Size(125, 23);
            this.printButton.TabIndex = 0;
            this.printButton.Text = "Экспорт PDF";
            this.printButton.UseVisualStyleBackColor = true;
            this.printButton.Click += new System.EventHandler(this.printButton_Click);
            // 
            // printerComboBox
            // 
            this.printerComboBox.FormattingEnabled = true;
            this.printerComboBox.Location = new System.Drawing.Point(6, 19);
            this.printerComboBox.Name = "printerComboBox";
            this.printerComboBox.Size = new System.Drawing.Size(250, 21);
            this.printerComboBox.TabIndex = 1;
            this.printerComboBox.SelectedIndexChanged += new System.EventHandler(this.printerComboBox_SelectedIndexChanged);
            // 
            // setComboBox
            // 
            this.setComboBox.Enabled = false;
            this.setComboBox.FormattingEnabled = true;
            this.setComboBox.Location = new System.Drawing.Point(7, 88);
            this.setComboBox.Name = "setComboBox";
            this.setComboBox.Size = new System.Drawing.Size(250, 21);
            this.setComboBox.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.printerComboBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(263, 54);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Виртуальный принтер";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.setComboBox);
            this.groupBox2.Controls.Add(this.setRadioButton);
            this.groupBox2.Controls.Add(this.currentRadioButton);
            this.groupBox2.Controls.Add(this.selectionRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(12, 72);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(263, 118);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Что печатать";
            // 
            // currentRadioButton
            // 
            this.currentRadioButton.AutoSize = true;
            this.currentRadioButton.Checked = true;
            this.currentRadioButton.Location = new System.Drawing.Point(6, 19);
            this.currentRadioButton.Name = "currentRadioButton";
            this.currentRadioButton.Size = new System.Drawing.Size(91, 17);
            this.currentRadioButton.TabIndex = 5;
            this.currentRadioButton.TabStop = true;
            this.currentRadioButton.Text = "Текущий вид";
            this.currentRadioButton.UseVisualStyleBackColor = true;
            // 
            // selectionRadioButton
            // 
            this.selectionRadioButton.AutoSize = true;
            this.selectionRadioButton.Location = new System.Drawing.Point(6, 42);
            this.selectionRadioButton.Name = "selectionRadioButton";
            this.selectionRadioButton.Size = new System.Drawing.Size(192, 17);
            this.selectionRadioButton.TabIndex = 6;
            this.selectionRadioButton.Text = "Виды, выделенные в диспетчере";
            this.selectionRadioButton.UseVisualStyleBackColor = true;
            // 
            // setRadioButton
            // 
            this.setRadioButton.AutoSize = true;
            this.setRadioButton.Location = new System.Drawing.Point(6, 65);
            this.setRadioButton.Name = "setRadioButton";
            this.setRadioButton.Size = new System.Drawing.Size(95, 17);
            this.setRadioButton.TabIndex = 7;
            this.setRadioButton.Text = "Набор листов";
            this.setRadioButton.UseVisualStyleBackColor = true;
            // 
            // PdfPrintingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 229);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.printButton);
            this.Name = "PdfPrintingForm";
            this.ShowIcon = false;
            this.Text = "Экспорт PDF";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button printButton;
        private System.Windows.Forms.ComboBox printerComboBox;
        private System.Windows.Forms.ComboBox setComboBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton setRadioButton;
        private System.Windows.Forms.RadioButton currentRadioButton;
        private System.Windows.Forms.RadioButton selectionRadioButton;
    }
}