namespace TerrTools.UI
{
    partial class IzometryGeneratorForm
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
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.viewTypeComboBox = new System.Windows.Forms.ComboBox();
            this.templateViewComboBox = new System.Windows.Forms.ComboBox();
            this.templateCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.warningLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox1.CheckOnClick = true;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 40);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(345, 274);
            this.checkedListBox1.TabIndex = 0;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 458);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(191, 30);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "Автоматически заменять \r\nсуществущие 3D-виды на новые";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(241, 26);
            this.label1.TabIndex = 2;
            this.label1.Text = "Выберите системы, для которых необходимо \r\nсформировать 3D-виды:";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(243, 466);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 31);
            this.button1.TabIndex = 3;
            this.button1.Text = "Создать 3D-виды";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(230, 320);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(125, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Отметить все";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(12, 320);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(136, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Снять выделенные";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // viewTypeComboBox
            // 
            this.viewTypeComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.viewTypeComboBox.FormattingEnabled = true;
            this.viewTypeComboBox.Location = new System.Drawing.Point(12, 431);
            this.viewTypeComboBox.Name = "viewTypeComboBox";
            this.viewTypeComboBox.Size = new System.Drawing.Size(187, 21);
            this.viewTypeComboBox.TabIndex = 0;
            this.viewTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.viewTypeComboBox_SelectedIndexChanged);
            // 
            // templateViewComboBox
            // 
            this.templateViewComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.templateViewComboBox.Enabled = false;
            this.templateViewComboBox.FormattingEnabled = true;
            this.templateViewComboBox.Location = new System.Drawing.Point(12, 383);
            this.templateViewComboBox.Name = "templateViewComboBox";
            this.templateViewComboBox.Size = new System.Drawing.Size(184, 21);
            this.templateViewComboBox.TabIndex = 0;
            this.templateViewComboBox.SelectedIndexChanged += new System.EventHandler(this.templateViewComboBox_SelectedIndexChanged);
            // 
            // templateCheckBox
            // 
            this.templateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.templateCheckBox.AutoSize = true;
            this.templateCheckBox.Location = new System.Drawing.Point(12, 360);
            this.templateCheckBox.Name = "templateCheckBox";
            this.templateCheckBox.Size = new System.Drawing.Size(167, 17);
            this.templateCheckBox.TabIndex = 8;
            this.templateCheckBox.Text = "Использовать шаблон вида";
            this.templateCheckBox.UseVisualStyleBackColor = true;
            this.templateCheckBox.CheckedChanged += new System.EventHandler(this.templateCheckBox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 415);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Тип вида";
            // 
            // warningLabel
            // 
            this.warningLabel.AutoSize = true;
            this.warningLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.warningLabel.Location = new System.Drawing.Point(205, 352);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(162, 91);
            this.warningLabel.TabIndex = 10;
            this.warningLabel.Text = "Внимание: \r\nЕсли в шаблоне \r\nпереопределяются фильтры, \r\nэто отменит фильтр по \r\n" +
    "системе. Во избежание этого \r\nотключите переопределение \r\nфильтров в шаблоне";
            // 
            // IzometryGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 509);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.viewTypeComboBox);
            this.Controls.Add(this.templateViewComboBox);
            this.Controls.Add(this.templateCheckBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.checkedListBox1);
            this.MaximizeBox = false;
            this.Name = "IzometryGeneratorForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Создание 3D-видов";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox viewTypeComboBox;
        private System.Windows.Forms.ComboBox templateViewComboBox;
        private System.Windows.Forms.CheckBox templateCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label warningLabel;
    }
}