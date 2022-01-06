
namespace OneMoreCalendar
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Windows.Forms;


	internal partial class DetailPagePanel : UserControl
	{
		public DetailPagePanel()
		{
			InitializeComponent();
		}


		public DetailPagePanel(IEnumerable<CalendarPage> pages)
			: this()
		{
			foreach (var page in pages)
			{
				var label = new MoreLinkLabel
				{
					AutoSize = true,
					TabStop = false,
					Text = page.Title,
					Tag = page
				};

				layout.Controls.Add(label);
			}
		}
	}
}
