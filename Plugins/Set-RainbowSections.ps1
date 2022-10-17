[CmdletBinding(SupportsShouldProcess = $true)]

param (
	[Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
	[string] $Path
)

Begin
{
	$assemblies = ('System.Xml', 'System.Xml.Linq')

	$code = @'
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Plugins
{
	public class Colorizer
	{
		private static string[] colors = new string[]
		{
			"#AB9CB7", "#8AA8E4", "#91BAAE", "#ADE792",
			"#B7C997", "#F5F96F", "#FFD869", "#F6B078",
			"#E8D199", "#EE9597", "#D5A4BB", "#B49EDE",
			"#9595AA"
		};

		public static void Colorize(string path)
		{
			var root = XElement.Load(path);
			var ns = root.GetNamespaceOfPrefix("one");
			Colorize(root, ns, root.Attribute("name").Value);			
			root.Save(path, SaveOptions.DisableFormatting);
		}

		static void Colorize(XElement node, XNamespace ns, string prefix)
		{
			var c = 0;
			foreach (var section in node.Elements(ns + "Section")
				.Where(n => n.Attribute("isInRecycleBin") == null))
			{
				var name = section.Attribute("name").Value;
				var color = colors[c++];
				section.Attribute("color").Value = color;
				Console.WriteLine(prefix + "/" + name + " (" + color + ")");

				if (c >= colors.Length) { c = 0; }
			}
	
			foreach (var group in node.Elements(ns + "SectionGroup")
				.Where(n => n.Attribute("isInRecycleBin") == null))
			{
				Colorize(group, ns, group.Attribute("name").Value);
			}
		}
	}
}
'@
}
Process
{
	$filepath = Resolve-Path $Path -ErrorAction SilentlyContinue
	if (!$filepath)
	{
		Write-Host "Could not find file $Path"
		return
	}

	Add-Type -ReferencedAssemblies $assemblies -TypeDefinition $code
	[Plugins.Colorizer]::Colorize($filepath)
	
	Write-Host 'Done'
}
