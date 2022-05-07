using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIFurnuture2
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private FamilySymbol selectedFurnitureType;
        private int amount;

        public List<FamilySymbol> FurnitureTypes { get; } = new List<FamilySymbol>();
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public XYZ Point1 { get; } = new XYZ();
        public XYZ Point2 { get; } = new XYZ();
        public FamilySymbol SelectedFurnitureType { get => selectedFurnitureType; set => selectedFurnitureType = value; }
        public Level SelectedLevel { get; set; }
        public int Amount { get => amount; set => amount = value; }
        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            FurnitureTypes = FurnitureUtils.GetFurnitureSymbols(commandData);
            Levels = LevelUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            Point1 = SelectionUtils.GetPoint(_commandData, "Выберите начальную точку", ObjectSnapTypes.Endpoints);
            Point2 = SelectionUtils.GetPoint(_commandData, "Выберите конечную точку", ObjectSnapTypes.Endpoints);

        }
        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Point1 == null || Point2 == null || SelectedFurnitureType == null || SelectedLevel == null || amount < 2)
                return;

            using (var ts = new Transaction(doc, "Разместить мебель"))
            {
                ts.Start();

                for (int i = 0; i < Amount; i++)

                {
                    double x, y, z;
                    x = Point1.X + i * ((Point2.X - Point1.X) / (Amount - 1));
                    y = Point1.Y + i * ((Point2.Y - Point1.Y) / (Amount - 1));
                    z = Point1.Z + i * ((Point2.Z - Point1.Z) / (Amount - 1));

                    XYZ point = new XYZ(x, y, z);

                    FamilyInstance instance = doc.Create.NewFamilyInstance(
                            point,
                            selectedFurnitureType,
                            SelectedLevel,
                            StructuralType.NonStructural);

                }

                ts.Commit();
            }
            RaiseCloseRequest();

        }
        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }

}
