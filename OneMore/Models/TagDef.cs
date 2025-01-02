//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class TagDef : XElement
	{
		private const int ToDoSymbol = 3;
		private const int DiscussSymbol = 94;
		private const int ManagerSymbol = 95;
		private const int ScheduleSymbol = 12;
		private const int Priority1Symbol = 28;
		private const int Priority2Symbol = 71;
		private const int ClientSymbol = 8;

		private int indexValue;
		private readonly int hashcode;


		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="original">Original TagDef to copy</param>
		public TagDef(XElement original)
			: base(original.GetNamespaceOfPrefix(OneNote.Prefix) + "TagDef",
				  original.Attributes())
		{
			ElementRef = original;
			IndexValue = int.Parse(original.Attribute("index").Value);
			Type = int.Parse(Attribute("type").Value);
			hashcode = original.GetHashCode();
		}


		/// <summary>
		/// Dynamic, indicates the order in which the tagdefs were added to the page
		/// </summary>
		public string Index => Attribute("index").Value;


		/// <summary>
		/// Gets the original XElement reference.
		/// </summary>
		public XElement ElementRef { get; private set; }


		public int IndexValue
		{
			get => indexValue;

			set
			{
				indexValue = value;
				Attribute("index").Value = value.ToString();
			}
		}


		/// <summary>
		/// Gets the unique font color of this symbol tag
		/// </summary>
		public string FontColor => Attribute("fontColor").Value;


		/// <summary>
		/// Gets the unique highlight color of this symbol tag
		/// </summary>
		public string HighlightColor => Attribute("highlightColor").Value;


		/// <summary>
		/// Gets the specific glyph used for the tag
		/// </summary>
		public string Symbol => Attribute("symbol").Value;


		/// <summary>
		/// Dynamic, can differ page to page, seems meaningless??
		/// </summary>
		public int Type { get; private set; }


		/// <summary>
		/// Compares by Symbol
		/// </summary>
		/// <param name="obj">other instance</param>
		/// <returns>True if the Symbols are equal</returns>
		public override bool Equals(object obj)
		{
			if (obj is XElement other && other.Name.LocalName == "TagDef")
			{
				// OneNote typically allows one of each symbol, however, you can have multiple
				// instances of each symbol as long as the font/highlight colors are unique

				return
					Symbol.Equals(other.Attribute("symbol").Value) &&
					FontColor.Equals(other.Attribute("fontColor").Value) &&
					HighlightColor.Equals(other.Attribute("highlightColor").Value);
			}

			return false;
		}


		public override int GetHashCode() => hashcode;


		public bool IsToDo()
		{
			return TagDef.IsToDo(int.Parse(Symbol));
		}


		public static bool IsToDo(int symbol)
		{
			return symbol == ToDoSymbol
				|| symbol == DiscussSymbol
				|| symbol == ManagerSymbol
				|| symbol == ScheduleSymbol
				|| symbol == Priority1Symbol
				|| symbol == Priority2Symbol
				|| symbol == ClientSymbol;
		}
	}
}
/*
idx	typ	sym	default	Tag
--- --- --- ------- ------------
0	0	33			Num three --> :three:
1	1	51			Num Two --> :two:
2	2	70			Num One --> :one:
3	3	39			Num Zero --> :zero:
4	4	3	false	To Do --> [x]
5	5	13			Important --> :star:
6	6	15			Question --> :question:
7	7	0			Remember for later
8	8	0			Definition
9	9	136			Highlight
10	10	118			Contact --> :mailbox:
11	11	23			Address --> :house:
12	12	18			Phone number --> :phone:
13	13	125			Web site to visit
14	14	21			Idea --> :bulb:
15	15	131			Password --> :secret:
16	16	17			Critical --> :exclamation:
17	17	100			Project A
18	18	101			Project B
19	19	122			Movie to see --> :movie_camera:
20	20	132			Book to read --> :book:
21	21	121			Music to listen to --> :musical_note:
22	22	125			Source for article
23	23	24			Remember for blog
24	24	94	false	Discuss with <Person A>
25	25	94	false	Discuss with <Person B>
26	26	95	false	Discuss with Manager
27	27	106			Send in email
28	28	12	false	Schedule meeting
29	29	12	false	Call back
30	30	28	false	To Do priority 1
31	31	71	false	To Do priority 2
32	32	8	false	Client request
        140         lightning bolt --> :zap:
*/