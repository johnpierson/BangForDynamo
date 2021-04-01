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


namespace BangUI
{
    [NodeName("Warnings")]
    [NodeCategory("Bang.Revit.Selection.Selection")]
    [NodeDescription("This provides access to all warnings in your current Revit file. The display is the partial description and the first failing element id. (This is for tracking)")]
    [IsDesignScriptCompatible]
    public class Warnings : RevitDropDownBase
    {
        private List<FailureMessage> RetrieveWarnings(Document doc)
        {
            var extraPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("bin\\BangUI.dll", "extra");
            var disallowedPath = Path.Combine(extraPath, "disallowedWarnings.txt");
            if (File.Exists(disallowedPath))
            {
                try
                {
                    var disallowedWarnings = File.ReadAllText(disallowedPath).ToLower().Replace(" ","").Split(',');

                    List<FailureMessage> allowedFailures = new List<FailureMessage>();

                    foreach (var failureMessage in doc.GetWarnings())
                    {
                        string description = System.Text.RegularExpressions.Regex.Replace(failureMessage.GetDescriptionText().ToLower(), @"[^0-9a-zA-Z]+", "");

                        if (!disallowedWarnings.Any(dw => description.Contains(dw) && dw.Length > 2))
                        {
                            allowedFailures.Add(failureMessage);
                        }
                    }

                    return allowedFailures;
                }
                catch (Exception)
                {
                    //suppress
                }
            }

            return doc.GetWarnings().ToList();
        }
        public class SpecificWarning
        {
            public FailureMessage failureMessage { get; set; }
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
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<FailureMessage> elements = RetrieveWarnings(doc);

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
        public static FailureMessage ById(string id)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            return doc.GetWarnings().First(w => (w.GetFailureDefinitionId().Guid.ToString() + w.GetFailingElements().First()).Equals(id));
        }
        /// <summary>
        /// Get all warnings
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<string> AllFailureDefinitionIds(Document doc)
        {
            return doc.GetWarnings().Select(x => x.GetFailureDefinitionId().Guid.ToString()).ToList();
        }
    }


    [NodeName("All Warnings of Type")]
    [NodeCategory("Bang.Revit.Selection.Selection")]
    [NodeDescription("This provides access to all warnings in your current Revit file. This version returns a list of all of the instances of that warning type.")]
    [IsDesignScriptCompatible]
    public class WarningsOfType : RevitDropDownBase
    {


        private const string outputName = "Warning";

        public WarningsOfType() : base(outputName) { }

        [JsonConstructor]
        public WarningsOfType(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(outputName, inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<FailureMessage> elements = doc.GetWarnings().GroupBy(warning => warning.GetFailureDefinitionId().Guid.ToString()).Select(groupedWarning => groupedWarning.First()).ToList();

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
                AstFactory.BuildStringNode(((FailureMessage) Items[SelectedIndex].Item).GetDescriptionText())
            };
            var functionCall = AstFactory.BuildFunctionCall("Bang.Warning",
                                                            "ByDescription",
                                                            args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }



    }
}
