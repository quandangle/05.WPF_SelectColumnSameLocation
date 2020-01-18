#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion

namespace QApps
{
    public class SelectColumnSamePlaceViewModel : ViewModelBase
    {
        public UIDocument UiDoc;
        public Document Doc;
        public List<Element> FirstElements = null;

        public SelectColumnSamePlaceViewModel(UIDocument uidoc)
        {
            // Lưu trữ data từ Revit
            UiDoc = uidoc;
            Doc = UiDoc.Document;

            // Lấy về Columns
            try
            {
                IEnumerable<Category> categories = new List<Category>
                {
                    Category.GetCategory(Doc, BuiltInCategory.OST_StructuralColumns),
                    Category.GetCategory(Doc, BuiltInCategory.OST_Columns),
                    Category.GetCategory(Doc, BuiltInCategory.OST_Walls),
                };

                FirstElements = KDXQqeontLCGWXe.PickObjects(UiDoc, new NelWeZOVFbiLttv(categories),
                    "Select the first Column/Wall", true);
            }
            catch (Exception)
            {
                return;
            }

            // khởi tạo data cho WPF

            AllLevel = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>().ToList();
            AllLevel = AllLevel.OrderBy(l => l.Elevation)
                .ToList();

            try
            {
                FromLevel = AllLevel[0];
                ToLevel = AllLevel[1];
            }
            catch (Exception)
            {
            }

        }

        #region Khai báo Binding Properties 
        public List<Level> AllLevel { get; set; }
        public Level FromLevel { get; set; }
        public Level ToLevel { get; set; }

        #endregion Khai báo Binding Properties 


        internal void SelectColumnSamePlace()
        {
            #region Lấy về tất cả Column/Wall đụng với FirstElement trên toàn bộ document

            List<ElementId> allElements = new List<ElementId>();
            allElements.AddRange(FirstElements.Select(e=>e.Id));

            foreach (var e in FirstElements)
            {
                List<ElementId> columnWallBoundingBoxIntersects
                    = GetColumnWallBoundingBoxIntersects(e);
                List<ElementId> elementIdsToCheckContinue 
                    = columnWallBoundingBoxIntersects.Except(allElements)
                    .ToList();

                allElements.AddRange(elementIdsToCheckContinue);

                while (elementIdsToCheckContinue.Any())
                {
                    List<ElementId> elementIdsToCheckContinueCopy 
                        = new List<ElementId>(elementIdsToCheckContinue);

                    foreach (var e2 in elementIdsToCheckContinueCopy)
                    {
                        columnWallBoundingBoxIntersects
                            = GetColumnWallBoundingBoxIntersects(Doc.GetElement(e2));
                        List<ElementId> elementIdsToCheckContinue2 
                            = columnWallBoundingBoxIntersects.Except(allElements)
                            .ToList();

                        elementIdsToCheckContinue.AddRange(elementIdsToCheckContinue2);
                        elementIdsToCheckContinue=
                            elementIdsToCheckContinue.Except(new List<ElementId>() {e2})
                                .ToList();
                        allElements.AddRange(elementIdsToCheckContinue2);
                    }
                }
            }

            #endregion

            #region Lọc theo tầng

            double minElevation = FromLevel.Elevation;
            double maxElevation = ToLevel.Elevation;

            List<ElementId> allElementIds=new List<ElementId>();
            foreach (var id in allElements)
            {
                Element e = Doc.GetElement(id);
                if (e.Category.Name.Contains("Columns"))
                {
                    ElementId levelId = e.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM)
                        ?.AsElementId();
                    if (levelId==null) continue;
                    Level level = Doc.GetElement(levelId) as Level;
                    if (level == null) continue;
                    double elevation = level.Elevation;
                    if (elevation>=minElevation && elevation <= maxElevation)
                    {
                        allElementIds.Add(e.Id);
                    }
                }
                else // Wall
                {
                    ElementId levelId = e.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT)
                        ?.AsElementId();
                    if (levelId == null) continue;
                    Level level = Doc.GetElement(levelId) as Level;
                    if (level == null) continue;
                    double elevation = level.Elevation;
                    if (elevation >= minElevation && elevation <= maxElevation)
                    {
                        allElementIds.Add(e.Id);
                    }
                }
            }

            #endregion

           UiDoc.Selection.SetElementIds(allElementIds);
        }

        /// <summary>
        /// Lấy về tất cả Column/Wall đụng e theo phương đứng
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal List<ElementId> GetColumnWallBoundingBoxIntersects(Element e)
        {
            ICollection<BuiltInCategory> categories
                = new List<BuiltInCategory>();
            categories.Add(BuiltInCategory.OST_StructuralColumns);
            categories.Add(BuiltInCategory.OST_Columns);
            categories.Add(BuiltInCategory.OST_Walls);

            ElementMulticategoryFilter categoryFilter
                = new ElementMulticategoryFilter(categories);

            BoundingBoxXYZ box = e.get_BoundingBox(null);
            XYZ min = new XYZ(
                box.Min.X + DLQUnitUtils.MmToFeet(10),
                box.Min.Y + DLQUnitUtils.MmToFeet(10),
                box.Min.Z - DLQUnitUtils.MmToFeet(10));

            XYZ max = new XYZ(
                box.Max.X - DLQUnitUtils.MmToFeet(10),
                box.Max.Y - DLQUnitUtils.MmToFeet(10),
                box.Max.Z + DLQUnitUtils.MmToFeet(10));

            Outline outline = new Outline(min, max);
            BoundingBoxIntersectsFilter bbFilter
                = new BoundingBoxIntersectsFilter(outline);

            LogicalAndFilter logicalAndFilter
                = new LogicalAndFilter(bbFilter, categoryFilter);

            return new FilteredElementCollector(e.Document)
                    .WherePasses(logicalAndFilter)
                    .Excluding(new List<ElementId>() { e.Id })
                    .Select(el=>el.Id)
                    .ToList();
        }

    }
}
