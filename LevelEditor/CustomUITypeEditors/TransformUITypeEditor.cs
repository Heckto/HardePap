using System;
using System.ComponentModel;
using drawing = System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace CustomUITypeEditors
{
    class TransformEditorControl : UserControl
    {
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox edtX;
        private TextBox edtSx;
        private TextBox edtSy;
        private TextBox edtY;
        private TextBox edtR;
        private Label label5;
        public Transform2 Value;

        public TransformEditorControl(Transform2 initialvalue)
        {
            InitializeComponent();
            Value = initialvalue;
            edtX.Text = Value.Position.X.ToString();
            edtY.Text = Value.Position.Y.ToString();
            edtSx.Text = Value.Scale.X.ToString();
            edtSy.Text = Value.Scale.Y.ToString();
            edtR.Text = Value.Rotation.ToString();
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.edtX = new System.Windows.Forms.TextBox();
            this.edtSx = new System.Windows.Forms.TextBox();
            this.edtSy = new System.Windows.Forms.TextBox();
            this.edtY = new System.Windows.Forms.TextBox();
            this.edtR = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
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
            this.label2.Location = new System.Drawing.Point(127, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "ScaleX";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(121, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "ScaleY";
            // 
            // edtX
            // 
            this.edtX.Location = new System.Drawing.Point(50, 18);
            this.edtX.Name = "edtX";
            this.edtX.Size = new System.Drawing.Size(64, 20);
            this.edtX.TabIndex = 4;
            this.edtX.TextChanged += new System.EventHandler(this.edtX_TextChanged);
            // 
            // edtSx
            // 
            this.edtSx.Location = new System.Drawing.Point(50, 44);
            this.edtSx.Name = "edtSx";
            this.edtSx.Size = new System.Drawing.Size(64, 20);
            this.edtSx.TabIndex = 5;
            this.edtSx.TextChanged += new System.EventHandler(this.edtW_TextChanged);
            // 
            // edtSy
            // 
            this.edtSy.Location = new System.Drawing.Point(168, 44);
            this.edtSy.Name = "edtSy";
            this.edtSy.Size = new System.Drawing.Size(43, 20);
            this.edtSy.TabIndex = 6;
            this.edtSy.TextChanged += new System.EventHandler(this.edtH_TextChanged);
            // 
            // edtY
            // 
            this.edtY.Location = new System.Drawing.Point(147, 18);
            this.edtY.Name = "edtY";
            this.edtY.Size = new System.Drawing.Size(64, 20);
            this.edtY.TabIndex = 7;
            this.edtY.TextChanged += new System.EventHandler(this.edtY_TextChanged);
            // 
            // edtR
            // 
            this.edtR.Location = new System.Drawing.Point(50, 70);
            this.edtR.Name = "edtR";
            this.edtR.Size = new System.Drawing.Size(64, 20);
            this.edtR.TabIndex = 9;
            this.edtR.TextChanged += new System.EventHandler(this.edtR_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Rotation";
            // 
            // TransformEditorControl
            // 
            this.Controls.Add(this.edtR);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.edtY);
            this.Controls.Add(this.edtSy);
            this.Controls.Add(this.edtSx);
            this.Controls.Add(this.edtX);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "TransformEditorControl";
            this.Size = new System.Drawing.Size(214, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void edtX_TextChanged(object sender, EventArgs e)
        {
            Value.Position = new Vector2(Convert.ToInt32(edtX.Text), Value.Position.Y);
        }

        private void edtY_TextChanged(object sender, EventArgs e)
        {
            Value.Position = new Vector2(Value.Position.X,Convert.ToInt32(edtY.Text));
        }
        private void edtW_TextChanged(object sender, EventArgs e)
        {
            Value.Scale = new Vector2((float)Convert.ToDouble(edtSx.Text),Value.Scale.Y);
        }

        private void edtH_TextChanged(object sender, EventArgs e)
        {
            Value.Scale = new Vector2(Value.Scale.X,(float)Convert.ToDouble(edtSy.Text));
        }

        private void edtR_TextChanged(object sender, EventArgs e)
        {
            Value.Rotation = (float)Convert.ToDouble(edtR.Text);
        }
    }



    public class TransformUITypeEditor : UITypeEditor
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
                var uc1 = new TransformEditorControl((Transform2)value);
                wfes.DropDownControl(uc1);
                value = uc1.Value;
            }
            return value;
        }
    }

        
    }
