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
            this.leftListBox = new System.Windows.Forms.ListBox();
            this.exportBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.NoSplitRB = new System.Windows.Forms.RadioButton();
            this.splitOneSheetRB = new System.Windows.Forms.RadioButton();
            this.splitMultiSheetRB = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.oneFileOneSheetRB = new System.Windows.Forms.RadioButton();
            this.oneFileMultipleSheetRB = new System.Windows.Forms.RadioButton();
            this.multipleFilesRB = new System.Windows.Forms.RadioButton();
            this.rightListBox = new System.Windows.Forms.ListBox();
            this.downBtn = new System.Windows.Forms.Button();
            this.upBtn = new System.Windows.Forms.Button();
            this.inBtn = new System.Windows.Forms.Button();
            this.outBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.mergeCheckBox = new System.Windows.Forms.CheckBox();
            this.headerCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // leftListBox
            // 
            this.leftListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.leftListBox.FormattingEnabled = true;
            this.leftListBox.Location = new System.Drawing.Point(12, 38);
            this.leftListBox.Name = "leftListBox";
            this.leftListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.leftListBox.Size = new System.Drawing.Size(258, 394);
            this.leftListBox.Sorted = true;
            this.leftListBox.TabIndex = 0;
            // 
            // exportBtn
            // 
            this.exportBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportBtn.Location = new System.Drawing.Point(729, 438);
            this.exportBtn.Name = "exportBtn";
            this.exportBtn.Size = new System.Drawing.Size(75, 23);
            this.exportBtn.TabIndex = 1;
            this.exportBtn.Text = "Экспорт";
            this.exportBtn.UseVisualStyleBackColor = true;
            this.exportBtn.Click += new System.EventHandler(this.exportBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.headerCheckBox);
            this.groupBox1.Controls.Add(this.mergeCheckBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(549, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(255, 420);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Опции";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(218, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Разбиение спецификации по заголовкам";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.NoSplitRB);
            this.panel2.Controls.Add(this.splitOneSheetRB);
            this.panel2.Controls.Add(this.splitMultiSheetRB);
            this.panel2.Location = new System.Drawing.Point(6, 152);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(243, 80);
            this.panel2.TabIndex = 2;
            // 
            // NoSplitRB
            // 
            this.NoSplitRB.AutoSize = true;
            this.NoSplitRB.Checked = true;
            this.NoSplitRB.Location = new System.Drawing.Point(4, 4);
            this.NoSplitRB.Name = "NoSplitRB";
            this.NoSplitRB.Size = new System.Drawing.Size(95, 17);
            this.NoSplitRB.TabIndex = 12;
            this.NoSplitRB.TabStop = true;
            this.NoSplitRB.Text = "Не разбивать";
            this.NoSplitRB.UseVisualStyleBackColor = true;
            // 
            // splitOneSheetRB
            // 
            this.splitOneSheetRB.AutoSize = true;
            this.splitOneSheetRB.Location = new System.Drawing.Point(4, 50);
            this.splitOneSheetRB.Name = "splitOneSheetRB";
            this.splitOneSheetRB.Size = new System.Drawing.Size(135, 17);
            this.splitOneSheetRB.TabIndex = 11;
            this.splitOneSheetRB.Text = "Разбивать, один лист";
            this.splitOneSheetRB.UseVisualStyleBackColor = true;
            // 
            // splitMultiSheetRB
            // 
            this.splitMultiSheetRB.AutoSize = true;
            this.splitMultiSheetRB.Location = new System.Drawing.Point(4, 27);
            this.splitMultiSheetRB.Name = "splitMultiSheetRB";
            this.splitMultiSheetRB.Size = new System.Drawing.Size(157, 17);
            this.splitMultiSheetRB.TabIndex = 1;
            this.splitMultiSheetRB.Text = "Разбивать, разные листы";
            this.splitMultiSheetRB.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Несколько спецификаций в:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.oneFileOneSheetRB);
            this.panel1.Controls.Add(this.oneFileMultipleSheetRB);
            this.panel1.Controls.Add(this.multipleFilesRB);
            this.panel1.Location = new System.Drawing.Point(6, 44);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(243, 80);
            this.panel1.TabIndex = 1;
            // 
            // oneFileOneSheetRB
            // 
            this.oneFileOneSheetRB.AutoSize = true;
            this.oneFileOneSheetRB.Location = new System.Drawing.Point(3, 50);
            this.oneFileOneSheetRB.Name = "oneFileOneSheetRB";
            this.oneFileOneSheetRB.Size = new System.Drawing.Size(136, 17);
            this.oneFileOneSheetRB.TabIndex = 0;
            this.oneFileOneSheetRB.Text = "Один файл, один лист";
            this.oneFileOneSheetRB.UseVisualStyleBackColor = true;
            this.oneFileOneSheetRB.CheckedChanged += new System.EventHandler(this.oneFileOneSheetRB_CheckedChanged);
            // 
            // oneFileMultipleSheetRB
            // 
            this.oneFileMultipleSheetRB.AutoSize = true;
            this.oneFileMultipleSheetRB.Location = new System.Drawing.Point(4, 27);
            this.oneFileMultipleSheetRB.Name = "oneFileMultipleSheetRB";
            this.oneFileMultipleSheetRB.Size = new System.Drawing.Size(158, 17);
            this.oneFileMultipleSheetRB.TabIndex = 1;
            this.oneFileMultipleSheetRB.Text = "Один файл, разные листы";
            this.oneFileMultipleSheetRB.UseVisualStyleBackColor = true;
            // 
            // multipleFilesRB
            // 
            this.multipleFilesRB.AutoSize = true;
            this.multipleFilesRB.Checked = true;
            this.multipleFilesRB.Location = new System.Drawing.Point(3, 3);
            this.multipleFilesRB.Name = "multipleFilesRB";
            this.multipleFilesRB.Size = new System.Drawing.Size(110, 17);
            this.multipleFilesRB.TabIndex = 0;
            this.multipleFilesRB.TabStop = true;
            this.multipleFilesRB.Text = "В разные файлы";
            this.multipleFilesRB.UseVisualStyleBackColor = true;
            // 
            // rightListBox
            // 
            this.rightListBox.FormattingEnabled = true;
            this.rightListBox.Location = new System.Drawing.Point(315, 38);
            this.rightListBox.Name = "rightListBox";
            this.rightListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.rightListBox.Size = new System.Drawing.Size(228, 394);
            this.rightListBox.TabIndex = 3;
            // 
            // downBtn
            // 
            this.downBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.downBtn.Location = new System.Drawing.Point(510, 438);
            this.downBtn.Name = "downBtn";
            this.downBtn.Size = new System.Drawing.Size(33, 23);
            this.downBtn.TabIndex = 4;
            this.downBtn.Text = "↓";
            this.downBtn.UseVisualStyleBackColor = true;
            this.downBtn.Click += new System.EventHandler(this.updownBtn_Click);
            // 
            // upBtn
            // 
            this.upBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.upBtn.Location = new System.Drawing.Point(471, 438);
            this.upBtn.Name = "upBtn";
            this.upBtn.Size = new System.Drawing.Size(33, 23);
            this.upBtn.TabIndex = 5;
            this.upBtn.Text = "↑";
            this.upBtn.UseVisualStyleBackColor = true;
            this.upBtn.Click += new System.EventHandler(this.updownBtn_Click);
            // 
            // inBtn
            // 
            this.inBtn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.inBtn.Location = new System.Drawing.Point(276, 192);
            this.inBtn.Name = "inBtn";
            this.inBtn.Size = new System.Drawing.Size(33, 23);
            this.inBtn.TabIndex = 6;
            this.inBtn.Text = "=>";
            this.inBtn.UseVisualStyleBackColor = true;
            this.inBtn.Click += new System.EventHandler(this.inBtn_Click);
            // 
            // outBtn
            // 
            this.outBtn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.outBtn.Location = new System.Drawing.Point(276, 221);
            this.outBtn.Name = "outBtn";
            this.outBtn.Size = new System.Drawing.Size(33, 23);
            this.outBtn.TabIndex = 6;
            this.outBtn.Text = "<=";
            this.outBtn.UseVisualStyleBackColor = true;
            this.outBtn.Click += new System.EventHandler(this.outBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Доступные спецификации:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(312, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(209, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Спецификации на экспорт (по порядку):";
            // 
            // mergeCheckBox
            // 
            this.mergeCheckBox.AutoSize = true;
            this.mergeCheckBox.Enabled = false;
            this.mergeCheckBox.Location = new System.Drawing.Point(5, 261);
            this.mergeCheckBox.Name = "mergeCheckBox";
            this.mergeCheckBox.Size = new System.Drawing.Size(209, 17);
            this.mergeCheckBox.TabIndex = 11;
            this.mergeCheckBox.Text = "Объединять одинаковые заголовки";
            this.mergeCheckBox.UseVisualStyleBackColor = true;
            // 
            // headerCheckBox
            // 
            this.headerCheckBox.AutoSize = true;
            this.headerCheckBox.Checked = true;
            this.headerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.headerCheckBox.Location = new System.Drawing.Point(6, 238);
            this.headerCheckBox.Name = "headerCheckBox";
            this.headerCheckBox.Size = new System.Drawing.Size(190, 17);
            this.headerCheckBox.TabIndex = 12;
            this.headerCheckBox.Text = "Показывать названия столбцов";
            this.headerCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExportSchedulesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 469);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.outBtn);
            this.Controls.Add(this.inBtn);
            this.Controls.Add(this.upBtn);
            this.Controls.Add(this.downBtn);
            this.Controls.Add(this.rightListBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.exportBtn);
            this.Controls.Add(this.leftListBox);
            this.Name = "ExportSchedulesForm";
            this.ShowIcon = false;
            this.Text = "Экспорт спецификаций";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox leftListBox;
        private System.Windows.Forms.Button exportBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton oneFileMultipleSheetRB;
        private System.Windows.Forms.RadioButton multipleFilesRB;
        private System.Windows.Forms.ListBox rightListBox;
        private System.Windows.Forms.Button downBtn;
        private System.Windows.Forms.Button upBtn;
        private System.Windows.Forms.Button inBtn;
        private System.Windows.Forms.Button outBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton oneFileOneSheetRB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton splitMultiSheetRB;
        private System.Windows.Forms.RadioButton splitOneSheetRB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton NoSplitRB;
        private System.Windows.Forms.CheckBox headerCheckBox;
        private System.Windows.Forms.CheckBox mergeCheckBox;
    }
}