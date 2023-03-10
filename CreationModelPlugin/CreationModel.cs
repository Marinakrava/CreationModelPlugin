using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]

    public class CreationModel : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            //var res1 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(WallType))
            //    .OfType<WallType>() 
            //    .ToList();

            //var res2 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    .Where(x => x.Name.Equals(""))
            //    .OfType<FamilyInstance>()
            //    .ToList();

            //var res3 = new FilteredElementCollector(doc)
            //    .WhereElementIsNotElementType()
            //    .ToList();

           List<Level> listlevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            Level level1 = listlevel               
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();

            Level level2 = listlevel
               .Where(x => x.Name.Equals("Уровень 2"))
               .FirstOrDefault();

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));        

         CreateWalls (doc, level1, level2, points);

            List<Wall> walls = new List<Wall>();

            AddDoor(doc, level1,walls[0]);

            AddWindow(doc, level1, walls [1,2,3]);


            return Result.Succeeded;
        }


        public void CreateWalls(Document doc, Level level1, Level level2, List<XYZ> points)
        {                            
            List<Wall> walls = new List<Wall>();
            Transaction ts = new Transaction(doc, "Построение стен");

            ts.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }
            ts.Commit();
        }




        public void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_Doors)
            .OfType<FamilySymbol>()
            .Where(x => x.Name.Equals("0915 x 2134 мм"))
            .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
            .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!doorType.IsActive)
                 doorType.Activate();

            doc.Create.NewFamilyInstance(point, doorType, wall, level1, StructuralType.NonStructural);
        }

        public void AddWindow(Document doc, Level level1, List<Wall> walls)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_Windows)
            .OfType<FamilySymbol>()
            .Where(x => x.Name.Equals("0406 x 0610 мм"))
            .Where(x => x.FamilyName.Equals("Фиксированные"))
            .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point3 = hostCurve.Curve.GetEndPoint(2);
            XYZ point4 = hostCurve.Curve.GetEndPoint(3);
            XYZ point1_2 = (point1 + point2) / 2;
            XYZ point2_3 = (point2 + point3) / 2;
            XYZ point3_4 = (point3 + point4) / 2;


            if (!windowType.IsActive)
                 windowType.Activate();

            doc.Create.NewFamilyInstance(point1_2, windowType, wall, level1, StructuralType.NonStructural);
            doc.Create.NewFamilyInstance(point2_3, windowType, wall, level1, StructuralType.NonStructural);
            doc.Create.NewFamilyInstance(point3_4, windowType, wall, level1, StructuralType.NonStructural);
        }

    }

   
}
