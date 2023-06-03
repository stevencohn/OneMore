namespace River.OneMoreAddIn.Commands.Navigator
{
    partial class NaviPages
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NaviPages));
            this.tBoxKw = new System.Windows.Forms.TextBox();
            this.btnJump = new System.Windows.Forms.Button();
            this.btnMove = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.hierarchyView11 = new River.OneMoreAddIn.UI.HierarchyView();
            this.SuspendLayout();
            // 
            // tBoxKw
            // 
            this.tBoxKw.Location = new System.Drawing.Point(63, 31);
            this.tBoxKw.Name = "tBoxKw";
            this.tBoxKw.Size = new System.Drawing.Size(631, 21);
            this.tBoxKw.TabIndex = 0;
            this.tBoxKw.TextChanged += new System.EventHandler(this.tBoxKw_TextChanged);
            // 
            // btnJump
            // 
            this.btnJump.Location = new System.Drawing.Point(65, 386);
            this.btnJump.Name = "btnJump";
            this.btnJump.Size = new System.Drawing.Size(89, 34);
            this.btnJump.TabIndex = 2;
            this.btnJump.Text = "JumpTo";
            this.btnJump.UseVisualStyleBackColor = true;
            // 
            // btnMove
            // 
            this.btnMove.Location = new System.Drawing.Point(312, 385);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(121, 35);
            this.btnMove.TabIndex = 3;
            this.btnMove.Text = "MoveTo";
            this.btnMove.UseVisualStyleBackColor = true;
            this.btnMove.Click += new System.EventHandler(this.ClickMoveTo);
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(575, 386);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(118, 34);
            this.btnMerge.TabIndex = 4;
            this.btnMerge.Text = "MergeTo";
            this.btnMerge.UseVisualStyleBackColor = true;
            // 
            // hierarchyView11
            // 
            this.hierarchyView11.CheckBoxes = true;
            this.hierarchyView11.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.hierarchyView11.HotTracking = true;
            this.hierarchyView11.Location = new System.Drawing.Point(65, 74);
            this.hierarchyView11.Name = "hierarchyView11";
            this.hierarchyView11.ShowLines = false;
            this.hierarchyView11.ShowRootLines = false;
            this.hierarchyView11.Size = new System.Drawing.Size(629, 305);
            this.hierarchyView11.Suspend = false;
            this.hierarchyView11.TabIndex = 1;
            this.hierarchyView11.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.CheckOneNode);
            this.hierarchyView11.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ClickNode);
            // 
            // NaviPages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 430);
            this.Controls.Add(this.hierarchyView11);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.btnMove);
            this.Controls.Add(this.btnJump);
            this.Controls.Add(this.tBoxKw);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NaviPages";
            this.Text = "NaviPages";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tBoxKw;
        private System.Windows.Forms.Button btnJump;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.Button btnMerge;
        private UI.HierarchyView hierarchyView11;
    }
}