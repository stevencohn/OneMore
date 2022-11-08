//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.ComponentModel;
	using System.Drawing;


	public class TableTheme : INotifyPropertyChanged, IDisposable
	{
		public sealed class ColorFont : IDisposable
		{
			public Font Font { get; set; }
			public Color Foreground { get; set; }

			public ColorFont()
			{
			}

			public ColorFont(ColorFont other)
			{
				if (other != null)
				{
					if (other.Font != null)
					{
						Font = new Font(other.Font, other.Font.Style);
					}

					Foreground = other.Foreground;
				}
			}

			public void Dispose()
			{
				Font?.Dispose();
			}

			public override bool Equals(object obj)
			{
				if (obj is ColorFont other)
				{
					return other.Font.Equals(Font) && other.Foreground == Foreground;
				}
				return false;
			}
			public override int GetHashCode()
			{
				return Font.GetHashCode() ^ Foreground.GetHashCode();
			}
			public override string ToString()
			{
				var name = Font?.FontFamily.Name ?? StyleBase.DefaultFontFamily;
				var size = Font?.SizeInPoints ?? StyleBase.DefaultFontSize;
				return $"{name}, {size.ToString("0.#", AddIn.Culture)}pt, {Foreground.ToNamedString()}";
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public static Color Rainbow => ColorTranslator.FromHtml("#12345678");

		/*
		Purple	Blue	Green	Yellow	Orange	Red	    Gray
		#E5E0EC	#DEEBF6	#E2EFD9	#FFF2CC	#FBE5D5	#FADBD2	#F2F2F2
		#B2A1C7	#9CC3E5	#A8D08D	#FFD965	#F4B183	#F1937A	#BFBFBF
		#8064A2	#5B9BD5	#70AD47	#FFC000	#ED7D31	#E84C22	#A5A5A5
		*/

		public static readonly string[] LightColorNames = new string[]
		{
			"#E5E0EC", "#DEEBF6", "#E2EFD9", "#FFF2CC", "#FBE5D5", "#FADBD2"
		};

		public static readonly string[] MediumColorNames = new string[]
		{
			"#B2A1C7", "#9CC3E5", "#A8D08D", "#FFD965", "#F4B183", "#F1937A"
		};


		private string name;

		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}


		// styles are applied in this order of the properties below
		// where lower styles override upper styles

		// *** DO NOT RE-ORDER THESE PROPERTIES ***

		public Color WholeTable { get; set; }

		public Color FirstColumnStripe { get; set; }

		public Color SecondColumnStripe { get; set; }

		public Color FirstRowStripe { get; set; }

		public Color SecondRowStripe { get; set; }

		public Color FirstColumn { get; set; }

		public Color LastColumn { get; set; }

		public Color HeaderRow { get; set; }

		public Color TotalRow { get; set; }

		public Color HeaderFirstCell { get; set; }

		public Color HeaderLastCell { get; set; }

		public Color TotalFirstCell { get; set; }

		public Color TotalLastCell { get; set; }


		public ColorFont DefaultFont { get; set; }

		public ColorFont HeaderFont { get; set; }

		public ColorFont TotalFont { get; set; }

		public ColorFont FirstColumnFont { get; set; }

		public ColorFont LastColumnFont { get; set; }


		public void CopyTo(TableTheme other)
		{
			other.Name = Name;
			other.WholeTable = WholeTable;
			other.FirstColumnStripe = FirstColumnStripe;
			other.SecondColumnStripe = SecondColumnStripe;
			other.FirstRowStripe = FirstRowStripe;
			other.SecondRowStripe = SecondRowStripe;
			other.FirstColumn = FirstColumn;
			other.LastColumn = LastColumn;
			other.HeaderRow = HeaderRow;
			other.TotalRow = TotalRow;
			other.HeaderFirstCell = HeaderFirstCell;
			other.HeaderLastCell = HeaderLastCell;
			other.TotalFirstCell = TotalFirstCell;
			other.TotalLastCell = TotalLastCell;

			other.DefaultFont?.Dispose();
			other.DefaultFont = DefaultFont == null ? null : new ColorFont(DefaultFont);

			other.HeaderFont?.Dispose();
			other.HeaderFont = HeaderFont == null ? null : new ColorFont(HeaderFont);

			other.TotalFont?.Dispose();
			other.TotalFont = TotalFont == null ? null : new ColorFont(TotalFont);

			other.FirstColumnFont?.Dispose();
			other.FirstColumnFont = FirstColumnFont == null ? null : new ColorFont(FirstColumnFont);

			other.LastColumnFont?.Dispose();
			other.LastColumnFont = LastColumnFont == null ? null : new ColorFont(LastColumnFont);
		}


		public void Dispose()
		{
			DefaultFont?.Dispose();
			HeaderFont?.Dispose();
			TotalFont?.Dispose();
			FirstColumnFont?.Dispose();
			LastColumnFont?.Dispose();
		}


		public override bool Equals(object obj)
		{
			if (obj is TableTheme other)
			{
				var same =
					other.Name == Name &&
					other.WholeTable.Equals(WholeTable) &&
					other.FirstColumnStripe.Equals(FirstColumnStripe) &&
					other.SecondColumnStripe.Equals(SecondColumnStripe) &&
					other.FirstRowStripe.Equals(FirstRowStripe) &&
					other.SecondRowStripe.Equals(SecondRowStripe) &&
					other.FirstColumn.Equals(FirstColumn) &&
					other.LastColumn.Equals(LastColumn) &&
					other.HeaderRow.Equals(HeaderRow) &&
					other.TotalRow.Equals(TotalRow) &&
					other.HeaderFirstCell.Equals(HeaderFirstCell) &&
					other.HeaderLastCell.Equals(HeaderLastCell) &&
					other.TotalFirstCell.Equals(TotalFirstCell) &&
					other.TotalLastCell.Equals(TotalLastCell);

				if (same)
				{
					same =
						(other.DefaultFont == null && DefaultFont == null) ||
						(other.DefaultFont is ColorFont cf && cf.Equals(DefaultFont));
				}

				if (same)
				{
					same =
						(other.HeaderFont == null && HeaderFont == null) ||
						(other.HeaderFont is ColorFont cf && cf.Equals(HeaderFont));
				}

				if (same)
				{
					same =
						(other.TotalFont == null && TotalFont == null) ||
						(other.TotalFont is ColorFont cf && cf.Equals(TotalFont));
				}

				if (same)
				{
					same =
						(other.FirstColumnFont == null && FirstColumnFont == null) ||
						(other.FirstColumnFont is ColorFont cf && cf.Equals(FirstColumnFont));
				}
				if (same)
				{
					same =
						(other.LastColumnFont == null && LastColumnFont == null) ||
						(other.LastColumnFont is ColorFont cf && cf.Equals(LastColumnFont));
				}

				return same;
			}

			return false;
		}


		public override int GetHashCode() => Name.GetHashCode();

		public override string ToString() => Name;
	}
}
