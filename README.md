# Sharper.GstarCAD.Extensions
### GstarCAD Extension Library
The GstarCAD port of **[Gile.AutoCAD.Extension](https://github.com/gileCAD/Gile.AutoCAD.Extension)**.
This is an unofficial extension library enhanced for GstarCAD .NET SDK.

> You can get GstarCAD SDK on the official website https://www.gstarcad.com/download/, or on the [Nuget Gallery](https://www.nuget.org/packages/GstarCADNET).

#### This library should help to write code in a more concise and declarative way.
Example with a method to erase lines in model space which are smaller than a given distance:
```csharp
public void EraseShortLines(double minLength)
{
    var db = Application.DocumentManager.MdiActiveDocument.Database;
    using (var tr = db.TransactionManager.StartTransaction())
    {
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        var lineClass = RXObject.GetClass(typeof(Line));
        foreach (ObjectId id in modelSpace)
        {
            if (id.ObjectClass == lineClass)
            {
                var line = (Line)tr.GetObject(id, OpenMode.ForRead);
                if (line.Length < minLength)
                {
                    tr.GetObject(id, OpenMode.ForWrite);
                    line.Erase();
                }
            }
        }
        tr.Commit();
    }
}
```
The same method can be written:
```csharp
public void EraseShortLines(double minLength)
{
    var db = Active.Database;
    using (var tr = db.TransactionManager.StartTransaction())
    {
        db.GetModelSpace()
            .GetObjects<Line>()
            .Where(line => line.Length < minLength)
            .UpgradeWrite()
            .ForEach(line => line.Erase());
        tr.Commit();
    }
}
```
