namespace TerrTools.UI
{
    partial class InsulCurvesForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SelectRadioButton = new System.Windows.Forms.RadioButton();
            this.ViewRadioButton = new System.Windows.Forms.RadioButton();
            this.DocumentRadioButton = new System.Windows.Forms.RadioButton();
            this.ModelCurveRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DetailRadioButton = new System.Windows.Forms.RadioButton();
            this.stepTextBox = new System.Windows.Forms.TextBox();
            this.heightTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.createButton = new System.Windows.Forms.Button();
            this.graphicComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.SelectRadioButton);
            this.groupBox1.Controls.Add(this.ViewRadioButton);
            this.groupBox1.Controls.Add(this.DocumentRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(244, 95);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Штриховать элементы:";
            // 
            // SelectRadioButton
            // 
            this.SelectRadioButton.AutoSize = true;
            this.SelectRadioButton.Location = new System.Drawing.Point(7, 68);
            this.SelectRadioButton.Name = "SelectRadioButton";
            this.SelectRadioButton.Size = new System.Drawing.Size(113, 17);
            this.SelectRadioButton.TabIndex = 2;
            this.SelectRadioButton.TabStop = true;
            this.SelectRadioButton.Text = "Выбрать вручную";
            this.SelectRadioButton.UseVisualStyleBackColor = true;
            this.SelectRadioButton.CheckedChanged += new System.EventHandler(this.DocumentRadioButton_CheckedChanged);
            // 
            // ViewRadioButton
            // 
            this.ViewRadioButton.AutoSize = true;
            this.ViewRadioButton.Location = new System.Drawing.Point(7, 44);
            this.ViewRadioButton.Name = "ViewRadioButton";
            this.ViewRadioButton.Size = new System.Drawing.Size(114, 17);
            this.ViewRadioButton.TabIndex = 1;
            this.ViewRadioButton.TabStop = true;
            this.ViewRadioButton.Text = "На текущем виде";
            this.ViewRadioButton.UseVisualStyleBackColor = true;
            this.ViewRadioButton.CheckedChanged += new System.EventHandler(this.DocumentRadioButton_CheckedChanged);
            // 
            // DocumentRadioButton
            // 
            this.DocumentRadioButton.AutoSize = true;
            this.DocumentRadioButton.Checked = true;
            this.DocumentRadioButton.Location = new System.Drawing.Point(7, 20);
            this.DocumentRadioButton.Name = "DocumentRadioButton";
            this.DocumentRadioButton.Size = new System.Drawing.Size(124, 17);
            this.DocumentRadioButton.TabIndex = 0;
            this.DocumentRadioButton.TabStop = true;
            this.DocumentRadioButton.Text = "Во всем документе";
            this.DocumentRadioButton.UseVisualStyleBackColor = true;
            this.DocumentRadioButton.CheckedChanged += new System.EventHandler(this.DocumentRadioButton_CheckedChanged);
            // 
            // ModelCurveRadioButton
            // 
            this.ModelCurveRadioButton.AutoSize = true;
            this.ModelCurveRadioButton.Checked = true;
            this.ModelCurveRadioButton.Location = new System.Drawing.Point(7, 20);
            this.ModelCurveRadioButton.Name = "ModelCurveRadioButton";
            this.ModelCurveRadioButton.Size = new System.Drawing.Size(121, 17);
            this.ModelCurveRadioButton.TabIndex = 0;
            this.ModelCurveRadioButton.TabStop = true;
            this.ModelCurveRadioButton.Text = "Линии модели (3D)";
            this.ModelCurveRadioButton.UseVisualStyleBackColor = true;
            this.ModelCurveRadioButton.CheckedChanged += new System.EventHandler(this.DocumentRadioButton_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.DetailRadioButton);
            this.groupBox2.Controls.Add(this.ModelCurveRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(12, 113);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(244, 72);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Тип создаваемых линий";
            // 
            // DetailRadioButton
            // 
            this.DetailRadioButton.AutoSize = true;
            this.DetailRadioButton.Enabled = false;
            this.DetailRadioButton.Location = new System.Drawing.Point(7, 43);
            this.DetailRadioButton.Name = "DetailRadioButton";
            this.DetailRadioButton.Size = new System.Drawing.Size(148, 17);
            this.DetailRadioButton.TabIndex = 1;
            this.DetailRadioButton.Text = "Линии детализации (2D)";
            this.DetailRadioButton.UseVisualStyleBackColor = true;
            this.DetailRadioButton.CheckedChanged += new System.EventHandler(this.DocumentRadioButton_CheckedChanged);
            // 
            // stepTextBox
            // 
            this.stepTextBox.Location = new System.Drawing.Point(136, 19);
            this.stepTextBox.Name = "stepTextBox";
            this.stepTextBox.Size = new System.Drawing.Size(100, 20);
            this.stepTextBox.TabIndex = 2;
            this.stepTextBox.Text = "300";
            this.stepTextBox.TextChanged += new System.EventHandler(this.stepTextBox_TextChanged);
            // 
            // heightTextBox
            // 
            this.heightTextBox.Location = new System.Drawing.Point(136, 45);
            this.heightTextBox.Name = "heightTextBox";
            this.heightTextBox.Size = new System.Drawing.Size(100, 20);
            this.heightTextBox.TabIndex = 3;
            this.heightTextBox.Text = "300";
            this.heightTextBox.TextChanged += new System.EventHandler(this.heightTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Шаг штриховки, мм:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Высота штриховки, мм:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.stepTextBox);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.heightTextBox);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 242);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(244, 74);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            // 
            // createButton
            // 
            this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.createButton.Location = new System.Drawing.Point(12, 322);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(244, 23);
            this.createButton.TabIndex = 7;
            this.createButton.Text = "Создать";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // graphicComboBox
            // 
            this.graphicComboBox.FormattingEnabled = true;
            this.graphicComboBox.Location = new System.Drawing.Point(6, 19);
            this.graphicComboBox.Name = "graphicComboBox";
            this.graphicComboBox.Size = new System.Drawing.Size(230, 21);
            this.graphicComboBox.TabIndex = 2;
            this.graphicComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.graphicComboBox);
            this.groupBox4.Location = new System.Drawing.Point(12, 191);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(244, 52);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Стиль линии";
            // 
            // InsulCurvesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 354);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "InsulCurvesForm";
            this.ShowIcon = false;
            this.Text = "Создание штриховки /\\";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton SelectRadioButton;
        private System.Windows.Forms.RadioButton ViewRadioButton;
        private System.Windows.Forms.RadioButton DocumentRadioButton;
        private System.Windows.Forms.RadioButton ModelCurveRadioButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox stepTextBox;
        private System.Windows.Forms.TextBox heightTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.RadioButton DetailRadioButton;
        private System.Windows.Forms.ComboBox graphicComboBox;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}