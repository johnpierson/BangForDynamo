using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using DSRevitNodesUI;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Revit.Application;
using Revit.Elements;


namespace BangUI
{
    [NodeName("Warnings")]
    [NodeCategory("Bang.Revit.Selection.Selection")]
    [NodeDescription("This provides access to all warnings in your current Revit file. The display is the partial description and the first failing element id. (This is for tracking)")]
    [IsDesignScriptCompatible]
    [Obsolete("Revit 2022 is the last version of Revit that Bang will support. Please migrate your projects to use the new OOTB nodes.")]
    public class Warnings : RevitDropDownBase
    {
        private List<Autodesk.Revit.DB.FailureMessage> RetrieveWarnings(Autodesk.Revit.DB.Document doc)
        {
            return doc.GetWarnings().ToList();
        }
        public class SpecificWarning
        {
            public Autodesk.Revit.DB.FailureMessage failureMessage { get; set; }
            public ElementId firstFailingElement { get; set; }
        }

        private const string outputName = "Warning";

        public Warnings() : base(outputName) { }

        [JsonConstructor]
        public Warnings(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            //the current document
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<Autodesk.Revit.DB.FailureMessage> elements = RetrieveWarnings(doc);

            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem("No warnings in this model.", null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            Items = elements.Select(x => new DynamoDropDownItem(x.GetDescriptionText().PadRight(35,'.').Substring(0,35) + "..." + "(" + x.GetFailingElements().First() + ")", new SpecificWarning(){failureMessage = x, firstFailingElement = x.GetFailingElements().First()})).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == "No warnings in this model." ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(((SpecificWarning) Items[SelectedIndex].Item).failureMessage.GetFailureDefinitionId().Guid.ToString() + ((SpecificWarning)Items[SelectedIndex].Item).firstFailingElement)
            };
            var functionCall = AstFactory.BuildFunctionCall("Bang.Warning",
                                                            "ById",
                                                            args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
        /// <summary>
        /// Get warning element by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.FailureMessage ById(string id)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            return doc.GetWarnings().First(w => (w.GetFailureDefinitionId().Guid.ToString() + w.GetFailingElements().First()).Equals(id));
        }
        /// <summary>
        /// Get all warnings
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<string> AllFailureDefinitionIds(Autodesk.Revit.DB.Document doc)
        {
            return doc.GetWarnings().Select(x => x.GetFailureDefinitionId().Guid.ToString()).ToList();
        }
    }


    [NodeName("All Warnings of Type")]
    [NodeCategory("Bang.Revit.Selection.Selection")]
    [NodeDescription("This provides access to all warnings in your current Revit file. This version returns a list of all of the instances of that warning type.")]
    [IsDesignScriptCompatible]
    [Obsolete("Revit 2022 is the last version of Revit that Bang will support. Please migrate your projects to use the new OOTB nodes.")]
    public class WarningsOfType : RevitDropDownBase
    {


        private const string outputName = "Warning";

        public WarningsOfType() : base(outputName) { }

        [JsonConstructor]
        public WarningsOfType(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<Autodesk.Revit.DB.FailureMessage> elements = doc.GetWarnings().GroupBy(warning => warning.GetFailureDefinitionId().Guid.ToString()).Select(groupedWarning => groupedWarning.First()).ToList();

            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem("No warnings in this model.", null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            Items = elements.Select(x => new DynamoDropDownItem(x.GetDescriptionText().PadRight(45, '.').Substring(0, 45) + ".....", x)).ToObservableCollection();
            return SelectionState.Restore;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == "No warnings in this model." ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(((Autodesk.Revit.DB.FailureMessage) Items[SelectedIndex].Item).GetDescriptionText())
            };
            var functionCall = AstFactory.BuildFunctionCall("Bang.Warning",
                                                            "ByDescription",
                                                            args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }



    }
}
