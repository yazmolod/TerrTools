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
        static public bool AddSharedParameter(
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

            try
            {
                doc.Application.SharedParametersFilename = sharedParameterFilePath;
                DefinitionFile spFile = doc.Application.OpenSharedParameterFile();
                DefinitionGroup terrGroup = spFile.Groups.get_Item(groupName);
                Definition sharedDef = terrGroup.Definitions.get_Item(parameterName);

                using (Transaction trDef = new Transaction(doc, "Добавление общего параметра"))
                {
                    trDef.Start();
                    Binding bind;
                    if (isIntance) bind = doc.Application.Create.NewInstanceBinding(catSet);                    
                    else bind = doc.Application.Create.NewTypeBinding(catSet);
                    bool result = doc.ParameterBindings.Insert(sharedDef, bind, group);
                    if (result)
                    {
                        trDef.Commit();
                        return true;
                    }
                    else
                    {
                        TaskDialog.Show("Ошибка", String.Format("Глобальный параметр \"{0}\" уже существует в проекте, но назначен другим категориям", parameterName));
                        return false;
                    }
                }                
            }
            catch
            {
                TaskDialog.Show("Ошибка", String.Format("Произошла ошибка добавления общего параметра \"{0}\"", parameterName));
                return false;
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

        static public RevitLinkInstance GetLinkedDoc(Document doc)
        {
            RevitLinkInstance[] linkedDocs = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToArray();
            var form = new UI.OneComboboxForm((from d in linkedDocs select d.Name).ToArray());
            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                RevitLinkInstance linkInstance = (from d in linkedDocs where d.Name == form.SelectedItem select d).First();
                return linkInstance;
            }
            else
            {
                return null;
            }
        }

        static public Transform GetCorrectionTransform(RevitLinkInstance linkedDocInstance)
        {
            Transform transform = linkedDocInstance.GetTransform();
            if (!transform.AlmostEqual(Transform.Identity)) return transform.Inverse;
            else return Transform.Identity;
        }

        static public Solid GetSolid(Element e)
        {
            Options opt = new Options();
            GeometryElement geomElem = e.get_Geometry(opt);
            foreach (GeometryObject geomObj in geomElem)
            {
                Solid geomSolid = geomObj as Solid;
                if (null != geomSolid) return geomSolid;
            }
            return null;
        }
    }
}
