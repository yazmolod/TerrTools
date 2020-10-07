namespace TerrTools.UI
{
    partial class TwoDoc
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
            this.LinkComboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.currentDocRadioButton1 = new System.Windows.Forms.RadioButton();
            this.linkDocRadioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxExample1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxExample2 = new System.Windows.Forms.TextBox();
            this.linkDocRadioButton2 = new System.Windows.Forms.RadioButton();
            this.LinkComboBox2 = new System.Windows.Forms.ComboBox();
            this.currentDocRadioButton2 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // LinkComboBox1
            // 
            this.LinkComboBox1.Enabled = false;
            this.LinkComboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LinkComboBox1.FormattingEnabled = true;
            this.LinkComboBox1.Location = new System.Drawing.Point(6, 66);
            this.LinkComboBox1.Name = "LinkComboBox1";
            this.LinkComboBox1.Size = new System.Drawing.Size(150, 21);
            this.LinkComboBox1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.Color.SteelBlue;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(283, 60);
            this.label3.TabIndex = 8;
            this.label3.Text = "Выберите соответствующие \r\nдокументы столбцам из отчета \r\nпо коллизиям";
            // 
            // currentDocRadioButton1
            // 
            this.currentDocRadioButton1.AutoSize = true;
            this.currentDocRadioButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.currentDocRadioButton1.Location = new System.Drawing.Point(6, 20);
            this.currentDocRadioButton1.Name = "currentDocRadioButton1";
            this.currentDocRadioButton1.Size = new System.Drawing.Size(121, 17);
            this.currentDocRadioButton1.TabIndex = 9;
            this.currentDocRadioButton1.TabStop = true;
            this.currentDocRadioButton1.Text = "Текущий документ";
            this.currentDocRadioButton1.UseVisualStyleBackColor = true;
            this.currentDocRadioButton1.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // linkDocRadioButton1
            // 
            this.linkDocRadioButton1.AutoSize = true;
            this.linkDocRadioButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.linkDocRadioButton1.Location = new System.Drawing.Point(7, 42);
            this.linkDocRadioButton1.Name = "linkDocRadioButton1";
            this.linkDocRadioButton1.Size = new System.Drawing.Size(133, 17);
            this.linkDocRadioButton1.TabIndex = 11;
            this.linkDocRadioButton1.TabStop = true;
            this.linkDocRadioButton1.Text = "Связанный документ";
            this.linkDocRadioButton1.UseVisualStyleBackColor = true;
            this.linkDocRadioButton1.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxExample1);
            this.groupBox1.Controls.Add(this.linkDocRadioButton1);
            this.groupBox1.Controls.Add(this.LinkComboBox1);
            this.groupBox1.Controls.Add(this.currentDocRadioButton1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(13, 72);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 95);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Элемент 1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(170, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Путь из файла";
            // 
            // textBoxExample1
            // 
            this.textBoxExample1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExample1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxExample1.Location = new System.Drawing.Point(173, 39);
            this.textBoxExample1.Name = "textBoxExample1";
            this.textBoxExample1.ReadOnly = true;
            this.textBoxExample1.Size = new System.Drawing.Size(245, 20);
            this.textBoxExample1.TabIndex = 12;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 276);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(425, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBoxExample2);
            this.groupBox2.Controls.Add(this.linkDocRadioButton2);
            this.groupBox2.Controls.Add(this.LinkComboBox2);
            this.groupBox2.Controls.Add(this.currentDocRadioButton2);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox2.Location = new System.Drawing.Point(13, 173);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(424, 97);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Элемент 2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(173, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Путь из файла";
            // 
            // textBoxExample2
            // 
            this.textBoxExample2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExample2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxExample2.Location = new System.Drawing.Point(173, 39);
            this.textBoxExample2.Name = "textBoxExample2";
            this.textBoxExample2.ReadOnly = true;
            this.textBoxExample2.Size = new System.Drawing.Size(239, 20);
            this.textBoxExample2.TabIndex = 13;
            // 
            // linkDocRadioButton2
            // 
            this.linkDocRadioButton2.AutoSize = true;
            this.linkDocRadioButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.linkDocRadioButton2.Location = new System.Drawing.Point(7, 42);
            this.linkDocRadioButton2.Name = "linkDocRadioButton2";
            this.linkDocRadioButton2.Size = new System.Drawing.Size(133, 17);
            this.linkDocRadioButton2.TabIndex = 11;
            this.linkDocRadioButton2.TabStop = true;
            this.linkDocRadioButton2.Text = "Связанный документ";
            this.linkDocRadioButton2.UseVisualStyleBackColor = true;
            this.linkDocRadioButton2.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // LinkComboBox2
            // 
            this.LinkComboBox2.Enabled = false;
            this.LinkComboBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LinkComboBox2.FormattingEnabled = true;
            this.LinkComboBox2.Location = new System.Drawing.Point(6, 66);
            this.LinkComboBox2.Name = "LinkComboBox2";
            this.LinkComboBox2.Size = new System.Drawing.Size(150, 21);
            this.LinkComboBox2.TabIndex = 2;
            // 
            // currentDocRadioButton2
            // 
            this.currentDocRadioButton2.AutoSize = true;
            this.currentDocRadioButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.currentDocRadioButton2.Location = new System.Drawing.Point(6, 20);
            this.currentDocRadioButton2.Name = "currentDocRadioButton2";
            this.currentDocRadioButton2.Size = new System.Drawing.Size(121, 17);
            this.currentDocRadioButton2.TabIndex = 9;
            this.currentDocRadioButton2.TabStop = true;
            this.currentDocRadioButton2.Text = "Текущий документ";
            this.currentDocRadioButton2.UseVisualStyleBackColor = true;
            this.currentDocRadioButton2.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // TwoDoc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(443, 307);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Name = "TwoDoc";
            this.Text = "Сопоставление документов";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox LinkComboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton currentDocRadioButton1;
        private System.Windows.Forms.RadioButton linkDocRadioButton1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton linkDocRadioButton2;
        private System.Windows.Forms.ComboBox LinkComboBox2;
        private System.Windows.Forms.RadioButton currentDocRadioButton2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxExample1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxExample2;
    }
}