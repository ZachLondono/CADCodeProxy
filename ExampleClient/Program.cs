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

var serializationOptions = new JsonSerializerOptions() {
	WriteIndented = true
};

var app = builder.Build();

app.AddCommand((ILogger<Program> logger) => {

	logger.LogInformation("Basic test");

	var batch = CreateBatch();
	var generator = CreateGCodeGenerator();
	var machines = GetMachines();

	GenerateGCodeForBatch(batch, generator, machines);

});

app.AddCommand("csv-reading", (ILogger<Program> logger, string file = @"R:\Door Orders\CC Input\OT3207 - DoorTokens.csv") => {

	logger.LogInformation("Reading CSV file '{File}'", file);

	var batches = ReadBatchesFromCSV(file);

	var generator = CreateGCodeGenerator();
	var machines = GetMachines();

	foreach (var batch in batches) {
		GenerateGCodeForBatch(batch, generator, machines);
	}

});

app.AddCommand("csv-writing", (ILogger<Program> logger, string outputDirectory = @"R:\Door Orders\CC Input\CSV Examples") => {

	var batch = CreateBatch();
	var writer = new CSVTokenWriter();
	var file = writer.WriteBatchCSV(batch, outputDirectory);

	logger.LogInformation("Wrote csv to file: {FilePath}", Path.GetFullPath(file));

});

app.AddCommand("json-reading", (ILogger<Program> logger) => {

	string file = @".\batches.json";
	var batches = JsonSerializer.Deserialize<Batch[]>(file);

	if (batches is null) {
		logger.LogError("No batches read from json file");
		return;
	}

	logger.LogInformation("Read ({BatchCount}) batches from file: ", batches.Length);

	var generator = CreateGCodeGenerator();
	var machines = GetMachines();

	foreach (var batch in batches) {
		GenerateGCodeForBatch(batch, generator, machines);
	}

});

app.AddCommand("json-writing", (ILogger<Program> logger, string outputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output") => {

	var batch = CreateBatch();
	Batch[] batches = [batch];

	var batchJson = JsonSerializer.Serialize(batches, serializationOptions);

	string file = Path.Combine(outputDirectory, @"batches.json");
	File.WriteAllText(file, batchJson);

	logger.LogInformation("Wrote json to file: {(FilePath)}", Path.GetFullPath(file));

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
			NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Omni\nest",
			SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Omni\single",
			ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Royal Omnitech Fanuc-Smart names.mdb",
			SinglePartToolFilePath = @"Y:\CADCode\cfg\Tool Files\Royal Omnitech Fanuc-Smart names X100 SHIFT.mdb",
			PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Omni\pix",
			LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Omni\labels",
		},
		new() {
			Name = "Anderson Stratos",
			NestOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Andi\nest",
			SingleProgramOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Andi\single",
			ToolFilePath = @"Y:\CADCode\cfg\Tool Files\Andi Stratos Royal - Tools from Omni.mdb",
			SinglePartToolFilePath = @"Y:\CADCode\cfg\Tool Files\Andi Stratos Royal - Tools from Omni.mdb",
			PictureOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Andi\pix",
			LabelDatabaseOutputDirectory = @"C:\Users\Zachary Londono\Desktop\CC Output\Andi\labels",
		}
	];

}

void GenerateGCodeForBatch(Batch batch, GCodeGenerator generator, List<Machine> machines) {

	var materials = batch.Parts
						 .GroupBy(p => (p.Material, p.Thickness))
						 .Select(g => g.Key);

	foreach (var material in materials) {
		generator.Inventory.Add(new() {
			MaterialName = material.Material,
			AvailableQty = 99999,
			IsGrained = true,
			PanelLength = 2464,
			PanelWidth = 1245,
			PanelThickness = material.Thickness,
			Priority = 1,
		});
	}

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
					Tokens = [
							new Route() {
								Start = new((460-20)/2, 20),
								End = new(460, 20),
								StartDepth = 19.05,
								EndDepth = 19.05,
								ToolName = "3-8Comp",
								NumberOfPasses = 1,
							},
							new Fillet() {
								Radius = 20,
							},
							new Route() {
								Start = new(500-40, 0+20),
								End = new(500-40, 250-40),
								StartDepth = 19.05,
								EndDepth = 19.05,
								ToolName = "3-8Comp",
								NumberOfPasses = 1,
							},
							new Fillet() {
								Radius = 20,
							},
							new Route() {
								Start = new(500-40, 250 - 40),
								End = new(0+20, 250-40),
								StartDepth = 19.05,
								EndDepth = 19.05,
								ToolName = "3-8Comp",
								NumberOfPasses = 1,
							},
							new Fillet() {
								Radius = 20,
							},
							new Route() {
								Start = new(0+20, 250-40),
								End = new(0+20, 0+20),
								StartDepth = 19.05,
								EndDepth = 19.05,
								ToolName = "3-8Comp",
								NumberOfPasses = 1,
							},
							new Fillet() {
								Radius = 20,
							},
							new Route() {
								Start = new(20, 20),
								End = new((460-20)/2, 20),
								StartDepth = 19.05,
								EndDepth = 19.05,
								ToolName = "3-8Comp",
								NumberOfPasses = 1,
							},
					]
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
					Tokens = [
							new Rectangle() {
								CornerA = new(20, 20),
								CornerC = new(460, 210),
								CornerB = new(460, 20),
								CornerD = new(20, 210),
								StartDepth = 19.05,
								EndDepth = 19.05,
								Radius = 20,
								ToolName = "3-8Comp",
								NumberOfPasses = 1,
							}
					]
				},
				SecondaryFace = null
			},
		]
	};
}

Batch[] ReadBatchesFromCSV(string filePath) {

	var reader = new CSVTokenReader();
	var batches = reader.ReadBatchCSV(filePath);

	Console.WriteLine($"Read '{batches.Length}' batches");
	foreach (var batch in batches) {
		Console.WriteLine(batch.Name);
		Console.WriteLine($"Info ({batch.InfoFields.Count()}):");
		foreach (var field in batch.InfoFields) {
			Console.WriteLine($"\t{field.Key} => {field.Value}");
		}
		Console.WriteLine($"Parts ({batch.Parts.Length}):");
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
