using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using FailureMessage = Autodesk.Revit.DB.FailureMessage;

namespace Bang.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Warning
    {
        private Warning()
        {
        }

        /// <summary>
        /// Provides the full description of the warning.
        /// </summary>
        /// <param name="warning">The warning to get the description of.</param>
        /// <returns name="description">The description.</returns>
        public static string Description(FailureMessage warning)
        { 
            return warning.GetDescriptionText();
        }
        /// <summary>
        /// Possibles severity messages include, Warning, Error and Document Corruption.
        /// </summary>
        /// <param name="warning">The warning to get the severity of.</param>
        /// <returns name="severity">How bad is it doc? </returns>
        public static string Severity(FailureMessage warning)
        {
            return warning.GetSeverity().ToString();
        }

        /// <summary>
        /// Retrieves the elements that are failing for that warning instance.
        /// </summary>
        /// <param name="warning">The warning to get the failing elements for.</param>
        /// <returns name="failingElements">All of the failing elements for that specific warning.</returns>
        public static List<global::Revit.Elements.Element> FailingElements(FailureMessage warning)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<global::Revit.Elements.Element> failingElements = warning.GetFailingElements().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
            return failingElements;
        }
        ///// <summary>
        ///// Retrieves the resolution caption if the warning has one.
        ///// </summary>
        ///// <param name="warning">The warning to get the failing elements for.</param>
        ///// <returns name="resolutionCaption">The default resolution caption for this warning.</returns>
        //public static string DefaultResolutionDescription(FailureMessage warning)
        //{
        //    Document doc = DocumentManager.Instance.CurrentDBDocument;
        //    return warning.GetDefaultResolutionCaption();
        //}
        /// <summary>
        /// Create Performance Adviser Rule by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static FailureMessage ById(string id)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
           
            return doc.GetWarnings().First(w => (w.GetFailureDefinitionId().Guid.ToString()+w.GetFailingElements().First()).Equals(id));
        }
        /// <summary>
        /// Create Performance Adviser Rule by Id
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<FailureMessage> ByDescription(string description)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            return doc.GetWarnings().Where(w => w.GetDescriptionText().Equals(description)).ToList();
        }
    }
}
