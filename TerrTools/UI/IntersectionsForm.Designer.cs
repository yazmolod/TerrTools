namespace TerrTools.UI
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IntersectionPoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PipeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PipeSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Offset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsBrick = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HoleSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LevelOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GroundOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HoleId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AddToProject = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.offsetTextBox = new System.Windows.Forms.TextBox();
            this.minSizeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
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
            this.IntersectionPoint,
            this.HostName,
            this.HostId,
            this.PipeName,
            this.PipeId,
            this.PipeSize,
            this.Offset,
            this.IsBrick,
            this.HoleSize,
            this.LevelOffset,
            this.GroundOffset,
            this.HoleId,
            this.AddToProject});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(1047, 458);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            // 
            // Level
            // 
            this.Level.HeaderText = "Уровень пересечения";
            this.Level.Name = "Level";
            this.Level.ReadOnly = true;
            // 
            // IntersectionPoint
            // 
            this.IntersectionPoint.HeaderText = "Точка пересечения";
            this.IntersectionPoint.Name = "IntersectionPoint";
            this.IntersectionPoint.ReadOnly = true;
            // 
            // HostName
            // 
            this.HostName.HeaderText = "Стена";
            this.HostName.Name = "HostName";
            this.HostName.ReadOnly = true;
            // 
            // HostId
            // 
            this.HostId.HeaderText = "Id стены";
            this.HostId.Name = "HostId";
            this.HostId.ReadOnly = true;
            this.HostId.Visible = false;
            this.HostId.Width = 50;
            // 
            // PipeName
            // 
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
            // PipeSize
            // 
            this.PipeSize.HeaderText = "Размер трубы/воздуховода";
            this.PipeSize.Name = "PipeSize";
            this.PipeSize.ReadOnly = true;
            // 
            // Offset
            // 
            this.Offset.HeaderText = "Минимальный отступ";
            this.Offset.Name = "Offset";
            this.Offset.Width = 50;
            // 
            // IsBrick
            // 
            this.IsBrick.HeaderText = "Размеры кирпича";
            this.IsBrick.Name = "IsBrick";
            this.IsBrick.Width = 60;
            // 
            // HoleSize
            // 
            this.HoleSize.HeaderText = "Размер отверстия";
            this.HoleSize.Name = "HoleSize";
            this.HoleSize.ReadOnly = true;
            // 
            // LevelOffset
            // 
            this.LevelOffset.HeaderText = "Отметка низа (от уровня этажа)";
            this.LevelOffset.Name = "LevelOffset";
            this.LevelOffset.ReadOnly = true;
            this.LevelOffset.Width = 80;
            // 
            // GroundOffset
            // 
            this.GroundOffset.HeaderText = "Отметка низа (от уровня 0.000)";
            this.GroundOffset.Name = "GroundOffset";
            this.GroundOffset.ReadOnly = true;
            this.GroundOffset.Width = 80;
            // 
            // HoleId
            // 
            this.HoleId.HeaderText = "Id отверстия ";
            this.HoleId.Name = "HoleId";
            this.HoleId.ReadOnly = true;
            this.HoleId.Width = 70;
            // 
            // AddToProject
            // 
            this.AddToProject.HeaderText = "Добавить в проект";
            this.AddToProject.Name = "AddToProject";
            this.AddToProject.Width = 60;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(954, 476);
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
            this.offsetTextBox.Location = new System.Drawing.Point(12, 495);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(98, 20);
            this.offsetTextBox.TabIndex = 2;
            this.offsetTextBox.Text = "50";
            this.offsetTextBox.TextChanged += new System.EventHandler(this.offsetTextBox_TextChanged);
            // 
            // minSizeTextBox
            // 
            this.minSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.minSizeTextBox.Location = new System.Drawing.Point(308, 495);
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
            this.label1.Location = new System.Drawing.Point(12, 476);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(269, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Задать минимальный отступ для всех пересечений";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(305, 476);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(432, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Добавить в проект отверстия в местах пересечения с сетями размером не менее:";
            // 
            // IntersectionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 527);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.minSizeTextBox);
            this.Controls.Add(this.offsetTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IntersectionsForm";
            this.ShowIcon = false;
            this.Text = "Пересечения сетей со стенами";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Level;
        private System.Windows.Forms.DataGridViewTextBoxColumn IntersectionPoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostName;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostId;
        private System.Windows.Forms.DataGridViewTextBoxColumn PipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PipeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn PipeSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn Offset;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsBrick;
        private System.Windows.Forms.DataGridViewTextBoxColumn HoleSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn LevelOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn GroundOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn HoleId;
        private System.Windows.Forms.DataGridViewCheckBoxColumn AddToProject;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox offsetTextBox;
        private System.Windows.Forms.TextBox minSizeTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}