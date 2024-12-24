using System.Collections.Generic;

namespace River.OneMoreAddIn.Commands
{
	public static class MarkdownEmojis
	{
		public static List<(string name, string id, string topic, int type)> taglist = new List<(string name, string id, string topic, int type)>
							{
//								(":todo:",                  "3",   "todo"       , 0),
								(":question:",              "6",    "question"  , 0),
								(":star:",                  "13",   "important", 0 ),
								(":exclamation:",           "17",   "critical", 0),
								(":phone:",                 "18",   "phone", 0),
								(":bulb:",                  "21",   "idea", 0),
								(":house:",                 "23",   "address", 0),
								(":three:",                 "33",   "three", 0),
								(":zero:",                  "39",   "zero", 0),
								(":two:",                   "51",   "two", 0),
								(":arrow_right:",           "59",   "main agenda item", 0),
								(":one:",                   "70",   "one", 0),
								(":information_desk_person:","94",   "discuss person a/b", 21),
								(":bellsymbol:",             "97",   "bellsymbol", 0),
								(":busts_in_silhouette:",   "116",   "busts_in_silhouette", 0),
								(":bell:",                  "117",   "bell", 0),
								(":letter:",                "118",   "letter", 0),
								(":musical_note:",          "121",   "musical_note", 0),
								(":secret:",                "131",   "idea", 0),
								(":book:",                  "132",   "book", 0),
								(":movie_camera:",          "133",   "movie_camera", 0),
								(":zap:",                   "140",   "lightning_bolt", 0),
								(":o:",                     "1",    "default", 0)
							};


	}
}
