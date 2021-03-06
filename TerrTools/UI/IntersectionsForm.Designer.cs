﻿namespace TerrTools.UI
{
    partial class IntersectionsForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.offsetTextBox = new System.Windows.Forms.TextBox();
            this.minSizeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.loadBtn = new System.Windows.Forms.Button();
            this.analyzeBtn = new System.Windows.Forms.Button();
            this.countLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.toleranceTextBox = new System.Windows.Forms.TextBox();
            this.mergeCheckBox = new System.Windows.Forms.CheckBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.Level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PipeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Offset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsBrick = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HoleSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LevelOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GroundOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HoleId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AddToProject = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Level,
            this.HostName,
            this.HostId,
            this.PipeName,
            this.PipeId,
            this.Offset,
            this.IsBrick,
            this.HoleSize,
            this.LevelOffset,
            this.GroundOffset,
            this.HoleId,
            this.AddToProject});
            this.dataGridView1.Location = new System.Drawing.Point(93, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(736, 519);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(724, 554);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 39);
            this.button1.TabIndex = 1;
            this.button1.Text = "Создать отверстия";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // offsetTextBox
            // 
            this.offsetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.offsetTextBox.Location = new System.Drawing.Point(12, 573);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(98, 20);
            this.offsetTextBox.TabIndex = 2;
            this.offsetTextBox.Text = "50";
            this.offsetTextBox.TextChanged += new System.EventHandler(this.offsetTextBox_TextChanged);
            // 
            // minSizeTextBox
            // 
            this.minSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.minSizeTextBox.Location = new System.Drawing.Point(153, 573);
            this.minSizeTextBox.Name = "minSizeTextBox";
            this.minSizeTextBox.Size = new System.Drawing.Size(100, 20);
            this.minSizeTextBox.TabIndex = 4;
            this.minSizeTextBox.Text = "150";
            this.minSizeTextBox.TextChanged += new System.EventHandler(this.minSizeTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 539);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 26);
            this.label1.TabIndex = 6;
            this.label1.Text = "Задать отступ \r\nдля всех пересечений";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(150, 539);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(229, 26);
            this.label2.TabIndex = 7;
            this.label2.Text = "Добавить в проект отверстия в местах \r\nпересечения с сетями размером не менее:";
            // 
            // loadBtn
            // 
            this.loadBtn.Location = new System.Drawing.Point(12, 12);
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(75, 61);
            this.loadBtn.TabIndex = 8;
            this.loadBtn.Text = "Загрузить отчет коллизий";
            this.loadBtn.UseVisualStyleBackColor = true;
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // analyzeBtn
            // 
            this.analyzeBtn.Location = new System.Drawing.Point(12, 79);
            this.analyzeBtn.Name = "analyzeBtn";
            this.analyzeBtn.Size = new System.Drawing.Size(75, 45);
            this.analyzeBtn.TabIndex = 9;
            this.analyzeBtn.Text = "Анализ";
            this.analyzeBtn.UseVisualStyleBackColor = true;
            this.analyzeBtn.Click += new System.EventHandler(this.analyzeBtn_Click);
            // 
            // countLabel
            // 
            this.countLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.countLabel.AutoSize = true;
            this.countLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.countLabel.Location = new System.Drawing.Point(721, 534);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(41, 13);
            this.countLabel.TabIndex = 10;
            this.countLabel.Text = "label3";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Enabled = false;
            this.label3.Location = new System.Drawing.Point(593, 539);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 26);
            this.label3.TabIndex = 12;
            this.label3.Text = "Максимальный зазор \r\nдля объединения";
            // 
            // toleranceTextBox
            // 
            this.toleranceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.toleranceTextBox.Location = new System.Drawing.Point(596, 573);
            this.toleranceTextBox.Name = "toleranceTextBox";
            this.toleranceTextBox.Size = new System.Drawing.Size(98, 20);
            this.toleranceTextBox.TabIndex = 11;
            this.toleranceTextBox.Text = "150";
            // 
            // mergeCheckBox
            // 
            this.mergeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mergeCheckBox.AutoSize = true;
            this.mergeCheckBox.Checked = true;
            this.mergeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mergeCheckBox.Location = new System.Drawing.Point(496, 539);
            this.mergeCheckBox.Name = "mergeCheckBox";
            this.mergeCheckBox.Size = new System.Drawing.Size(91, 30);
            this.mergeCheckBox.TabIndex = 13;
            this.mergeCheckBox.Text = "Объединять \r\nотверстия";
            this.mergeCheckBox.UseVisualStyleBackColor = true;
            this.mergeCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(12, 495);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 36);
            this.clearButton.TabIndex = 14;
            this.clearButton.Text = "Очистить\r\nтаблицу";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // Level
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Level.DefaultCellStyle = dataGridViewCellStyle1;
            this.Level.HeaderText = "Уровень пересечения";
            this.Level.Name = "Level";
            this.Level.ReadOnly = true;
            // 
            // HostName
            // 
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.HostName.DefaultCellStyle = dataGridViewCellStyle2;
            this.HostName.HeaderText = "Конструкция";
            this.HostName.Name = "HostName";
            this.HostName.ReadOnly = true;
            // 
            // HostId
            // 
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.HostId.DefaultCellStyle = dataGridViewCellStyle3;
            this.HostId.HeaderText = "Id конструкции";
            this.HostId.Name = "HostId";
            this.HostId.ReadOnly = true;
            this.HostId.Width = 50;
            // 
            // PipeName
            // 
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PipeName.DefaultCellStyle = dataGridViewCellStyle4;
            this.PipeName.HeaderText = "Труба/Воздуховод";
            this.PipeName.Name = "PipeName";
            this.PipeName.ReadOnly = true;
            // 
            // PipeId
            // 
            this.PipeId.HeaderText = "Id трубы";
            this.PipeId.Name = "PipeId";
            this.PipeId.ReadOnly = true;
            this.PipeId.Visible = false;
            this.PipeId.Width = 50;
            // 
            // Offset
            // 
            this.Offset.HeaderText = "Отступ";
            this.Offset.Name = "Offset";
            this.Offset.Visible = false;
            this.Offset.Width = 50;
            // 
            // IsBrick
            // 
            this.IsBrick.HeaderText = "В кирпиче";
            this.IsBrick.Name = "IsBrick";
            this.IsBrick.ReadOnly = true;
            this.IsBrick.Width = 60;
            // 
            // HoleSize
            // 
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.HoleSize.DefaultCellStyle = dataGridViewCellStyle5;
            this.HoleSize.HeaderText = "Размер отверстия";
            this.HoleSize.Name = "HoleSize";
            this.HoleSize.ReadOnly = true;
            this.HoleSize.Width = 120;
            // 
            // LevelOffset
            // 
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LevelOffset.DefaultCellStyle = dataGridViewCellStyle6;
            this.LevelOffset.HeaderText = "Отм. от этажа";
            this.LevelOffset.Name = "LevelOffset";
            this.LevelOffset.ReadOnly = true;
            this.LevelOffset.Width = 50;
            // 
            // GroundOffset
            // 
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GroundOffset.DefaultCellStyle = dataGridViewCellStyle7;
            this.GroundOffset.HeaderText = "Отм. от 0.000";
            this.GroundOffset.Name = "GroundOffset";
            this.GroundOffset.ReadOnly = true;
            this.GroundOffset.Width = 50;
            // 
            // HoleId
            // 
            this.HoleId.HeaderText = "Id отверстия ";
            this.HoleId.Name = "HoleId";
            this.HoleId.ReadOnly = true;
            this.HoleId.Visible = false;
            this.HoleId.Width = 70;
            // 
            // AddToProject
            // 
            this.AddToProject.HeaderText = "Добавить в проект";
            this.AddToProject.Name = "AddToProject";
            this.AddToProject.Width = 60;
            // 
            // IntersectionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(837, 605);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.mergeCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.toleranceTextBox);
            this.Controls.Add(this.countLabel);
            this.Controls.Add(this.analyzeBtn);
            this.Controls.Add(this.loadBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.minSizeTextBox);
            this.Controls.Add(this.offsetTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2000, 2000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(840, 300);
            this.Name = "IntersectionsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Пересечения сетей с конструкциями";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox offsetTextBox;
        private System.Windows.Forms.TextBox minSizeTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Button analyzeBtn;
        private System.Windows.Forms.Label countLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox toleranceTextBox;
        private System.Windows.Forms.CheckBox mergeCheckBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn Level;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostName;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostId;
        private System.Windows.Forms.DataGridViewTextBoxColumn PipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PipeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Offset;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsBrick;
        private System.Windows.Forms.DataGridViewTextBoxColumn HoleSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn LevelOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn GroundOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn HoleId;
        private System.Windows.Forms.DataGridViewCheckBoxColumn AddToProject;
    }
}