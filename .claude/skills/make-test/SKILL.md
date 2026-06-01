# make-test — Generate a unit test from a manual test protocol

The user has pasted a manual test spec into the conversation. Your job is to
translate it into one or more MSTest unit test methods in the OneMoreTests project.

---

## Step 1 — Parse the spec

Extract the following from the spec text:

**Command identity**
- Command category and name will be in the spec file, e.g. Commands/Edit/AlterSizeCommand.
- Map the name to a C# class name: AlterSizeCommand → `IncreaseFontSizeCommand.cs`.
- Identify the category folder: "More/Edit/..." → `Commands/Edit/`.
- If there are two paired commands (increase/decrease, add/remove), plan tests for both.

**Steps**
- Odd-numbered steps are **actions** — they invoke a command.
- Even-numbered steps are **assertions** — they describe what to verify.
- "Confirm X" or "Verify X" wording in a step = an assertion, regardless of position.

**Boundary conditions**
- Lines stating "Min X is N" or "Max X is N" → generate dedicated boundary test methods.
- Each boundary needs two tests: at the limit (no change expected) and just inside it.

**Constraints**
- Lines like "Cannot be constrained to a selection" mean the command ignores selection state;
  the test page does not need any `selected="all"` attributes.

---

## Step 2 — Find the command

Search `OneMore/Commands/**/*Command.cs` for the class name identified above.
Read the implementation to understand:

- What `Execute(params object[] args)` receives (increment, index, etc.)
- Which XML elements or attributes it modifies — there are three common patterns:

  | Pattern | XML location | How to assert |
  |---------|-------------|---------------|
  | Named attribute | `<one:QuickStyleDef fontSize="11.0"/>` | Read `@fontSize` from `QuickStyleDef` descendants |
  | Inline CSS on element | `<one:OE style="font-size:12.0pt">` | Parse `style` attribute of OE/T descendants |
  | CSS inside CDATA span | `<![CDATA[<span style="font-size:12.0pt">text</span>]]>` | Read the XCData value as a string and search for `font-size:` |

- Whether it forces a full page update (`one.Update(page, force: true)`), which means
  `GetUpdatedPage` will always return a result when the page has matching content.

- Confirm that the command can be run through its Execute method without UI intervention.
  If not, terminate the task and report that the test cannot be automated.
  Instruct the user to refactor the command to separate the logic into a testable method that can 
  be called from Execute.
---

## Step 3 — Design the test cases

Produce one `[TestMethod]` per logical scenario. Typical set:

1. **Happy-path increase** — page has content at a known size; run increase; verify size went up by 1.
2. **Happy-path decrease** — same pattern in the other direction.
3. **At maximum, increase clamps** — set content at max value; run increase; verify size unchanged.
4. **At minimum, decrease clamps** — set content at min value; run decrease; verify size unchanged.
5. **No content, no update** — empty page; verify `GetUpdatedPage` returns null (UpdatePageContent never called).

Add or remove cases based on what the spec actually says.

---

## Step 4 — Write the test file

Create `OneMoreTests/Commands/<Category>/<CommandName>Tests.cs`.

Follow the exact pattern of `OneMoreTests/Commands/Edit/HighlightCommandTests.cs`
and paste the test spec in the comment block above the class declaration for reference.


```csharp
namespace River.OneMoreAddIn.Tests.Commands.<Category>
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using River.OneMoreAddIn.Commands;
    using River.OneMoreAddIn.Tests.Builders;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /*
     * <paste test spec here>
     */

    [TestClass]
    public class <CommandName>Tests : TestBase
    {
        private const string PageId = "page-1";
        private static readonly XNamespace Ns =
            "http://schemas.microsoft.com/office/onenote/2013/onenote";

        [TestMethod]
        public async Task <MethodName>()
        {
            // Arrange
            var xml = new PageBuilder(PageId, "<title>")
                .WithParagraph("text")
                .Build();
            SetupPage(PageId, xml);

            // Act
            await new <CommandName>().Execute(/* args */);

            // Assert
            var updated = GetUpdatedPage(PageId);
            Assert.IsNotNull(updated, "UpdatePageContent was never called");
            // ... specific assertions on updated XElement
        }
    }
}
```

**Assertion helpers for the three XML patterns:**

```csharp
// Named fontSize attribute on QuickStyleDef or Bullet
var size = (double?)updated.Descendants(Ns + "QuickStyleDef")
    .FirstOrDefault()?.Attribute("fontSize");

// font-size in an OE/T style attribute
var style = (string)updated.Descendants(Ns + "OE").First().Attribute("style") ?? "";
// then: StringAssert.Contains(style, "font-size:12.0pt");

// font-size inside CDATA span
var cdata = updated.Descendants(Ns + "T")
    .Select(t => t.FirstNode as XCData)
    .FirstOrDefault(cd => cd?.Value.Contains("font-size:") == true);
// then: StringAssert.Contains(cdata.Value, "font-size:12.0pt");
```

---

## Step 5 — Register the file

Add a `<Compile>` entry to `OneMoreTests/OneMoreTests.csproj` in the same
`<ItemGroup>` as the other test files:

```xml
<Compile Include="Commands\<Category>\<CommandName>Tests.cs" />
```

---

## Step 6 — Build and run

Build with PowerShell script at root of project:
```
.\build.ps1 -fast
```

Run with:
```
$vstest = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
& $vstest "C:\Github\OneMore\OneMoreTests\bin\Debug\OneMoreTests.dll" /Platform:x64
```

All new tests must pass before reporting the task complete.
