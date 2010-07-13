namespace NewViewGenerator
{
    partial class AddViewDlg
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
            this.lblViewName = new System.Windows.Forms.Label();
            this.tbViewName = new System.Windows.Forms.TextBox();
            this.lblViewModel = new System.Windows.Forms.Label();
            this.comboModel = new System.Windows.Forms.ComboBox();
            this.lblBaseTemplate = new System.Windows.Forms.Label();
            this.chkInheritance = new System.Windows.Forms.CheckBox();
            this.checkedListBase = new System.Windows.Forms.CheckedListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.checkedListBlocks = new System.Windows.Forms.CheckedListBox();
            this.lblBlocks = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblViewName
            // 
            this.lblViewName.AutoSize = true;
            this.lblViewName.Location = new System.Drawing.Point(12, 9);
            this.lblViewName.Name = "lblViewName";
            this.lblViewName.Size = new System.Drawing.Size(64, 13);
            this.lblViewName.TabIndex = 0;
            this.lblViewName.Text = "View Name:";
            // 
            // tbViewName
            // 
            this.tbViewName.Location = new System.Drawing.Point(18, 26);
            this.tbViewName.Name = "tbViewName";
            this.tbViewName.Size = new System.Drawing.Size(267, 20);
            this.tbViewName.TabIndex = 1;
            // 
            // lblViewModel
            // 
            this.lblViewModel.AutoSize = true;
            this.lblViewModel.Location = new System.Drawing.Point(15, 53);
            this.lblViewModel.Name = "lblViewModel";
            this.lblViewModel.Size = new System.Drawing.Size(126, 13);
            this.lblViewModel.TabIndex = 2;
            this.lblViewModel.Text = "Select model for the view";
            // 
            // comboModel
            // 
            this.comboModel.FormattingEnabled = true;
            this.comboModel.Location = new System.Drawing.Point(18, 70);
            this.comboModel.Name = "comboModel";
            this.comboModel.Size = new System.Drawing.Size(267, 21);
            this.comboModel.TabIndex = 3;
            // 
            // lblBaseTemplate
            // 
            this.lblBaseTemplate.AutoSize = true;
            this.lblBaseTemplate.Location = new System.Drawing.Point(18, 147);
            this.lblBaseTemplate.Name = "lblBaseTemplate";
            this.lblBaseTemplate.Size = new System.Drawing.Size(138, 13);
            this.lblBaseTemplate.TabIndex = 4;
            this.lblBaseTemplate.Text = "Select  templates to extend:";
            // 
            // chkInheritance
            // 
            this.chkInheritance.AutoSize = true;
            this.chkInheritance.Location = new System.Drawing.Point(18, 109);
            this.chkInheritance.Name = "chkInheritance";
            this.chkInheritance.Size = new System.Drawing.Size(180, 17);
            this.chkInheritance.TabIndex = 5;
            this.chkInheritance.Text = "The template will use inheritance";
            this.chkInheritance.UseVisualStyleBackColor = true;
            this.chkInheritance.CheckStateChanged += new System.EventHandler(this.chkInheritance_CheckStateChanged);
            // 
            // checkedListBase
            // 
            this.checkedListBase.Enabled = false;
            this.checkedListBase.FormattingEnabled = true;
            this.checkedListBase.Location = new System.Drawing.Point(18, 172);
            this.checkedListBase.Name = "checkedListBase";
            this.checkedListBase.Size = new System.Drawing.Size(242, 79);
            this.checkedListBase.TabIndex = 6;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(18, 297);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(89, 35);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // checkedListBlocks
            // 
            this.checkedListBlocks.FormattingEnabled = true;
            this.checkedListBlocks.Location = new System.Drawing.Point(306, 172);
            this.checkedListBlocks.Name = "checkedListBlocks";
            this.checkedListBlocks.Size = new System.Drawing.Size(166, 154);
            this.checkedListBlocks.TabIndex = 8;
            this.checkedListBlocks.Visible = false;
            // 
            // lblBlocks
            // 
            this.lblBlocks.AutoSize = true;
            this.lblBlocks.Location = new System.Drawing.Point(303, 147);
            this.lblBlocks.Name = "lblBlocks";
            this.lblBlocks.Size = new System.Drawing.Size(134, 13);
            this.lblBlocks.TabIndex = 9;
            this.lblBlocks.Text = "Choose Blocks to override:";
            this.lblBlocks.Visible = false;
            // 
            // AddViewDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 345);
            this.Controls.Add(this.lblBlocks);
            this.Controls.Add(this.checkedListBlocks);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.checkedListBase);
            this.Controls.Add(this.chkInheritance);
            this.Controls.Add(this.lblBaseTemplate);
            this.Controls.Add(this.comboModel);
            this.Controls.Add(this.lblViewModel);
            this.Controls.Add(this.tbViewName);
            this.Controls.Add(this.lblViewName);
            this.Location = new System.Drawing.Point(50, 50);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddViewDlg";
            this.Text = "Add Django View";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblViewName;
        private System.Windows.Forms.TextBox tbViewName;
        private System.Windows.Forms.Label lblViewModel;
        private System.Windows.Forms.ComboBox comboModel;
        private System.Windows.Forms.Label lblBaseTemplate;
        private System.Windows.Forms.CheckBox chkInheritance;
        private System.Windows.Forms.CheckedListBox checkedListBase;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.CheckedListBox checkedListBlocks;
        private System.Windows.Forms.Label lblBlocks;
    }
}