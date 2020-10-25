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


		public FlatToggleButton()
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

		protected virtual void OnClicked(EventArgs e)
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

		private void MakeItHot(object sender, EventArgs e)
		{
			hot = true;
			Invalidate();
		}

		private void MakeItCool(object sender, EventArgs e)
		{
			hot = false;
			Invalidate();
		}

		private void FlatToggleButton_Paint(object sender, PaintEventArgs e)
		{
			if (hot)
			{
				BackColor = SystemColors.ActiveBorder;
				BorderStyle = BorderStyle.FixedSingle;
			}
			else
			{
				if (Checked)
				{
					BackColor = SystemColors.ActiveBorder;
				}
				else
				{
					BackColor = SystemColors.Control;
				}

				BorderStyle = BorderStyle.None;
			}
		}

		private void ClickPictureBox(object sender, EventArgs e)
		{
			Checked = !Checked;
		}
	}
}
