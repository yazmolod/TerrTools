using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF = System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TerrTools.UI
{    
    public partial class ElementFilterForm : WF.Form
    {
        private List<Category> viewCategories = new List<Category>();
        private BindingList<BindedParameter> currentParameters = new BindingList<BindedParameter>();
        private BindingList<string> currentValues = new BindingList<string>();
        private Document doc;
        private UIDocument uidoc;
        private View activeView;
        public ElementFilterForm(ExternalCommandData commandData)
        {
            InitializeComponent();
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            activeView = commandData.View;


            Element[] allElements = new FilteredElementCollector(doc, activeView.Id).Where(x => x.Category != null).ToArray();
            foreach (Element e in allElements)
            {
                if (e.Category != null && !(from x in viewCategories select x.Id.IntegerValue).Contains(e.Category.Id.IntegerValue)) viewCategories.Add(e.Category);
            }
            viewCategories.OrderBy(x => x.Name);
            ((WF.ListBox)checkedListBox).DataSource = viewCategories;
            ((WF.ListBox)checkedListBox).DisplayMember = "Name";
            parameterComboBox.DataSource = currentParameters;
            parameterComboBox.DisplayMember = "ShowName";
            valueComboBox.DataSource = currentValues;
        }

        private List<Element> GetFilteredElements()
        {
            List<ElementFilter> filters = new List<ElementFilter>();
            foreach (Category c in checkedListBox.CheckedItems) filters.Add(new ElementCategoryFilter(c.Id));
            LogicalOrFilter filter = new LogicalOrFilter(filters);
            string parameter = parameterComboBox.Text;
            string value = valueComboBox.Text;
            string condition = comparisonComboBox.Text;

            List<Element> elements = new List<Element>();
            switch (condition)
            {
                case "равно":
                    elements = new FilteredElementCollector(doc, activeView.Id)
                        .WherePasses(filter)
                        .Where(x => x.LookupParameter(parameter).AsValueString() == value || x.LookupParameter(parameter).AsString() == value)
                        .ToList();
                    break;
                case "не равно":
                    elements = new FilteredElementCollector(doc, activeView.Id)
                        .WherePasses(filter)
                        .Where(x => x.LookupParameter(parameter).AsValueString() != value && x.LookupParameter(parameter).AsString() != value)
                        .ToList();
                    break;
                default:
                    break;
            }
            return elements;
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            List<Element> elements = GetFilteredElements();
            if (elements.Count > 0) uidoc.Selection.SetElementIds(elements.Select(x => x.Id).ToList());            
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            List<Element> elements = GetFilteredElements();
            if (elements.Count > 0) uidoc.ShowElements(elements.Select(x => x.Id).ToList());
        }

        private void checkedListBox_ItemCheck(object sender, WF.ItemCheckEventArgs e)
        {
            // Создаем временный список со всеми помеченными элементами
            List<Category> checkedElements = new List<Category>();
            foreach (Category cat in checkedListBox.CheckedItems) checkedElements.Add(cat);
            if (e.NewValue == WF.CheckState.Checked) checkedElements.Add(checkedListBox.Items[e.Index] as Category);
            else checkedElements.Remove(checkedListBox.Items[e.Index] as Category);
            // Создаем список параметров список параметров
            HashSet<string> firstSet = new HashSet<string>();
            List<BindedParameter> allParameters = new List<BindedParameter>();
            foreach (Category cat in checkedElements)
            {
                Element el = new FilteredElementCollector(doc, activeView.Id).OfCategoryId(cat.Id).FirstOrDefault();
                if (el != null)
                {
                    allParameters.AddRange((from x in el.GetOrderedParameters() select new BindedParameter(x)));
                    HashSet<string> secondSet = new HashSet<string>(from x in el.GetOrderedParameters() select x.Definition.Name);
                    if (firstSet.Count == 0) firstSet = secondSet;
                    else firstSet.IntersectWith(secondSet);                    
                }
            }
            // Обновить список, связанный с UI
            currentParameters.Clear();
            foreach (var p in firstSet) currentParameters.Add(allParameters.Where(x => x.Parameter.Definition.Name == p).First());
        }

        private List<string> GetParamValues(List<Element> el, string parameterName)
        {
            List<Parameter> param = new List<Parameter>();
            List<string> param_values = new List<string>();
            foreach (Element e in el)
            {
                Parameter p = e.GetOrderedParameters().Where(x => x.Definition.Name == parameterName).FirstOrDefault();
                if (p != null) param.Add(p);
            }
            foreach (Parameter p in param)
            {
                string value = p.AsValueString() ?? p.AsString();
                if (!param_values.Contains(value)) param_values.Add(value);
            }
            return param_values;
        }

        private void parameterComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            currentValues.Clear();
            List<ElementFilter> catfs = new List<ElementFilter>();
            foreach (Category c in checkedListBox.CheckedItems) { catfs.Add(new ElementCategoryFilter(c.Id)); }
            if (catfs.Count > 0)
            {
                LogicalOrFilter categoriesFilter = new LogicalOrFilter(catfs);
                List<Element> elements = new FilteredElementCollector(doc, activeView.Id).WherePasses(categoriesFilter).ToList();

                string currentParameter = parameterComboBox.Text;
                
                List<string> values = GetParamValues(elements, currentParameter);
                foreach (string v in values) currentValues.Add(v);
            }
        }
    }
    public class BindedElement
    {
        public BindedElement(Element el) { Element = el; }
        public Element Element { get; set; }
        public string ShowName { get { return Element.Category.Name; } }
    }
    public class BindedParameter
    {
        public BindedParameter(Parameter p) { Parameter = p; }
        public Parameter Parameter { get; set; }
        public string ShowName { get { return Parameter.Definition.Name; } }
    }    
}
