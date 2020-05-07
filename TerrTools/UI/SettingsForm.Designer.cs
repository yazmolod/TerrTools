namespace TerrTools.UI
{
    partial class SettingsForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.UpdatersPage = new System.Windows.Forms.TabPage();
            this.runCurrentButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.VersionPage = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.updateButton = new System.Windows.Forms.Button();
            this.versionLabel = new System.Windows.Forms.Label();
            this.RadiatorSettingsPage = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lengthPlusBtn = new System.Windows.Forms.Button();
            this.lengthMinusBtn = new System.Windows.Forms.Button();
            this.lengthLE = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lengthListBox = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.heightPlusBtn = new System.Windows.Forms.Button();
            this.heightMinusBtn = new System.Windows.Forms.Button();
            this.heightLE = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.heightListBox = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.typePlusBtn = new System.Windows.Forms.Button();
            this.typeMinusBtn = new System.Windows.Forms.Button();
            this.typeLE = new System.Windows.Forms.MaskedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.typeListBox = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.UpdatersPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.VersionPage.SuspendLayout();
            this.RadiatorSettingsPage.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.UpdatersPage);
            this.tabControl1.Controls.Add(this.VersionPage);
            this.tabControl1.Controls.Add(this.RadiatorSettingsPage);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(689, 375);
            this.tabControl1.TabIndex = 0;
            // 
            // UpdatersPage
            // 
            this.UpdatersPage.Controls.Add(this.runCurrentButton);
            this.UpdatersPage.Controls.Add(this.dataGridView1);
            this.UpdatersPage.Location = new System.Drawing.Point(4, 22);
            this.UpdatersPage.Name = "UpdatersPage";
            this.UpdatersPage.Padding = new System.Windows.Forms.Padding(3);
            this.UpdatersPage.Size = new System.Drawing.Size(681, 349);
            this.UpdatersPage.TabIndex = 0;
            this.UpdatersPage.Text = "Updaters";
            this.UpdatersPage.UseVisualStyleBackColor = true;
            // 
            // runCurrentButton
            // 
            this.runCurrentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.runCurrentButton.Location = new System.Drawing.Point(516, 320);
            this.runCurrentButton.Name = "runCurrentButton";
            this.runCurrentButton.Size = new System.Drawing.Size(156, 23);
            this.runCurrentButton.TabIndex = 1;
            this.runCurrentButton.Text = "Выполнить выделенное";
            this.runCurrentButton.UseVisualStyleBackColor = true;
            this.runCurrentButton.Click += new System.EventHandler(this.runCurrentButton_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(669, 308);
            this.dataGridView1.TabIndex = 0;
            // 
            // VersionPage
            // 
            this.VersionPage.Controls.Add(this.richTextBox1);
            this.VersionPage.Controls.Add(this.updateButton);
            this.VersionPage.Controls.Add(this.versionLabel);
            this.VersionPage.Location = new System.Drawing.Point(4, 22);
            this.VersionPage.Name = "VersionPage";
            this.VersionPage.Padding = new System.Windows.Forms.Padding(3);
            this.VersionPage.Size = new System.Drawing.Size(681, 349);
            this.VersionPage.TabIndex = 1;
            this.VersionPage.Text = "Версия";
            this.VersionPage.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(14, 47);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(658, 296);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // updateButton
            // 
            this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateButton.Location = new System.Drawing.Point(238, 365);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(156, 23);
            this.updateButton.TabIndex = 2;
            this.updateButton.Text = "Проверить обновления";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(11, 21);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(35, 13);
            this.versionLabel.TabIndex = 0;
            this.versionLabel.Text = "label1";
            // 
            // RadiatorSettingsPage
            // 
            this.RadiatorSettingsPage.Controls.Add(this.panel3);
            this.RadiatorSettingsPage.Controls.Add(this.panel2);
            this.RadiatorSettingsPage.Controls.Add(this.panel1);
            this.RadiatorSettingsPage.Location = new System.Drawing.Point(4, 22);
            this.RadiatorSettingsPage.Name = "RadiatorSettingsPage";
            this.RadiatorSettingsPage.Size = new System.Drawing.Size(681, 349);
            this.RadiatorSettingsPage.TabIndex = 2;
            this.RadiatorSettingsPage.Text = "Подбор радиаторов";
            this.RadiatorSettingsPage.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel3.Controls.Add(this.lengthPlusBtn);
            this.panel3.Controls.Add(this.lengthMinusBtn);
            this.panel3.Controls.Add(this.lengthLE);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.lengthListBox);
            this.panel3.Location = new System.Drawing.Point(452, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(226, 343);
            this.panel3.TabIndex = 6;
            // 
            // lengthPlusBtn
            // 
            this.lengthPlusBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lengthPlusBtn.Image = global::TerrTools.Properties.Resources.icons8_плюс_40;
            this.lengthPlusBtn.Location = new System.Drawing.Point(170, 315);
            this.lengthPlusBtn.Name = "lengthPlusBtn";
            this.lengthPlusBtn.Size = new System.Drawing.Size(53, 23);
            this.lengthPlusBtn.TabIndex = 4;
            this.lengthPlusBtn.UseVisualStyleBackColor = true;
            this.lengthPlusBtn.Click += new System.EventHandler(this.PlusBtn_Click);
            this.lengthPlusBtn.Click += new System.EventHandler(this.UpdateSettings);
            // 
            // lengthMinusBtn
            // 
            this.lengthMinusBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lengthMinusBtn.Image = global::TerrTools.Properties.Resources.icons8_минус_40;
            this.lengthMinusBtn.Location = new System.Drawing.Point(112, 315);
            this.lengthMinusBtn.Name = "lengthMinusBtn";
            this.lengthMinusBtn.Size = new System.Drawing.Size(53, 23);
            this.lengthMinusBtn.TabIndex = 3;
            this.lengthMinusBtn.UseVisualStyleBackColor = true;
            this.lengthMinusBtn.Click += new System.EventHandler(this.MinusBtn_Click);
            this.lengthMinusBtn.Click += new System.EventHandler(this.UpdateSettings);
            // 
            // lengthLE
            // 
            this.lengthLE.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lengthLE.Location = new System.Drawing.Point(5, 316);
            this.lengthLE.Mask = "0000";
            this.lengthLE.Name = "lengthLE";
            this.lengthLE.Size = new System.Drawing.Size(100, 20);
            this.lengthLE.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Допустимые длины";
            // 
            // lengthListBox
            // 
            this.lengthListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lengthListBox.FormattingEnabled = true;
            this.lengthListBox.Location = new System.Drawing.Point(5, 27);
            this.lengthListBox.Name = "lengthListBox";
            this.lengthListBox.Size = new System.Drawing.Size(218, 277);
            this.lengthListBox.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel2.Controls.Add(this.heightPlusBtn);
            this.panel2.Controls.Add(this.heightMinusBtn);
            this.panel2.Controls.Add(this.heightLE);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.heightListBox);
            this.panel2.Location = new System.Drawing.Point(228, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(226, 343);
            this.panel2.TabIndex = 5;
            // 
            // heightPlusBtn
            // 
            this.heightPlusBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.heightPlusBtn.Image = global::TerrTools.Properties.Resources.icons8_плюс_40;
            this.heightPlusBtn.Location = new System.Drawing.Point(170, 315);
            this.heightPlusBtn.Name = "heightPlusBtn";
            this.heightPlusBtn.Size = new System.Drawing.Size(53, 23);
            this.heightPlusBtn.TabIndex = 4;
            this.heightPlusBtn.UseVisualStyleBackColor = true;
            this.heightPlusBtn.Click += new System.EventHandler(this.PlusBtn_Click);
            this.heightPlusBtn.Click += new System.EventHandler(this.UpdateSettings);
            // 
            // heightMinusBtn
            // 
            this.heightMinusBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.heightMinusBtn.Image = global::TerrTools.Properties.Resources.icons8_минус_40;
            this.heightMinusBtn.Location = new System.Drawing.Point(112, 315);
            this.heightMinusBtn.Name = "heightMinusBtn";
            this.heightMinusBtn.Size = new System.Drawing.Size(53, 23);
            this.heightMinusBtn.TabIndex = 3;
            this.heightMinusBtn.UseVisualStyleBackColor = true;
            this.heightMinusBtn.Click += new System.EventHandler(this.MinusBtn_Click);
            this.heightMinusBtn.Click += new System.EventHandler(this.UpdateSettings);
            // 
            // heightLE
            // 
            this.heightLE.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.heightLE.Location = new System.Drawing.Point(5, 316);
            this.heightLE.Mask = "0000";
            this.heightLE.Name = "heightLE";
            this.heightLE.Size = new System.Drawing.Size(100, 20);
            this.heightLE.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Допустимые высоты";
            // 
            // heightListBox
            // 
            this.heightListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.heightListBox.FormattingEnabled = true;
            this.heightListBox.Location = new System.Drawing.Point(5, 27);
            this.heightListBox.Name = "heightListBox";
            this.heightListBox.Size = new System.Drawing.Size(218, 277);
            this.heightListBox.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.typePlusBtn);
            this.panel1.Controls.Add(this.typeMinusBtn);
            this.panel1.Controls.Add(this.typeLE);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.typeListBox);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(226, 343);
            this.panel1.TabIndex = 1;
            // 
            // typePlusBtn
            // 
            this.typePlusBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.typePlusBtn.Image = global::TerrTools.Properties.Resources.icons8_плюс_40;
            this.typePlusBtn.Location = new System.Drawing.Point(170, 315);
            this.typePlusBtn.Name = "typePlusBtn";
            this.typePlusBtn.Size = new System.Drawing.Size(53, 23);
            this.typePlusBtn.TabIndex = 4;
            this.typePlusBtn.UseVisualStyleBackColor = true;
            this.typePlusBtn.Click += new System.EventHandler(this.PlusBtn_Click);
            this.typePlusBtn.Click += new System.EventHandler(this.UpdateSettings);
            // 
            // typeMinusBtn
            // 
            this.typeMinusBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.typeMinusBtn.Image = global::TerrTools.Properties.Resources.icons8_минус_40;
            this.typeMinusBtn.Location = new System.Drawing.Point(112, 315);
            this.typeMinusBtn.Name = "typeMinusBtn";
            this.typeMinusBtn.Size = new System.Drawing.Size(53, 23);
            this.typeMinusBtn.TabIndex = 3;
            this.typeMinusBtn.UseVisualStyleBackColor = true;
            this.typeMinusBtn.Click += new System.EventHandler(this.MinusBtn_Click);
            this.typeMinusBtn.Click += new System.EventHandler(this.UpdateSettings);
            // 
            // typeLE
            // 
            this.typeLE.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typeLE.Location = new System.Drawing.Point(5, 316);
            this.typeLE.Mask = "00";
            this.typeLE.Name = "typeLE";
            this.typeLE.Size = new System.Drawing.Size(100, 20);
            this.typeLE.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Тип радиаторов";
            // 
            // typeListBox
            // 
            this.typeListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typeListBox.FormattingEnabled = true;
            this.typeListBox.Location = new System.Drawing.Point(5, 27);
            this.typeListBox.Name = "typeListBox";
            this.typeListBox.Size = new System.Drawing.Size(218, 277);
            this.typeListBox.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(601, 381);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Закрыть";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(520, 381);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Принять";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 416);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tabControl1);
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.Text = "Настройки ТеррНИИ BIM";
            this.tabControl1.ResumeLayout(false);
            this.UpdatersPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.VersionPage.ResumeLayout(false);
            this.VersionPage.PerformLayout();
            this.RadiatorSettingsPage.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage VersionPage;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.TabPage UpdatersPage;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button runCurrentButton;
        private System.Windows.Forms.TabPage RadiatorSettingsPage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button typeMinusBtn;
        private System.Windows.Forms.MaskedTextBox typeLE;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox typeListBox;
        private System.Windows.Forms.Button typePlusBtn;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button lengthPlusBtn;
        private System.Windows.Forms.Button lengthMinusBtn;
        private System.Windows.Forms.MaskedTextBox lengthLE;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lengthListBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button heightPlusBtn;
        private System.Windows.Forms.Button heightMinusBtn;
        private System.Windows.Forms.MaskedTextBox heightLE;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox heightListBox;
    }
}