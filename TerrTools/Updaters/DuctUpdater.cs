using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TerrTools.Updaters
{
    public class DuctsAreaUpdater : TerrUpdater
    {
        public DuctsAreaUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }

        public override string Name => "DuctsAreaUpdater";

        public override string Info => "Расчитывает площадь поверхности воздуховодов и фитингов";

        public override string Guid => "82646252-8930-41b7-9b38-83c545122fd6";

        public override ChangePriority Priority => ChangePriority.MEPAccessoriesFittingsSegmentsWires;

        public readonly string areaParamName = "ТеррНИИ_Площадь коммуникаций";

        private void main(Element el)
        {
            double fillArea = this.getSolidSurfaceArea(el);
            double connArea = this.getConnectorArea(el);
            el.LookupParameter(this.areaParamName).Set(fillArea - connArea);
        }

        private double getSolidSurfaceArea(Element el)
        {
            var opt = el.Document.Application.Create.NewGeometryOptions();
            GeometryElement elGeometries = el.get_Geometry(opt);
            double area = 0;
            foreach (GeometryObject geomObject in elGeometries)
            {
                if (geomObject is Solid)
                {
                    area += (geomObject as Solid).SurfaceArea;
                    continue;
                }
                if (geomObject is GeometryInstance)
                {
                    foreach (Solid symbolSolid in (geomObject as GeometryInstance).SymbolGeometry)
                    {
                        area += symbolSolid.SurfaceArea;
                    }
                }
            }                
            return area;
        }

        private double getConnectorArea(Element el)
        {
            double area = 0;
            ConnectorManager connManager = null;
            if (el is FamilyInstance)
            {
                connManager = (el as FamilyInstance).MEPModel.ConnectorManager;
            }
            else if (el is Duct)
            {
                connManager = (el as Duct).ConnectorManager;
            }
            if (connManager != null)
            {
                foreach (Connector con in connManager.Connectors)
                {
                    if (con.Shape == ConnectorProfileType.Round)
                    {
                        area += Math.Pow(con.Radius, 2) * Math.PI;
                    }
                    else if (con.Shape == ConnectorProfileType.Rectangular)
                    {
                        area += con.Height * con.Width;
                    }
                }
            }
            return area;
        }

        public override void GlobalExecute(Document doc)
        {
            foreach (Element el in new FilteredElementCollector(doc).WherePasses(TriggerPairs[0].Filter).ToElements())
            {
                try
                {
                    main(el);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Element id: " + el.Id.Value.ToString());
                }
            }
        }


        public override void InnerExecute(UpdaterData data)
        {
            foreach (ElementId id in data.GetModifiedElementIds().Concat(data.GetAddedElementIds()))
            {
                try
                {
                    Element el = Document.GetElement(id);
                    main(el);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Element id: " + id.Value.ToString());
                }
            }
        }
    }
}
