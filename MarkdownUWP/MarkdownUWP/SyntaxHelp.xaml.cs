using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MarkdownUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SyntaxHelp : Page
    {
        public SyntaxHelp()
        {
            this.InitializeComponent();
            HeadersInfo.Text =
                "Headers are the titles for Pages and sections, 1 is largest 6 is smallest\n" +
                "Insert a hash (#) at the start of a line, followed by a space\n\n";
            HeadersSyntax.Text =
                "# Heading 1\n" +
                "## Heading 2\n" +
                "### Heading 3\n" +
                "#### Heading 4\n" +
                "##### Heading 5\n" +
                "###### Heading 6\n\n";

            ItalicsInfo.Text =
                "Italics need the whole word or phrase to be encased in either '* *' or '_ _'\n\n";
            ItalicsSyntax.Text =
                "*This will be Itlaic*\n" +
                "_This will also be Italic_\n\n";

            BoldInfo.Text =
                "Bold words or phrases need to be encased in '** **' or '__ __'\n\n";
            BoldSyntax.Text =
                "**This will be Bold**\n" +
                "__This will also be Bold__\n\n";

            StrikeInfo.Text =
                "Striked text need to be encased in '~~ ~~'\n\n";
            StrikeSyntax.Text =
                "~~This will have a line~~\n\n";

            ListsInfo.Text =
                "Lists use '*' or '-' as a prefix to display a list view, and can be stacked\n\n";
            ListsSyntax.Text =
                "* Item 1\n" +
                "* Item 2\n" +
                "  * Item 2a\n" +
                "  * Item 2a\n" +
                "     * Item 2b\n" +
                "     * Item 2b\n\n";

            LinksInfo.Text =
                "Links can display different text to the actual link, Text is encased in '[ ]' and link is '( )'\n\n";
            LinksSyntax.Text =
                "[Click Here](https://github.com/empyreal96)\n\n";

            ImageLinksInfo.Text =
                "Images can be inserted from links, using the same concept as Links but prefixing '!'\n\n";
            ImageLinksSyntax.Text =
                "![Image Desription](https://somesite.com/image.jpg)\n\n";

            ImageResizingInfo.Text =
                "Resizing can be done by adding '| width=' to the image link\n\n";
            ImageResizingSyntax.Text =
                "![Image Desription](https://somesite.com/image.jpg | width=80)\n\n";

            TablesInfo.Text =
                "Tables take some setting up but can be achieved by 'creating' a table from characters as shown beside\n\n";
            TablesSyntax.Text =
                "|Header1|Header2|Header3|\n" +
                "| --- | --- | --- |\n" +
                "| Col1 | Col2 | Col3 |\n" +
                "| Row2 | Row2 | Row2 |\n" +
                "| This | Is a | Row |\n\n";

            HighlightInfo.Text =
                "Text and phrases can me highlighted to show emphasis, by encasing the words with ` `\n\n";
            HighlightSyntax.Text =
                "Here we have `emphasised` test to make it `stand out`\n\n";

            CodeInfo.Text =
                "Code snippets can be added to markdown to help display code samples, use ``` and write the code below\n\n";
            CodeSyntax.Text =
                "```\n" +
                "using System;\n" +
                "\n" +
                "namespace SampleCode\n" +
                "{\n" +
                "   public sealed partial class Sample : Page\n" +
                "   {\n" +
                "       public SampleCode\n" +
                "       {\n" +
                "           this.InitializeComponent();\n" +
                "       }\n" +
                "   }\n" +
                "}\n\n";

        }
    }
}
