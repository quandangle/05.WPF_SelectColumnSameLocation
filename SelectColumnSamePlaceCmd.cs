#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using Application = Autodesk.Revit.ApplicationServices.Application;
#endregion

namespace QApps
{
    [Transaction(TransactionMode.Manual)]
    public class SelectColumnSamePlaceCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, 
            ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
          

             // code    
             SelectColumnSamePlaceViewModel viewModel
                       = new SelectColumnSamePlaceViewModel(uidoc);
                if (viewModel.FirstElements == null) return Result.Cancelled;

                SelectColumnSamePlaceWindow window =
                    new SelectColumnSamePlaceWindow(viewModel);
                window.ShowDialog();


            return Result.Cancelled;
        }
    }
}


