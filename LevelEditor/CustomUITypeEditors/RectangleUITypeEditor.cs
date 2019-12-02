using System;
using System.ComponentModel;
using drawing = System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.Xna.Framework;

namespace CustomUITypeEditors
{



    class RectangleEditorControl : UserControl
    {
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox edtX;
        private TextBox edtW;
        private TextBox edtH;
        private TextBox edtY;
        public Rectangle Value;

        public RectangleEditorControl(Rectangle initialvalue)
        {
            InitializeComponent();
            Value = initialvalue;
            edtX.Text = Value.X.ToString();
            edtY.Text = Value.Y.ToString();
            edtW.Text = Value.Width.ToString();
            edtH.Text = Value.Height.ToString();
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.edtX = new System.Windows.Forms.TextBox();
            this.edtW = new System.Windows.Forms.TextBox();
            this.edtH = new System.Windows.Forms.TextBox();
            this.edtY = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Width";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Height";
            // 
            // edtX
            // 
            this.edtX.Location = new System.Drawing.Point(50, 18);
            this.edtX.Name = "edtX";
            this.edtX.Size = new System.Drawing.Size(64, 20);
            this.edtX.TabIndex = 4;
            this.edtX.TextChanged += new System.EventHandler(this.edtX_TextChanged);
            // 
            // edtW
            // 
            this.edtW.Location = new System.Drawing.Point(50, 63);
            this.edtW.Name = "edtW";
            this.edtW.Size = new System.Drawing.Size(64, 20);
            this.edtW.TabIndex = 5;
            this.edtW.TextChanged += new System.EventHandler(this.edtW_TextChanged);
            // 
            // edtH
            // 
            this.edtH.Location = new System.Drawing.Point(50, 87);
            this.edtH.Name = "edtH";
            this.edtH.Size = new System.Drawing.Size(64, 20);
            this.edtH.TabIndex = 6;
            this.edtH.TextChanged += new System.EventHandler(this.edtH_TextChanged);
            // 
            // edtY
            // 
            this.edtY.Location = new System.Drawing.Point(50, 40);
            this.edtY.Name = "edtY";
            this.edtY.Size = new System.Drawing.Size(64, 20);
            this.edtY.TabIndex = 7;
            this.edtY.TextChanged += new System.EventHandler(this.edtY_TextChanged);
            // 
            // RectangleEditorControl
            // 
            this.Controls.Add(this.edtY);
            this.Controls.Add(this.edtH);
            this.Controls.Add(this.edtW);
            this.Controls.Add(this.edtX);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "RectangleEditorControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void edtX_TextChanged(object sender, EventArgs e)
        {
            Value.X = Convert.ToInt32(edtX.Text);
        }

        private void edtY_TextChanged(object sender, EventArgs e)
        {
            Value.Y = Convert.ToInt32(edtY.Text);
        }

        private void edtW_TextChanged(object sender, EventArgs e)
        {
            Value.Width = Convert.ToInt32(edtW.Text);
        }

        private void edtH_TextChanged(object sender, EventArgs e)
        {
            Value.Height = Convert.ToInt32(edtH.Text);
        }
    }



    public class RectangleUITypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var wfes =
                provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                var uc1 = new RectangleEditorControl((Rectangle)value);
                wfes.DropDownControl(uc1);
                value = uc1.Value;
            }
            return value;
        }
    }

        
    }
