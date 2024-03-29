﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using CurveElement = Autodesk.Revit.DB.CurveElement;
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
        [Obsolete("Revit 2022 is the last version of Revit that Bang will support. Please migrate your projects to use the new OOTB nodes.")]

        public static string Description(FailureMessage warning)
        { 
            return warning.GetDescriptionText();
        }
        /// <summary>
        /// Possibles severity messages include, Warning, Error and Document Corruption.
        /// </summary>
        /// <param name="warning">The warning to get the severity of.</param>
        /// <returns name="severity">How bad is it doc? </returns>
        [Obsolete("Revit 2022 is the last version of Revit that Bang will support. Please migrate your projects to use the new OOTB nodes.")]

        public static string Severity(FailureMessage warning)
        {
            return warning.GetSeverity().ToString();
        }

        /// <summary>
        /// Retrieves the elements that are failing for that warning instance.
        /// </summary>
        /// <param name="warning">The warning to get the failing elements for.</param>
        /// <returns name="failingElements">All of the failing elements for that specific warning.</returns>
        [Obsolete("Revit 2022 is the last version of Revit that Bang will support. Please migrate your projects to use the new OOTB nodes.")]

        public static List<global::Revit.Elements.Element> FailingElements(FailureMessage warning)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<global::Revit.Elements.Element> failingElements = warning.GetFailingElements().Select(x => doc.GetElement(x).ToDSType(true)).ToList();

            //unfortunately this is not cross language compatible that I know of. :(
            if (warning.GetDescriptionText().Contains("slightly off axis") && failingElements.Count.Equals(1))
            {
                //we land here because for some reason Revit 2019 occasionally has issues retrieving the host too
                var modelLine = failingElements.First(e => e.InternalElement is ModelLine).InternalElement as ModelLine;
                var intersecting = new FilteredElementCollector(doc).WhereElementIsNotElementType().WherePasses(SketchFilter()).ToList();
                var host = intersecting.First(e => e.GetDependentElements(new ElementClassFilter(typeof(CurveElement))).Contains(modelLine.Id));
                failingElements.Add(host.ToDSType(true));
            }
            // grab the additional elements as well.
            failingElements.AddRange(warning.GetAdditionalElements().Select(x => doc.GetElement(x).ToDSType(true)));

            return failingElements.Distinct().ToList();
        }
        //this is to get sketch elements
        [IsVisibleInDynamoLibrary(false)]
        private static ElementMulticategoryFilter SketchFilter()
        {
            List<BuiltInCategory> sketchCategories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_FilledRegion,
                BuiltInCategory.OST_MaskingRegion,
                BuiltInCategory.OST_Ceilings,
                BuiltInCategory.OST_Roofs
            };

            return new ElementMulticategoryFilter(sketchCategories);
        }

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
