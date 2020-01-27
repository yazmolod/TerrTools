namespace TerrTools.UI
{
    partial class ElementFilterForm
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
            this.checkedListBox = new System.Windows.Forms.CheckedListBox();
            this.showButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.valueComboBox = new System.Windows.Forms.ComboBox();
            this.comparisonComboBox = new System.Windows.Forms.ComboBox();
            this.parameterComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.checkedListBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(227, 391);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Категории";
            // 
            // checkedListBox
            // 
            this.checkedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBox.FormattingEnabled = true;
            this.checkedListBox.Location = new System.Drawing.Point(3, 16);
            this.checkedListBox.Name = "checkedListBox";
            this.checkedListBox.Size = new System.Drawing.Size(221, 372);
            this.checkedListBox.TabIndex = 0;
            this.checkedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox_ItemCheck);
            // 
            // showButton
            // 
            this.showButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.showButton.Location = new System.Drawing.Point(395, 378);
            this.showButton.Name = "showButton";
            this.showButton.Size = new System.Drawing.Size(100, 25);
            this.showButton.TabIndex = 1;
            this.showButton.Text = "Показать";
            this.showButton.UseVisualStyleBackColor = true;
            this.showButton.Click += new System.EventHandler(this.showButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.valueComboBox);
            this.groupBox2.Controls.Add(this.comparisonComboBox);
            this.groupBox2.Controls.Add(this.parameterComboBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(245, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(250, 108);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Критерий фильтрации";
            // 
            // valueComboBox
            // 
            this.valueComboBox.FormattingEnabled = true;
            this.valueComboBox.Location = new System.Drawing.Point(78, 74);
            this.valueComboBox.Name = "valueComboBox";
            this.valueComboBox.Size = new System.Drawing.Size(166, 21);
            this.valueComboBox.TabIndex = 3;
            // 
            // comparisonComboBox
            // 
            this.comparisonComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comparisonComboBox.FormattingEnabled = true;
            this.comparisonComboBox.Items.AddRange(new object[] {
            "равно",
            "не равно"});
            this.comparisonComboBox.Location = new System.Drawing.Point(78, 47);
            this.comparisonComboBox.Name = "comparisonComboBox";
            this.comparisonComboBox.Size = new System.Drawing.Size(166, 21);
            this.comparisonComboBox.TabIndex = 2;
            // 
            // parameterComboBox
            // 
            this.parameterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.parameterComboBox.FormattingEnabled = true;
            this.parameterComboBox.Location = new System.Drawing.Point(78, 20);
            this.parameterComboBox.Name = "parameterComboBox";
            this.parameterComboBox.Size = new System.Drawing.Size(166, 21);
            this.parameterComboBox.TabIndex = 1;
            this.parameterComboBox.SelectedValueChanged += new System.EventHandler(this.parameterComboBox_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Фильтр по:";
            // 
            // selectButton
            // 
            this.selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.selectButton.Location = new System.Drawing.Point(289, 378);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(100, 25);
            this.selectButton.TabIndex = 3;
            this.selectButton.Text = "Выбрать";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // ElementFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 415);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.showButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ElementFilterForm";
            this.ShowIcon = false;
            this.Text = "Выбор элементов на виде";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox;
        private System.Windows.Forms.Button showButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox valueComboBox;
        private System.Windows.Forms.ComboBox comparisonComboBox;
        private System.Windows.Forms.ComboBox parameterComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button selectButton;
    }
}