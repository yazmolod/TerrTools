using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace TerrTools
{
    class Static
    {
        static public string sharedParameterFilePath = @"\\serverL\PSD\REVIT\ФОП\ФОП2017.txt";
        public static bool AddSharedParameter(
            Document doc,
            string parameterName,
            string groupName,
            bool isIntance,
            BuiltInCategory[] categories,
            BuiltInParameterGroup group = BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES
            )
        {
            CategorySet catSet = doc.Application.Create.NewCategorySet();
            foreach (BuiltInCategory c in categories)
            {
                catSet.Insert(doc.Settings.Categories.get_Item(c));
            }

            doc.Application.SharedParametersFilename = sharedParameterFilePath;
            DefinitionFile spFile = doc.Application.OpenSharedParameterFile();
            DefinitionGroup terrGroup = spFile.Groups.get_Item(groupName);
            Definition sharedDef = terrGroup.Definitions.get_Item(parameterName);
            if (sharedDef == null)
            {
                TaskDialog.Show("Ошибка добавления общих параметров",
                    "Параметр " + parameterName + " отсутствует в файле общих параметров. Проверьте его наличие и повторите попытку");
                return false;
            }
            else
            {
                using (Transaction trDef = new Transaction(doc, "Добавление общего параметра"))
                {
                    trDef.Start();
                    if (isIntance)
                    {
                        InstanceBinding bind = doc.Application.Create.NewInstanceBinding(catSet);
                        doc.ParameterBindings.Insert(sharedDef, bind, group);
                    }
                    else
                    {
                        TypeBinding bind = doc.Application.Create.NewTypeBinding(catSet);
                        doc.ParameterBindings.Insert(sharedDef, bind, group);
                    }
                    trDef.Commit();
                }
                return true;
            }
        }
        
        static public List<List<Curve>> GetCurvesListFromRoom(Room room)
        {
            List<List<Curve>> profiles = new List<List<Curve>>();
            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(opt);
            for (int i = 0; i < boundaries.Count; i++)
            {
                profiles.Add(new List<Curve>());
                foreach (BoundarySegment s in boundaries[i])
                {
                    profiles[i].Add(s.GetCurve());
                }
            }
            return profiles;
        }
    }
}
