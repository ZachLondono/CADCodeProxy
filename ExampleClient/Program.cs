using CADCodeProxy;
using CADCodeProxy.CNC;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Exceptions;
using CADCodeProxy.Machining;

Console.WriteLine("Generating GCode/CSV Tokens");

var machines = new List<Machine>() {
    new() {
        Name = "Anderson Stratos",
        TableOrientation = TableOrientation.Standard,
        NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Andi Stratos Royal - Tools from Omni.mdb",
        PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    },
    new() {
        Name = "Omnitech Selexx",
        TableOrientation = TableOrientation.Rotated,
        NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Royal Omnitech Fanuc-Smart names SMALL PARTS.mdb",
        PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    }
};

var batch = new Batch() {
    Name = "Test Batch",
    InfoFields = new() {
        { "Field", "Value" }
    },
    Parts = new Part[] {
        new() {
            Name = "ABC123",
            Qty = 5,
            Material = "3/4\" MDF",
            Width = 500,
            Length = 500,
            Thickness = 19.05,
            IsGrained = true,
            InfoFields = new() {
                { "Name", "Value" }
            },
            PrimaryFace = new() {
                ProgramName = "PartFront",
                Tokens = new IToken[] {
                    new OutlineSegment() {
                        Start = new(0, 0),
                        End = new(200, 0),
                        StartDepth = 19.05,
                        EndDepth = 19.05,
                        ToolName = "3-8Comp"
                    },
                    new OutlineSegment() {
                        Start = new(200, 0),
                        End = new(200, 500),
                        StartDepth = 19.05,
                        EndDepth = 19.05,
                        ToolName = "3-8Comp"
                    },
                    new OutlineSegment() {
                        Start = new(200, 500),
                        End = new(0, 500),
                        StartDepth = 19.05,
                        EndDepth = 19.05,
                        ToolName = "3-8Comp"
                    },
                    new OutlineSegment() {
                        Start = new(0, 500),
                        End = new(0, 0),
                        StartDepth = 19.05,
                        EndDepth = 19.05,
                        ToolName = "3-8Comp"
                    },
                    new Route() {
                        Start = new(0, 0),
                        End = new(100, 100),
                        StartDepth = 19.05,
                        EndDepth = 19.05,
                        ToolName = "3-8Comp"
                    }
                }
            },
            SecondaryFace = null
        },
        new Part() {
            Qty = 5,
            Width = 100,
            Length = 200,
            Thickness = 19.05,
            IsGrained = true,
            Name = "UBottom",
            Material = "3/4\" MDF",
            PrimaryFace = new() {
                ProgramName = $"UBottom-{1}",
                Tokens = new IToken[] {
                }
            }
        }
    }
};

var generator = new GCodeGenerator(LinearUnits.Millimeters);
GenerateGCodeForBatch(batch, generator, machines);
//WriteBatchToCSVFile(batch, @"R:\Door Orders\CC Input");

static void GenerateGCodeForBatch(Batch batch, GCodeGenerator generator, List<Machine> machines) {
    generator.Inventory.Add(new() {
        MaterialName = "1/2\" MDF",
        AvailableQty = 10,
        IsGrained = true,
        PanelLength = 2000,
        PanelWidth = 1000,
        PanelThickness = 12.7,
        Priority = 1,
    });
    generator.Inventory.Add(new() {
        MaterialName = "3/4\" MDF",
        AvailableQty = 10,
        IsGrained = true,
        PanelLength = 2000,
        PanelWidth = 1000,
        PanelThickness = 19.05,
        Priority = 1,
    });

    try {

        var result = generator.GeneratePrograms(machines, batch, @"C:\Users\Zachary Londono\Desktop\CC Output\reports");
        //var result = generator.GenerateProgramFromWSXMLFile(@"C:\Users\Zachary Londono\Desktop\WSXML\WSXML Drawing\Draw 05-31-2023-V11\testjob.xml", machines.First());

        Console.WriteLine($"Report: {result.WinStepReportFilePath}");
        foreach (var machineResult in result.MachineResults) {
            Console.WriteLine("====================");
            Console.WriteLine($"Machine: {machineResult.MachineName}");
            foreach (var matResult in machineResult.MaterialGCodeGenerationResults) {
                Console.WriteLine($"Material: {matResult.MaterialName} {matResult.MaterialThickness}");
                Console.WriteLine($"\tUnplaced parts ({matResult.UnplacedParts.Length})");
                Console.WriteLine($"\tUsed Inventory ({matResult.UsedInventory.Length}):");
                Console.WriteLine($"\tPlaced Parts ({matResult.PlacedParts.Length}):");
                foreach (var inv in matResult.UsedInventory) {
                    Console.WriteLine($"\t\tG: {inv.IsGrained}");
                }
            }
            Console.WriteLine("====================");
        }

    } catch (CADCodeAuthorizationException ex) {

        Console.WriteLine("Could not get authorization to use CADCode");
        Console.Write(ex);

    }

    Console.Read();

}

static void WriteBatchToCSVFile(Batch batch, string directory) {

    var writer = new CSVTokenWriter();

    var file = writer.WriteBatchCSV(batch, directory);

    Console.WriteLine($"File written to '{file}'");

}
