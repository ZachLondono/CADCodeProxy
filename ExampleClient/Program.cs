using CADCodeProxy;
using CADCodeProxy.CNC;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Exceptions;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;
using System.Text;
using System.Text.Json;

/*
var gcodeResult = new GCodeGenerationResult() {
	WinStepReportFilePath = null,
	MachineResults = new MachineGCodeGenerationResult[] {
		new() {
			MachineName = "Machine A",
			MaterialGCodeGenerationResults = new MaterialGCodeGenerationResult[] {
				new() {
					MaterialName = "Material 1",
					MaterialThickness = 19.05,
					ProgramNames = new string[] {
						"0", "1", "2"
					},
					PartLabels = new PartLabel[] {
						new() {
							PartId = Guid.Empty,
							Fields = new()
						}
					},
					PlacedParts = new PlacedPart[] {
						new() {
							PartId = Guid.Empty,
							Name = "Part",
							Width = 0,
							Length = 0,
							UsedInventoryIndex = 0,
							ProgramIndex = 0,
							Area = 0,
							IsRotated = false,
							InsertionPoint = new(0,0),
						}
					},
					UnplacedParts = new UnplacedPart[] {
						new() {
							PartName = "Part",
							Qty = 0
						}
					},
					UsedInventory = new UsedInventory[] {
						new() {
							Name = "Material",
							Width = 0,
							Length = 0,
							Thickness = 0,
							Qty = 0,
							IsGrained = false,
						}
					}
				}
			}
		}
	}
};

var gcodeSerialize = JsonSerializer.Serialize(gcodeResult, new JsonSerializerOptions() {
	WriteIndented = true
});
File.WriteAllText(@"C:\Users\Zachary Londono\Desktop\TestOutput\GCode Result.json", gcodeSerialize);
*/

var batches = ReadBatchesFromCSV();

var machines = new List<Machine>() {
    //new() {
    //    Name = "Omnitech Selexx",
    //    NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    //    SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    //    ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Royal Omnitech Fanuc-Smart names.mdb",
    //    PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    //    LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    //},
	new() {
        Name = "Anderson Stratos",
        NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Andi Stratos Royal - Tools from Omni.mdb",
        PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
        LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
    }
};

//Batch batch = CreateBatch();
var batch = batches.First();

var generator = new GCodeGenerator(LinearUnits.Millimeters);
GenerateGCodeForBatch(batch, generator, machines);
//WriteBatchToCSVFile(batch, @"R:\Door Orders\CC Input");

