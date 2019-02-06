using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using Revit.Elements;
using Element = Revit.Elements.Element;


namespace Bang.Revit.Elements
{
    /// <summary>
    /// Wrapper for warnings.
    /// </summary>
    public class WarningTools
    {
        private WarningTools()
        { }
        /// <summary>
        /// This node will get the warnings for the current document. Revit 2018 and up only!
        /// </summary>
        /// <param name="toggle">Toggle to reset the collection.</param>
        /// <returns name="Warning Text">The description of the warning.</returns>
        /// <returns name="Failing Elements">The elements that are failing.</returns>
        /// <search>
        /// warnings
        /// </search>
        [MultiReturn(new[] { "Warning Text","Failing Elements" })]
        public static Dictionary<string, object> GetWarnings(bool toggle)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            IList<Autodesk.Revit.DB.FailureMessage> warningList = doc.GetWarnings();
            warningList[0].GetFailureDefinitionId();
            //lists for output
            List<string> messagesList = new List<string>();
            List<List<global::Revit.Elements.Element>> failingElementsList = new List<List<global::Revit.Elements.Element>>();

            foreach (var message in warningList)
            {
                messagesList.Add(message.GetDescriptionText());
                var failingElementIds = new List<ElementId>(message.GetFailingElements());
                List<Element> failingElements = new List<Element>();
                foreach (var id in failingElementIds)
                {
                    failingElements.Add(doc.GetElement(id).ToDSType(true));
                }
                failingElementsList.Add(failingElements);
            }

            //returns the outputs
            var outInfo = new Dictionary<string, object>
                {
                    { "Warning Text", messagesList},
                    { "Failing Elements", failingElementsList}
                };
            return outInfo;
        }



    }
}
