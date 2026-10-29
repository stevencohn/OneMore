//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using River.OneMoreAddIn.Styles;


	/// <summary>
	/// Uses a rotating array of colors to highlight selected text.
	/// </summary>
	internal class HighlightCommand : Command
	{
		public HighlightCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var increment = (int)args[0];

			await using var one = new OneNote(out var page, out _);

			var (index, color) = increment < 0
				? (0, StyleBase.Transparent)
				: HighlightRotator.GetColor(page, advance: increment > 0);

			if (HighlightRotator.Apply(page, color))
			{
				if (increment > 0)
				{
					HighlightRotator.SaveIndex(page, index);
				}

				await one.Update(page);
			}
		}
	}
}
