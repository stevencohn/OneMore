//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S125        // Sections of code should not be commented out

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Helpers.Office;
	using System.Linq;


	public partial class AddIn
	{
		private const int BodyContext = 0x01;
		private const int ImageContext = 0x02;

		private int context = 0;
		private int prevContext = 0;


		/// <summary>
		/// Analyze the current page and determine if the text cursor is positioned in the body
		/// of the page or in the title.
		/// </summary>
		/// <param name="control">
		/// The menu item invoking the action. This should be done by one "controller" item so
		/// optimize performance. All other menu items can rely on the GetBodyContext method.
		/// </param>
		/// <returns>True</returns>
		public bool SetBodyContext(IRibbonControl control)
		{
			using (var one = new OneNote(out var page, out _))
			{
				// set the context for the getters
				if (page.ConfirmBodyContext())
					context |= BodyContext;
				else
					context &= ~BodyContext;

				if (page.ConfirmImageSelected())
					context |= ImageContext;
				else
					context &= ~ImageContext;
			}

			logger.WriteLine($"SetBodyContext({control.Id}) context:{context}");

			// the setter always returns true; the getter will return bodyContext
			return true;
		}


		/// <summary>
		/// Gets a Boolean value indicating whether the text cursor is positioned in the body of the page.
		/// </summary>
		/// <param name="control">The menu item invoking the action.</param>
		/// <returns>True if the text cursor is position in the body.</returns>
		public bool GetBodyContext(IRibbonControl control)
		{
			if (context != prevContext)
			{
				logger.WriteLine($"GetBodyContext({control.Id}) context:{context}");
				ribbon.Invalidate();
				prevContext = context;
			}

			return (context & BodyContext) > 0;
		}


		/// <summary>
		/// Gets a Boolean value indicating whether at least one image is selected on the page
		/// </summary>
		/// <param name="control">The menu item invoking the action.</param>
		/// <returns>True if at least one image is selected on the current page.</returns>
		public bool GetImageSelected(IRibbonControl control)
		{

			if (context != prevContext)
			{
				logger.WriteLine($"GetImageSelected({control.Id}) context:{context}");
				ribbon.Invalidate();
				prevContext = context;
			}

			return (context & ImageContext) > 0;
		}


		/// <summary>
		/// Gets a Boolean value indicating whether the item should be shown
		/// </summary>
		/// <param name="control">The menu item invoking the action</param>
		/// <returns>True if the item should be shown; false otherwise</returns>
		public bool GetItemVisible(IRibbonControl control)
		{
			//logger.WriteLine($"GetItemVisible({control.Id})");

			if (control.Id == "ribPronunciateButton")
			{
				return System.Environment.GetEnvironmentVariable("USERNAME") == "steven";
			}

			return true;
		}


		/// <summary>
		/// Analyze the current page selections and determined if at least two pages are selected.
		/// </summary>
		/// <param name="control">The menu item invoking the action.</param>
		/// <returns>True if two or more pages are selected.</returns>
		public bool GetMultiPageContext(IRibbonControl control)
		{
			logger.WriteLine($"GetMultiPageContext({control.Id})");

			using (var one = new OneNote())
			{
				var section = one.GetSection();
				var sns = one.GetNamespace(section);

				var count =
					section.Elements(sns + "Page")
					.Count(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

				return count > 1;
			}
		}


		/// <summary>
		/// Gets a Boolean value indicating whether Word or PowerPoint is installed.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public bool GetOfficeInstalled(IRibbonControl control)
		{
			logger.WriteLine($"GetOfficeInstalled({control.Id})");

			return Office.IsWordInstalled() || Office.IsPowerPointInstalled();
		}
	}
}
