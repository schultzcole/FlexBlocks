using System;
using System.Collections.Generic;
using FlexBlocks;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocks.Renderables.Debug;
using Playground;

var block = new BorderBlock
{
    Overlay = new CompositeRenderable
    {
        new FrametimeOverlay { ShowFps = true },
        new TitleOverlay { Title = " FlexBlocks Demo ", Offset = 2 }
    },
    Border = Borders.Rounded,
    Padding = new Padding(1, 3),
    HorizontalSizing = Sizing.Fill,
    VerticalSizing = Sizing.Fill,
    Content = new FlexBlock
    {
        HorizontalSizing = Sizing.Content,
        VerticalSizing = Sizing.Content,
        Contents = new List<UiBlock>
        {
            new MyAnimatedWidthBlock
            {
                Size = UnboundedBlockSize.From(BlockLength.From(30), BlockLength.Unbounded),
                MinWidth = 20,
                MaxWidth = 60,
                Content = new BorderBlock
                {
                    Overlay = new TitleOverlay { Title = "Sidebar", Offset = 2 },
                    Border = Borders.Square,
                    Padding = new Padding(1, 3),
                    Content = new TextBlock
                    {
                        Text = "Alpha\nBravo\nCharlie\nDelta\nEcho\nFoxtrot\nGolf\nHotel\nIndia\nJuliet\nKilo\nLima\nMike\nNovember\nOscar\nPapa\nQuebec\nRomeo\nSierra\nTango\nUniform\nVictor\nWhiskey\nX-Ray\nYankee\nZulu"
                    }
                }
            },
            new FixedSizeBlock
            {
                Size = UnboundedBlockSize.Unbounded,
                Content = new BorderBlock
                {
                    Overlay = new TitleOverlay { Title = "Content", Offset = ^2 },
                    Border = Borders.Square,
                    Padding = new Padding(1, 7),
                    Content = new TextBlock
                    {
                        Text = """
                               Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Enim nec dui nunc mattis enim ut. Sit amet venenatis urna cursus eget nunc scelerisque. Lorem mollis aliquam ut porttitor. Risus nullam eget felis eget. Dolor magna eget est lorem ipsum. Suspendisse sed nisi lacus sed viverra tellus in hac habitasse. Et netus et malesuada fames ac turpis egestas sed. Viverra vitae congue eu consequat ac felis donec et odio. Quisque egestas diam in arcu. Nulla facilisi nullam vehicula ipsum. Lacus luctus accumsan tortor posuere ac. Purus semper eget duis at tellus at urna. Venenatis urna cursus eget nunc scelerisque.

                               Donec ultrices tincidunt arcu non. Consequat mauris nunc congue nisi vitae suscipit tellus mauris a. Ultricies leo integer malesuada nunc vel risus commodo. In est ante in nibh mauris cursus mattis. Semper feugiat nibh sed pulvinar proin. Ultrices tincidunt arcu non sodales neque sodales ut etiam sit. Mattis pellentesque id nibh tortor id aliquet. Eu feugiat pretium nibh ipsum consequat nisl vel pretium lectus. Hac habitasse platea dictumst vestibulum rhoncus est pellentesque elit ullamcorper. Est placerat in egestas erat imperdiet. In eu mi bibendum neque egestas congue quisque egestas. Rhoncus est pellentesque elit ullamcorper dignissim cras tincidunt lobortis feugiat. Mi sit amet mauris commodo quis imperdiet massa tincidunt. Tincidunt augue interdum velit euismod in pellentesque massa placerat duis. Mauris commodo quis imperdiet massa tincidunt nunc. In hac habitasse platea dictumst quisque sagittis purus sit amet. Vestibulum rhoncus est pellentesque elit ullamcorper dignissim. Sit amet consectetur adipiscing elit ut. Erat velit scelerisque in dictum non consectetur a erat nam.

                               Ac turpis egestas maecenas pharetra convallis posuere. Tellus integer feugiat scelerisque varius morbi enim nunc faucibus a. Odio tempor orci dapibus ultrices in iaculis nunc sed. Arcu dictum varius duis at consectetur lorem. Tristique nulla aliquet enim tortor at auctor urna nunc. Cursus eget nunc scelerisque viverra. Interdum posuere lorem ipsum dolor. Platea dictumst quisque sagittis purus sit amet. Posuere lorem ipsum dolor sit amet consectetur adipiscing. Integer enim neque volutpat ac tincidunt vitae semper quis lectus. Facilisis volutpat est velit egestas dui id ornare arcu. Platea dictumst quisque sagittis purus sit. Nisl vel pretium lectus quam id leo in.

                               Sit amet tellus cras adipiscing. Vel orci porta non pulvinar neque laoreet suspendisse interdum consectetur. Urna neque viverra justo nec ultrices. Etiam erat velit scelerisque in dictum non consectetur a erat. Amet est placerat in egestas erat imperdiet sed euismod nisi. Cras tincidunt lobortis feugiat vivamus at. Aenean pharetra magna ac placerat vestibulum. Bibendum arcu vitae elementum curabitur vitae nunc sed. Amet dictum sit amet justo. Quisque egestas diam in arcu cursus euismod quis viverra. Eu facilisis sed odio morbi quis commodo odio aenean sed. Ut morbi tincidunt augue interdum velit euismod in. Elementum nisi quis eleifend quam adipiscing. Pellentesque habitant morbi tristique senectus et netus et malesuada. Rutrum tellus pellentesque eu tincidunt tortor aliquam.

                               Nisl pretium fusce id velit ut. Vitae congue eu consequat ac felis. Augue neque gravida in fermentum et sollicitudin. Nibh praesent tristique magna sit amet. Ultrices vitae auctor eu augue ut. Id neque aliquam vestibulum morbi. Magna sit amet purus gravida quis blandit turpis. Mi eget mauris pharetra et ultrices neque. Duis ultricies lacus sed turpis tincidunt id. Hendrerit gravida rutrum quisque non tellus orci. Blandit cursus risus at ultrices mi tempus. In hac habitasse platea dictumst. Aenean euismod elementum nisi quis eleifend quam adipiscing vitae.
                               """
                    }
                }
            }
        }
    },
};

Console.Title = "FlexBlocks Playground";

Console.WriteLine("Hello, World!");
await FlexBlocksDriver.Run(block);
Console.WriteLine("Goodbye!");
