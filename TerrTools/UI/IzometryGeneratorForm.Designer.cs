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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.viewTypeComboBox = new System.Windows.Forms.ComboBox();
            this.templateViewComboBox = new System.Windows.Forms.ComboBox();
            this.templateCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox1.CheckOnClick = true;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 40);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(234, 319);
            this.checkedListBox1.TabIndex = 0;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 488);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(231, 30);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "Автоматически заменять существущие \r\n3D-виды на новые";
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
            this.button1.Location = new System.Drawing.Point(252, 485);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 31);
            this.button1.TabIndex = 3;
            this.button1.Text = "Создать 3D-виды";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(252, 69);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Отметить все";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(252, 40);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Снять выделенные";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.viewTypeComboBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 364);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(231, 54);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Тип вида";
            // 
            // viewTypeComboBox
            // 
            this.viewTypeComboBox.FormattingEnabled = true;
            this.viewTypeComboBox.Location = new System.Drawing.Point(6, 19);
            this.viewTypeComboBox.Name = "viewTypeComboBox";
            this.viewTypeComboBox.Size = new System.Drawing.Size(219, 21);
            this.viewTypeComboBox.TabIndex = 0;
            this.viewTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.viewTypeComboBox_SelectedIndexChanged);
            // 
            // templateViewComboBox
            // 
            this.templateViewComboBox.Enabled = false;
            this.templateViewComboBox.FormattingEnabled = true;
            this.templateViewComboBox.Location = new System.Drawing.Point(12, 457);
            this.templateViewComboBox.Name = "templateViewComboBox";
            this.templateViewComboBox.Size = new System.Drawing.Size(219, 21);
            this.templateViewComboBox.TabIndex = 0;
            this.templateViewComboBox.SelectedIndexChanged += new System.EventHandler(this.templateViewComboBox_SelectedIndexChanged);
            // 
            // templateCheckBox
            // 
            this.templateCheckBox.AutoSize = true;
            this.templateCheckBox.Location = new System.Drawing.Point(12, 434);
            this.templateCheckBox.Name = "templateCheckBox";
            this.templateCheckBox.Size = new System.Drawing.Size(167, 17);
            this.templateCheckBox.TabIndex = 8;
            this.templateCheckBox.Text = "Использовать шаблон вида";
            this.templateCheckBox.UseVisualStyleBackColor = true;
            this.templateCheckBox.CheckedChanged += new System.EventHandler(this.templateCheckBox_CheckedChanged);
            // 
            // IzometryGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 522);
            this.Controls.Add(this.templateViewComboBox);
            this.Controls.Add(this.templateCheckBox);
            this.Controls.Add(this.groupBox1);
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
            this.groupBox1.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox viewTypeComboBox;
        private System.Windows.Forms.ComboBox templateViewComboBox;
        private System.Windows.Forms.CheckBox templateCheckBox;
    }
}