static void GenerateGCodeForBatch(Batch batch, GCodeGenerator generator, List<Machine> machines) {

	generator.Inventory.Add(new() {
		MaterialName = "1/2\" MDF",
		AvailableQty = 2,
		IsGrained = true,
		PanelLength = 2464,
		PanelWidth = 1245,
		PanelThickness = 12.7,
		Priority = 1,
	});
	generator.Inventory.Add(new() {
		MaterialName = "3/4\" MDF",
		AvailableQty = 2,
		IsGrained = true,
		PanelLength = 2464,
		PanelWidth = 1245,
		PanelThickness = 19.05,
		Priority = 1,
	});
	generator.Inventory.Add(new() {
		MaterialName = "MDF-3/4\"",
		AvailableQty = 2,
		IsGrained = true,
		PanelLength = 2464,
		PanelWidth = 1245,
		PanelThickness = 19.1,
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

static Batch CreateBatch() {
	return new Batch() {
		Name = "Test Batch",
		InfoFields = new() {
			{ "Field", "Value" }
		},
		Parts = new Part[] {
		new() {
			Qty = 1,
			Material = "3/4\" MDF",
			Width = 250,
			Length = 500,
			Thickness = 19.05,
			IsGrained = true,
			InfoFields = new() {
				{ "Name", "Value" },
				{ "CustomerInfo1", "Customer Name" },
				{ "Level1", "Room Name" },
				{ "Comment1", "Comment 1" },
				{ "Comment2", "Comment 2" },
				{ "Side1Color", "Color" },
				{ "Side1Material", "Material" },
				{ "CabinetNumber", "1234" },
				{ "ProductName", "ProdABC" },
				{ "Description", "Product ABC" },
			},
			Width1Banding = new("Purple", "PVC"),
			Width2Banding = new("Pink", "Paper"),
			Length1Banding = new("Red", "Bubble Gum"),
			Length2Banding = new("Blue", "Rocks"),
			PrimaryFace = new() {
				ProgramName = "PartFront1",
				Tokens = new IToken[] {
					new OutlineSegment() {
						Start = new(20, 20),
						End = new(460, 20),
						StartDepth = 19.05,
						EndDepth = 19.05,
						ToolName = "",
						NumberOfPasses = 1,
						SequenceNumber = 99
					},
					new Fillet() {
						Radius = 20,
					},
					new OutlineSegment() {
						Start = new(500-40, 0+20),
						End = new(500-40, 250-40),
						StartDepth = 19.05,
						EndDepth = 19.05,
						ToolName = "",
						NumberOfPasses = 1,
						SequenceNumber = 99
					},
					new OutlineSegment() {
						Start = new(500-40, 250 - 40),
						End = new(0+20, 250-40),
						StartDepth = 19.05,
						EndDepth = 19.05,
						ToolName = "",
						NumberOfPasses = 1,
						SequenceNumber = 99
					},
					new OutlineSegment() {
						Start = new(0+20, 250-40),
						End = new(0+20, 0+20),
						StartDepth = 19.05,
						EndDepth = 19.05,
						ToolName = "",
						NumberOfPasses = 1,
						SequenceNumber = 99
					}
				}
			},
			SecondaryFace = null
		},
		new() {
			Qty = 1,
			Material = "3/4\" MDF",
			Width = 250,
			Length = 500,
			Thickness = 19.05,
			IsGrained = true,
			InfoFields = new() {
				{ "Name", "Value" },
				{ "CustomerInfo1", "Customer Name" },
				{ "Level1", "Room Name" },
				{ "Comment1", "Comment 1" },
				{ "Comment2", "Comment 2" },
				{ "Side1Color", "Color" },
				{ "Side1Material", "Material" },
				{ "CabinetNumber", "1234" },
				{ "ProductName", "ProdABC" },
				{ "Description", "Product ABC" },
			},
			Width1Banding = new("Purple", "PVC"),
			Width2Banding = new("Pink", "Paper"),
			Length1Banding = new("Red", "Bubble Gum"),
			Length2Banding = new("Blue", "Rocks"),
			PrimaryFace = new() {
				ProgramName = "PartFront2",
				Tokens = new IToken[] {
				}
			},
			SecondaryFace = null
		}
	}
	};
}

static Batch[] ReadBatchesFromCSV() {

	var reader = new CSVTokenReader();
	var batches = reader.ReadBatchCSV(@"R:\Door Orders\CC Input\Number - DoorTokens 2.csv");//@"C:\Users\Zachary Londono\Desktop\TestOutput\Number - DoorTokens.csv");

	var batchJson = JsonSerializer.Serialize(batches, new JsonSerializerOptions() {
		WriteIndented = true,
	});
	File.WriteAllText(@"C:\Users\Zachary Londono\Desktop\TestOutput\Number - DoorTokens.json", batchJson);

	Console.WriteLine($"Read '{batches.Count()}' batches");
	foreach (var batch in batches) {
		Console.WriteLine(batch.Name);
		Console.WriteLine($"Info ({batch.InfoFields.Count()}):");
		foreach (var field in batch.InfoFields) {
			Console.WriteLine($"\t{field.Key} => {field.Value}");
		}
		Console.WriteLine($"Parts ({batch.Parts.Count()}):");
		foreach (var part in batch.Parts) {
			Console.WriteLine($"\t{part.Qty}");
			Console.WriteLine($"\t{part.Width}");
			Console.WriteLine($"\t{part.Length}");
			Console.WriteLine($"\t{part.Material}");
			foreach (var token in part.PrimaryFace.Tokens) {
				Console.WriteLine($"\t\t{token.GetType().Name} - {(token as IMachiningOperation)?.ToolName}");
			}
		}
	}

	return batches;

}