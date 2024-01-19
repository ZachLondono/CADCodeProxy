using CADCodeProxy;
using CADCodeProxy.CNC;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Exceptions;
using CADCodeProxy.Machining;
using System.Text.Json;
using Cocona;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
builder.Logging.AddDebug();

var app = builder.Build();

app.AddCommand((ILogger<Program> logger) => {

	logger.LogInformation("Basic test");

	var batch = CreateBatch();
	var generator = CreateGCodeGenerator();
	var machines = GetMachines();

	GenerateGCodeForBatch(batch, generator, machines);

});

app.AddCommand("csv-reading", (ILogger<Program> logger, string file = @"R:\Door Orders\CC Input\CSV Examples\Simple Door.csv") => {

	logger.LogInformation($"Reading CSV file '{file}'");

	var batches = ReadBatchesFromCSV(file);

	var generator = CreateGCodeGenerator();
	var machines = GetMachines();

	foreach (var batch in batches) {
		GenerateGCodeForBatch(batch, generator, machines);
	}

});

app.AddCommand("csv-writing", (ILogger<Program> logger, string outputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output") => {

	var batch = CreateBatch();
	var writer = new CSVTokenWriter();
	var file = writer.WriteBatchCSV(batch, outputDirectory);

	logger.LogInformation($"Wrote csv to file: {Path.GetFullPath(file)}");

});

app.AddCommand("json-reading", (ILogger<Program> logger) => {

	string file = @".\batches.json";
	var batches = JsonSerializer.Deserialize<Batch[]>(file);

	if (batches is null) {
		logger.LogError("No batches read from json file");
		return;
	}

	logger.LogInformation($"Read ({batches.Length}) batches from file: ");

	var generator = CreateGCodeGenerator();
	var machines = GetMachines();

	foreach (var batch in batches) {
		GenerateGCodeForBatch(batch, generator, machines);
	}

});

app.AddCommand("json-writing", (ILogger<Program> logger, string outputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output") => {

	var batch = CreateBatch();
	Batch[] batches = [batch];

	var batchJson = JsonSerializer.Serialize(batches, new JsonSerializerOptions() {
		WriteIndented = true,
	});

	string file = Path.Combine(outputDirectory, @"batches.json");
	File.WriteAllText(file, batchJson);

	logger.LogInformation($"Wrote json to file: {Path.GetFullPath(file)}");

});

app.Run();


static GCodeGenerator CreateGCodeGenerator() {
	var generator = new GCodeGenerator(LinearUnits.Millimeters);
	generator.GenerationEvent += Console.WriteLine;
	//generator.CADCodeProgressEvent += Console.WriteLine;
	generator.CADCodeErrorEvent += Console.WriteLine;
	return generator;
}

static List<Machine> GetMachines() {

	return [
		new() {
			Name = "Omnitech Selexx",
			NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
			SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
			ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Royal Omnitech Fanuc-Smart names.mdb",
			PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
			LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
		},
		new() {
			Name = "Anderson Stratos",
			NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
			SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
			ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Andi Stratos Royal - Tools from Omni.mdb",
			PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
			LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output",
		}
	];

}

void GenerateGCodeForBatch(Batch batch, GCodeGenerator generator, List<Machine> machines) {

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
	generator.Inventory.Add(new() {
		MaterialName = "MEDEX-3/4\"",
		AvailableQty = 2,
		IsGrained = true,
		PanelLength = 2464,
		PanelWidth = 1245,
		PanelThickness = 19.1,
		Priority = 1,
	});
	generator.Inventory.Add(new() {
		MaterialName = "White Mela MDF-3/4\"",
		AvailableQty = 2,
		IsGrained = true,
		PanelLength = 2464,
		PanelWidth = 1245,
		PanelThickness = 19.1,
		Priority = 1,
	});

	try {

		var result = generator.GeneratePrograms(machines, batch);

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

static Batch CreateBatch() {
	return new Batch() {
		Name = "Test Batch",
		InfoFields = new() {
				{ "Field", "Value" }
			},
		Parts = [
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
					Tokens = []
				},
				SecondaryFace = null
			}
		]
	};
}

Batch[] ReadBatchesFromCSV(string filePath) {

	var reader = new CSVTokenReader();
	var batches = reader.ReadBatchCSV(filePath);

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
