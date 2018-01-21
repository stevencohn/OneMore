namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class FlatToggleButton : UserControl
	{
		private bool hot;
		private bool isChecked;
		private Image image;


		public FlatToggleButton ()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}


		public bool Checked
		{
			get
			{
				return isChecked;
			}

			set
			{
				isChecked = value;
				Invalidate();
				OnClicked(new EventArgs());
			}
		}


		public event EventHandler CheckedChanged;

		protected virtual void OnClicked (EventArgs e)
		{
			CheckedChanged?.Invoke(this, e);
		}

		public Image Image
		{
			get
			{
				return image;
			}

			set
			{
				image = value;
				pictureBox.BackgroundImage = value;
			}
		}

		private void FlatToggleButton_MouseEnter (object sender, EventArgs e)
		{
			hot = true;
			Invalidate();
		}

		private void FlatToggleButton_MouseLeave (object sender, EventArgs e)
		{
			hot = false;
			Invalidate();
		}

		private void FlatToggleButton_Paint (object sender, PaintEventArgs e)
		{
			if (hot)
			{
				this.BackColor = SystemColors.ActiveBorder;
				this.BorderStyle = BorderStyle.FixedSingle;
			}
			else
			{
				if (Checked)
				{
					this.BackColor = SystemColors.ActiveBorder;
				}
				else
				{
					this.BackColor = SystemColors.Control;
				}

				BorderStyle = BorderStyle.None;
			}
		}

		private void pictureBox_MouseEnter (object sender, EventArgs e)
		{
			hot = true;
			Invalidate();
		}

		private void pictureBox_MouseLeave (object sender, EventArgs e)
		{
			hot = false;
			Invalidate();
		}

		private void pictureBox_Click (object sender, EventArgs e)
		{
			Checked = !Checked;
		}
	}
}
