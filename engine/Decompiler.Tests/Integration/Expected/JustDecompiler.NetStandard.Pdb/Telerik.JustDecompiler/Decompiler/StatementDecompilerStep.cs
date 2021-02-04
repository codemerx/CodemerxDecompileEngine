using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using Telerik.JustDecompiler.Ast.Expressions;
using Telerik.JustDecompiler.Ast.Statements;
using Telerik.JustDecompiler.Cil;
using Telerik.JustDecompiler.Decompiler.LogicFlow;
using Telerik.JustDecompiler.Decompiler.LogicFlow.Conditions;
using Telerik.JustDecompiler.Decompiler.LogicFlow.Exceptions;
using Telerik.JustDecompiler.Decompiler.LogicFlow.Loops;
using Telerik.JustDecompiler.Decompiler.LogicFlow.Switches;
using Telerik.JustDecompiler.Steps;

namespace Telerik.JustDecompiler.Decompiler
{
	public class StatementDecompilerStep : BaseInstructionVisitor, IDecompilationStep
	{
		private ExpressionDecompilerData expressions;

		private BlockLogicalConstruct logicalTree;

		private ControlFlowGraph theCFG;

		private DecompilationContext context;

		private TypeSystem typeSystem;

		private readonly Dictionary<CFGBlockLogicalConstruct, HashSet<CFGBlockLogicalConstruct>> gotoEndpointToOrigins;

		private readonly Dictionary<CFGBlockLogicalConstruct, HashSet<CFGBlockLogicalConstruct>> gotoOriginsToEndpoints;

		private readonly Dictionary<ILogicalConstruct, string> gotoEndpointConstructToLabel;

		private readonly Dictionary<CFGBlockLogicalConstruct, Dictionary<CFGBlockLogicalConstruct, GotoStatement>> gotoOriginBlockToGoToStatement;

		private readonly Dictionary<ILogicalConstruct, List<Statement>> logicalConstructToStatements;

		private readonly Dictionary<Statement, ILogicalConstruct> statementToLogicalConstruct;

		private readonly Dictionary<CFGBlockLogicalConstruct, CFGBlockLogicalConstruct> breaksOriginToEndPoint;

		private readonly Dictionary<CFGBlockLogicalConstruct, CFGBlockLogicalConstruct> continuesOriginToEndPoint;

		private readonly Dictionary<IBreaksContainer, CFGBlockLogicalConstruct> breaksContainerToBreakEndPoint;

		private List<GotoStatement> contextGotoStatements;

		private Dictionary<string, Statement> contextGotoLabels;

		private uint gotoLabelsCounter;

		private readonly Stack<ILogicalConstruct> parents;

		public StatementDecompilerStep()
		{
			base();
			this.gotoEndpointToOrigins = new Dictionary<CFGBlockLogicalConstruct, HashSet<CFGBlockLogicalConstruct>>();
			this.gotoEndpointConstructToLabel = new Dictionary<ILogicalConstruct, string>();
			this.gotoOriginBlockToGoToStatement = new Dictionary<CFGBlockLogicalConstruct, Dictionary<CFGBlockLogicalConstruct, GotoStatement>>();
			this.gotoOriginsToEndpoints = new Dictionary<CFGBlockLogicalConstruct, HashSet<CFGBlockLogicalConstruct>>();
			this.logicalConstructToStatements = new Dictionary<ILogicalConstruct, List<Statement>>();
			this.statementToLogicalConstruct = new Dictionary<Statement, ILogicalConstruct>();
			this.parents = new Stack<ILogicalConstruct>();
			this.breaksOriginToEndPoint = new Dictionary<CFGBlockLogicalConstruct, CFGBlockLogicalConstruct>();
			this.continuesOriginToEndPoint = new Dictionary<CFGBlockLogicalConstruct, CFGBlockLogicalConstruct>();
			this.breaksContainerToBreakEndPoint = new Dictionary<IBreaksContainer, CFGBlockLogicalConstruct>();
			return;
		}

		private void AddAndRegisterGoToLabel(ILogicalConstruct gotoConstruct, string theLabel)
		{
			V_0 = null;
			if (gotoConstruct as ConditionLogicalConstruct == null || (gotoConstruct as ConditionLogicalConstruct).get_LogicalContainer() == null)
			{
				if (gotoConstruct as CFGBlockLogicalConstruct == null || gotoConstruct.get_Parent() as SwitchLogicalConstruct == null)
				{
					V_0 = this.logicalConstructToStatements.get_Item(gotoConstruct).get_Item(0);
				}
				else
				{
					V_0 = this.logicalConstructToStatements.get_Item(gotoConstruct.get_Parent() as ILogicalConstruct).get_Item(0);
				}
			}
			else
			{
				V_1 = gotoConstruct as ConditionLogicalConstruct;
				if (V_1.get_LogicalContainer() as LoopLogicalConstruct == null)
				{
					if (V_1.get_LogicalContainer() as IfLogicalConstruct == null)
					{
						throw new Exception("Condition containing construct unaccounted for.");
					}
					V_0 = this.logicalConstructToStatements.get_Item(V_1.get_LogicalContainer()).get_Item(0);
				}
				else
				{
					V_2 = (LoopLogicalConstruct)gotoConstruct.get_Parent();
					if (V_2.get_LoopType() != 2)
					{
						if (V_2.get_LoopType() != 1)
						{
							throw new Exception("Infinite loop with condition encountered.");
						}
						V_0 = this.logicalConstructToStatements.get_Item(V_2).get_Item(0);
					}
					else
					{
						V_0 = new EmptyStatement();
						((DoWhileStatement)this.logicalConstructToStatements.get_Item(V_2).get_Item(0)).get_Body().AddStatement(V_0);
					}
				}
			}
			V_0.set_Label(theLabel);
			this.contextGotoLabels.Add(V_0.get_Label(), V_0);
			return;
		}

		private void AppendGoToFromConditionStartingAt(List<CFGBlockLogicalConstruct> goToStartPointHosts, CFGBlockLogicalConstruct theSuccessor, List<Statement> results)
		{
			this.AppendGoToStartingAt(goToStartPointHosts.get_Item(0), results);
			V_0 = this.gotoOriginBlockToGoToStatement.get_Item(goToStartPointHosts.get_Item(0)).get_Item(theSuccessor);
			V_1 = 1;
			while (V_1 < goToStartPointHosts.get_Count())
			{
				if (!this.gotoOriginBlockToGoToStatement.ContainsKey(goToStartPointHosts.get_Item(V_1)))
				{
					this.gotoOriginBlockToGoToStatement.set_Item(goToStartPointHosts.get_Item(V_1), new Dictionary<CFGBlockLogicalConstruct, GotoStatement>());
				}
				this.gotoOriginBlockToGoToStatement.get_Item(goToStartPointHosts.get_Item(V_1)).Add(theSuccessor, V_0);
				V_1 = V_1 + 1;
			}
			return;
		}

		private void AppendGoToStartingAt(CFGBlockLogicalConstruct goToStartPointHost, List<Statement> results)
		{
			V_0 = this.gotoOriginsToEndpoints.get_Item(goToStartPointHost).FirstOrDefault<CFGBlockLogicalConstruct>();
			V_1 = this.ConstructAndRecordGoToStatementToContext("", this.GetJumpingInstructions(goToStartPointHost, V_0));
			V_2 = new Dictionary<CFGBlockLogicalConstruct, GotoStatement>();
			V_2.Add(V_0, V_1);
			this.gotoOriginBlockToGoToStatement.Add(goToStartPointHost, V_2);
			results.Add(V_1);
			return;
		}

		private bool CheckForBreak(ILogicalConstruct theConstruct)
		{
			if (theConstruct as CFGBlockLogicalConstruct == null)
			{
				if (theConstruct as ConditionLogicalConstruct == null)
				{
					throw new Exception("The given construct cannot be break.");
				}
				V_0 = theConstruct.get_Children().GetEnumerator();
				try
				{
					while (V_0.MoveNext())
					{
						V_1 = (CFGBlockLogicalConstruct)V_0.get_Current();
						if (!this.breaksOriginToEndPoint.ContainsKey(V_1))
						{
							continue;
						}
						V_2 = true;
						goto Label1;
					}
					goto Label0;
				}
				finally
				{
					((IDisposable)V_0).Dispose();
				}
			Label1:
				return V_2;
			}
			else
			{
				if (this.breaksOriginToEndPoint.ContainsKey(theConstruct as CFGBlockLogicalConstruct))
				{
					return true;
				}
			}
		Label0:
			return false;
		}

		private bool CheckForContinue(ILogicalConstruct theConstruct)
		{
			if (theConstruct as CFGBlockLogicalConstruct == null)
			{
				if (theConstruct as ConditionLogicalConstruct == null)
				{
					throw new Exception("The given construct cannot be continue.");
				}
				V_0 = theConstruct.get_Children().GetEnumerator();
				try
				{
					while (V_0.MoveNext())
					{
						V_1 = (CFGBlockLogicalConstruct)V_0.get_Current();
						if (!this.continuesOriginToEndPoint.ContainsKey(V_1))
						{
							continue;
						}
						V_2 = true;
						goto Label1;
					}
					goto Label0;
				}
				finally
				{
					((IDisposable)V_0).Dispose();
				}
			Label1:
				return V_2;
			}
			else
			{
				if (this.continuesOriginToEndPoint.ContainsKey(theConstruct as CFGBlockLogicalConstruct))
				{
					return true;
				}
			}
		Label0:
			return false;
		}

		private bool CheckForNormalFlowReachability(ILogicalConstruct startPoint, ILogicalConstruct parentConstruct)
		{
			V_0 = startPoint;
			while (V_0 != parentConstruct)
			{
				if (V_0.get_CFGFollowNode() != null)
				{
					return false;
				}
				V_0 = V_0.get_Parent() as ILogicalConstruct;
			}
			return true;
		}

		private GotoStatement ConstructAndRecordGoToStatementToContext(string label, IEnumerable<Instruction> jumps)
		{
			V_0 = new GotoStatement(label, jumps);
			this.contextGotoStatements.Add(V_0);
			return V_0;
		}

		private void DetermineBreaksEndPoint(IBreaksContainer breaksContainer)
		{
			V_0 = this.GetBreakContainerCFGFollowNode(breaksContainer);
			if (V_0 != null)
			{
				this.breaksContainerToBreakEndPoint.set_Item(breaksContainer, V_0);
			}
			return;
		}

		private bool ExistsStatementForConstruct(ILogicalConstruct theConstruct)
		{
			if (theConstruct as ConditionLogicalConstruct == null || (theConstruct as ConditionLogicalConstruct).get_LogicalContainer() == null)
			{
				return this.logicalConstructToStatements.ContainsKey(theConstruct);
			}
			return this.logicalConstructToStatements.ContainsKey((theConstruct as ConditionLogicalConstruct).get_LogicalContainer());
		}

		private void FindAndMarkGoToExits(ILogicalConstruct current, bool gotoReachableConstruct)
		{
			if (current as IBreaksContainer != null)
			{
				this.DetermineBreaksEndPoint((IBreaksContainer)current);
			}
			if (current as CFGBlockLogicalConstruct != null)
			{
				dummyVar0 = this.FindBreak(current as CFGBlockLogicalConstruct);
				dummyVar1 = this.FindContinue(current as CFGBlockLogicalConstruct);
			}
			V_0 = current.get_AllSuccessors().GetEnumerator();
			try
			{
				while (V_0.MoveNext())
				{
					V_1 = (ILogicalConstruct)V_0.get_Current();
					V_2 = this.GetNearestFollowNode(current);
					if (V_1.get_FirstBlock() == V_2)
					{
						if (current as LoopLogicalConstruct == null || (current as LoopLogicalConstruct).get_LoopType() == LoopType.InfiniteLoop || (current as LoopLogicalConstruct).get_LoopCondition().get_FalseCFGSuccessor() == V_2.get_FirstBlock())
						{
							if (!gotoReachableConstruct)
							{
								continue;
							}
							V_3 = V_2;
							while (V_3.get_Parent() != null && ((ILogicalConstruct)V_3.get_Parent()).get_FirstBlock() == V_2)
							{
								V_3 = (ILogicalConstruct)V_3.get_Parent();
							}
							if (!this.ExistsStatementForConstruct(V_3))
							{
								continue;
							}
							this.MarkGoTosIfNotLoopEdge(current, V_1);
						}
						else
						{
							this.MarkGoTosIfNotLoopEdge(current, V_1);
						}
					}
					else
					{
						this.MarkGoTosIfNotLoopEdge(current, V_1);
					}
				}
			}
			finally
			{
				((IDisposable)V_0).Dispose();
			}
			return;
		}

		private CFGBlockLogicalConstruct FindBreak(CFGBlockLogicalConstruct startPoint)
		{
			if (this.breaksOriginToEndPoint.ContainsKey(startPoint))
			{
				return this.breaksOriginToEndPoint.get_Item(startPoint);
			}
			V_0 = this.GetInnerMostParentOfType<IBreaksContainer>(startPoint);
			if (V_0 == null || this.ExistsStatementForConstruct(V_0))
			{
				return null;
			}
			if (!this.breaksContainerToBreakEndPoint.TryGetValue(V_0, out V_1) || !startPoint.get_CFGSuccessors().Contains(V_1))
			{
				return null;
			}
			this.breaksOriginToEndPoint.set_Item(startPoint, V_1);
			return V_1;
		}

		private CFGBlockLogicalConstruct FindContinue(CFGBlockLogicalConstruct startPoint)
		{
			if (this.continuesOriginToEndPoint.ContainsKey(startPoint))
			{
				return this.continuesOriginToEndPoint.get_Item(startPoint);
			}
			V_0 = this.GetInnerMostParentOfType<LoopLogicalConstruct>(startPoint);
			if (V_0 == null || this.ExistsStatementForConstruct(V_0))
			{
				return null;
			}
			if (startPoint.get_CFGSuccessors().Contains(V_0.get_LoopContinueEndPoint()))
			{
				if (!this.CheckForNormalFlowReachability(startPoint, V_0))
				{
					this.continuesOriginToEndPoint.set_Item(startPoint, V_0.get_LoopContinueEndPoint());
					return V_0.get_LoopContinueEndPoint();
				}
				V_1 = this.GetInnerMostParentOfType<IBreaksContainer>(startPoint);
				if (V_1 != null)
				{
					V_2 = this.GetInnerMostParentOfType<LoopLogicalConstruct>(V_1);
					if (V_2 != null && V_2 == V_0)
					{
						this.continuesOriginToEndPoint.set_Item(startPoint, V_0.get_LoopContinueEndPoint());
						return V_0.get_LoopContinueEndPoint();
					}
				}
			}
			return null;
		}

		private ILogicalConstruct FindGoToEndpointConstruct(CFGBlockLogicalConstruct gotoEndPoint, CFGBlockLogicalConstruct gotoOrigin)
		{
			if (gotoEndPoint == gotoOrigin)
			{
				if (gotoEndPoint.get_Parent() as ConditionLogicalConstruct == null || gotoEndPoint.get_Parent().get_Parent() as LoopLogicalConstruct == null && gotoEndPoint.get_Parent().get_Parent() as IfLogicalConstruct == null)
				{
					return (ILogicalConstruct)gotoEndPoint.get_Parent();
				}
				return (ILogicalConstruct)gotoEndPoint.get_Parent().get_Parent();
			}
			stackVariable3 = new ISingleEntrySubGraph[2];
			stackVariable3[0] = gotoEndPoint;
			stackVariable3[1] = gotoOrigin;
			V_0 = (ILogicalConstruct)LogicalFlowUtilities.FindFirstCommonParent((IEnumerable<ISingleEntrySubGraph>)stackVariable3);
			V_1 = gotoEndPoint;
			while (V_1.get_Parent() != V_0)
			{
				V_1 = (ILogicalConstruct)V_1.get_Parent();
			}
			if (V_1.get_FirstBlock() != gotoEndPoint)
			{
				throw new Exception("GoTo misplaced.");
			}
			if (V_1 as CaseLogicalConstruct == null)
			{
				if (V_1 as CFGBlockLogicalConstruct != null && V_1.get_Parent() as SwitchLogicalConstruct != null)
				{
					V_1 = V_1.get_Parent() as ILogicalConstruct;
				}
			}
			else
			{
				V_1 = V_1.get_Entry() as ILogicalConstruct;
			}
			return V_1;
		}

		private ILogicalConstruct FindTopMostParentOfBlock(CFGBlockLogicalConstruct gotoEndPoint)
		{
			V_0 = gotoEndPoint;
			while (V_0.get_Parent() != null && ((ILogicalConstruct)V_0.get_Parent()).get_FirstBlock() == gotoEndPoint)
			{
				V_0 = (ILogicalConstruct)V_0.get_Parent();
			}
			if (V_0 as BlockLogicalConstruct != null)
			{
				V_0 = (ILogicalConstruct)V_0.get_Entry();
			}
			return V_0;
		}

		private string GenerateGoToLabel()
		{
			V_0 = this.gotoLabelsCounter;
			this.gotoLabelsCounter = V_0 + 1;
			return String.Concat("Label", V_0.ToString());
		}

		private CFGBlockLogicalConstruct GetBreakContainerCFGFollowNode(IBreaksContainer breaksContainer)
		{
			V_0 = breaksContainer;
			while (V_0 != null && V_0.get_CFGFollowNode() == null)
			{
				V_0 = V_0.get_Parent() as ILogicalConstruct;
				if (V_0 as IBreaksContainer == null)
				{
					continue;
				}
				return null;
			}
			if (V_0 == null || this.ExistsStatementForConstruct(V_0.get_FollowNode()))
			{
				return null;
			}
			if (breaksContainer as SwitchLogicalConstruct != null)
			{
				return V_0.get_CFGFollowNode();
			}
			V_1 = (LoopLogicalConstruct)breaksContainer;
			if (V_1.get_LoopType() != LoopType.InfiniteLoop)
			{
				V_2 = V_1.get_LoopCondition();
			}
			else
			{
				V_2 = V_1.get_LoopBodyBlock();
			}
			if (!V_2.get_CFGSuccessors().Contains(V_0.get_CFGFollowNode()))
			{
				return null;
			}
			return V_0.get_CFGFollowNode();
		}

		private CFGBlockLogicalConstruct GetEffectiveGotoEndPoint(CFGBlockLogicalConstruct gotoEndPointBlock)
		{
			V_0 = gotoEndPointBlock;
			while (this.logicalConstructToStatements.TryGetValue(V_0, out V_1) && V_1.get_Count() == 0)
			{
				V_2 = V_0.get_CFGSuccessors().GetEnumerator();
				V_3 = V_2;
				try
				{
					if (!V_2.MoveNext())
					{
						throw new Exception("End block with no statements reached.");
					}
					V_0 = V_2.get_Current();
					if (V_2.MoveNext())
					{
						throw new Exception("No statements generated for multi exit block");
					}
				}
				finally
				{
					if (V_3 != null)
					{
						V_3.Dispose();
					}
				}
			}
			return V_0;
		}

		private T GetInnerMostParentOfType<T>()
		where T : class
		{
			V_0 = this.parents.ToArray();
			V_1 = 0;
			while (V_1 < (int)V_0.Length)
			{
				V_2 = V_0[V_1];
				if (V_2 as T != null)
				{
					return (T)(V_2 as T);
				}
				V_1 = V_1 + 1;
			}
			V_3 = default(T);
			return V_3;
		}

		private T GetInnerMostParentOfType<T>(ILogicalConstruct startNode)
		where T : class
		{
			V_0 = startNode.get_Parent() as ILogicalConstruct;
			while (V_0 != null)
			{
				if (V_0 as T != null)
				{
					return (T)(V_0 as T);
				}
				V_0 = V_0.get_Parent() as ILogicalConstruct;
			}
			V_1 = default(T);
			return V_1;
		}

		private List<Instruction> GetJumpingInstructions(ILogicalConstruct from, CFGBlockLogicalConstruct to)
		{
			if (from as CFGBlockLogicalConstruct == null || from.get_Parent() as ConditionLogicalConstruct != null || !this.IsUnconditionalJump((from as CFGBlockLogicalConstruct).get_TheBlock().get_Last()))
			{
				return null;
			}
			V_0 = new List<Instruction>();
			V_1 = from.get_CFGBlocks().GetEnumerator();
			try
			{
				while (V_1.MoveNext())
				{
					V_2 = V_1.get_Current();
					if (!V_2.get_CFGSuccessors().Contains(to))
					{
						continue;
					}
					V_0.Add(V_2.get_TheBlock().get_Last());
				}
			}
			finally
			{
				((IDisposable)V_1).Dispose();
			}
			return V_0;
		}

		private CFGBlockLogicalConstruct GetNearestFollowNode(ILogicalConstruct current)
		{
			if (current.get_CFGFollowNode() != null)
			{
				return current.get_CFGFollowNode();
			}
			V_0 = this.parents.ToArray();
			V_1 = 0;
			while (V_1 < (int)V_0.Length)
			{
				V_2 = V_0[V_1];
				if (V_2.get_CFGFollowNode() != null)
				{
					return V_2.get_CFGFollowNode();
				}
				V_1 = V_1 + 1;
			}
			return null;
		}

		private List<CFGBlockLogicalConstruct> HostsGoToStartPoint(ConditionLogicalConstruct condition)
		{
			V_0 = new List<CFGBlockLogicalConstruct>();
			V_1 = condition.get_CFGBlocks().GetEnumerator();
			try
			{
				while (V_1.MoveNext())
				{
					V_2 = V_1.get_Current();
					if (!this.gotoOriginsToEndpoints.ContainsKey(V_2))
					{
						continue;
					}
					V_0.Add(V_2);
				}
			}
			finally
			{
				((IDisposable)V_1).Dispose();
			}
			return V_0;
		}

		private BlockStatement InsertGotoEndpoints(BlockStatement body)
		{
			V_0 = false;
			do
			{
				V_1 = new HashSet<ILogicalConstruct>();
				V_2 = this.gotoEndpointToOrigins.get_Keys().GetEnumerator();
				try
				{
					while (V_2.MoveNext())
					{
						V_3 = V_2.get_Current();
						V_4 = this.FindTopMostParentOfBlock(V_3);
						if (this.ExistsStatementForConstruct(V_4))
						{
							continue;
						}
						dummyVar0 = V_1.Add(V_4);
					}
				}
				finally
				{
					((IDisposable)V_2).Dispose();
				}
				V_0 = V_1.get_Count() > 0;
				V_5 = V_1.GetEnumerator();
				try
				{
					while (V_5.MoveNext())
					{
						V_6 = V_5.get_Current();
						if (this.ExistsStatementForConstruct(V_6))
						{
							continue;
						}
						V_7 = this.TraverseGoToOnlyReachableStatements(V_6).GetEnumerator();
						try
						{
							while (V_7.MoveNext())
							{
								V_8 = V_7.get_Current();
								body.AddStatement(V_8);
							}
						}
						finally
						{
							((IDisposable)V_7).Dispose();
						}
					}
				}
				finally
				{
					((IDisposable)V_5).Dispose();
				}
			}
			while (V_0);
			V_2 = this.gotoEndpointToOrigins.get_Keys().GetEnumerator();
			try
			{
				while (V_2.MoveNext())
				{
					V_9 = V_2.get_Current();
					V_10 = this.GetEffectiveGotoEndPoint(V_9);
					V_11 = this.gotoEndpointToOrigins.get_Item(V_9).GetEnumerator();
					try
					{
						while (V_11.MoveNext())
						{
							V_12 = V_11.get_Current();
							V_13 = this.FindGoToEndpointConstruct(V_10, V_12);
							if (!this.gotoEndpointConstructToLabel.ContainsKey(V_13))
							{
								V_14 = this.GenerateGoToLabel();
								this.AddAndRegisterGoToLabel(V_13, V_14);
								this.gotoEndpointConstructToLabel.set_Item(V_13, V_14);
							}
							this.gotoOriginBlockToGoToStatement.get_Item(V_12).get_Item(V_9).set_TargetLabel(this.gotoEndpointConstructToLabel.get_Item(V_13));
						}
					}
					finally
					{
						((IDisposable)V_11).Dispose();
					}
				}
			}
			finally
			{
				((IDisposable)V_2).Dispose();
			}
			return body;
		}

		private bool IsLoopCondition(CFGBlockLogicalConstruct cfgBlock)
		{
			if (cfgBlock.get_Parent() as ConditionLogicalConstruct == null)
			{
				return false;
			}
			return (cfgBlock.get_Parent() as ConditionLogicalConstruct).get_LogicalContainer() as LoopLogicalConstruct != null;
		}

		private bool IsUnconditionalJump(Instruction instruction)
		{
			V_0 = instruction.get_OpCode().get_Code();
			if (V_0 == 55)
			{
				return true;
			}
			return V_0 == 42;
		}

		private void MarkGoTosIfNotLoopEdge(ILogicalConstruct start, ILogicalConstruct end)
		{
			V_0 = end.get_FirstBlock();
			V_1 = this.GetInnerMostParentOfType<LoopLogicalConstruct>();
			V_2 = start.get_CFGBlocks();
			V_3 = V_0.get_CFGPredecessors().GetEnumerator();
			try
			{
				while (V_3.MoveNext())
				{
					V_4 = V_3.get_Current();
					if (!V_2.Contains(V_4))
					{
						continue;
					}
					if (!this.IsLoopCondition(V_4))
					{
						V_5 = this.GetInnerMostParentOfType<LoopLogicalConstruct>(V_4);
					}
					else
					{
						V_5 = this.GetInnerMostParentOfType<LoopLogicalConstruct>(V_4.get_Parent().get_Parent() as ILogicalConstruct);
					}
					V_6 = this.GetInnerMostParentOfType<IBreaksContainer>(V_4);
					stackVariable27 = this.FindBreak(V_4);
					V_7 = this.FindContinue(V_4);
					if (stackVariable27 == V_0 && !this.ExistsStatementForConstruct(V_6) || V_7 == V_0 && !this.ExistsStatementForConstruct(V_5) || V_1 != null && V_5 == V_1 && !this.ExistsStatementForConstruct(V_1) && V_1.get_LoopContinueEndPoint() == V_0 && this.CheckForNormalFlowReachability(V_4, V_1))
					{
						continue;
					}
					if (!this.gotoOriginsToEndpoints.TryGetValue(V_4, out V_8))
					{
						stackVariable40 = false;
					}
					else
					{
						stackVariable40 = V_8.Contains(V_0);
					}
					if (stackVariable40)
					{
						continue;
					}
					if (!this.gotoEndpointToOrigins.ContainsKey(V_0))
					{
						this.gotoEndpointToOrigins.Add(V_0, new HashSet<CFGBlockLogicalConstruct>());
					}
					if (!this.gotoEndpointToOrigins.get_Item(V_0).Contains(V_4))
					{
						dummyVar0 = this.gotoEndpointToOrigins.get_Item(V_0).Add(V_4);
					}
					if (this.gotoOriginsToEndpoints.ContainsKey(V_4))
					{
						dummyVar1 = this.gotoOriginsToEndpoints.get_Item(V_4).Add(V_0);
					}
					else
					{
						stackVariable62 = this.gotoOriginsToEndpoints;
						stackVariable65 = new CFGBlockLogicalConstruct[1];
						stackVariable65[0] = V_0;
						stackVariable62.Add(V_4, new HashSet<CFGBlockLogicalConstruct>(stackVariable65));
					}
				}
			}
			finally
			{
				((IDisposable)V_3).Dispose();
			}
			return;
		}

		public BlockStatement Process(DecompilationContext context, BlockStatement body)
		{
			this.context = context;
			this.contextGotoLabels = context.get_MethodContext().get_GotoLabels();
			this.contextGotoStatements = context.get_MethodContext().get_GotoStatements();
			this.theCFG = context.get_MethodContext().get_ControlFlowGraph();
			this.typeSystem = context.get_MethodContext().get_Method().get_Module().get_TypeSystem();
			this.logicalTree = context.get_MethodContext().get_LogicalConstructsTree();
			this.expressions = context.get_MethodContext().get_Expressions();
			body = (BlockStatement)this.ProcessLogicalConstruct(this.logicalTree, false).get_Item(0);
			body = this.InsertGotoEndpoints(body);
			context.get_MethodContext().set_StatementToLogicalConstruct(this.statementToLogicalConstruct);
			context.get_MethodContext().set_LogicalConstructToStatements(this.logicalConstructToStatements);
			return body;
		}

		private BlockStatement ProcessBlockLogicalConstruct(ILogicalConstruct theConstruct, bool gotoReachableConstruct)
		{
			V_0 = new BlockStatement();
			V_1 = (ILogicalConstruct)theConstruct.get_Entry();
			while (V_1 != null)
			{
				V_2 = this.ProcessLogicalConstruct(V_1, gotoReachableConstruct).GetEnumerator();
				try
				{
					while (V_2.MoveNext())
					{
						V_3 = V_2.get_Current();
						V_0.AddStatement(V_3);
					}
				}
				finally
				{
					((IDisposable)V_2).Dispose();
				}
				V_1 = V_1.get_FollowNode();
			}
			return V_0;
		}

		private void ProcessCfgBlockLogicalConstruct(CFGBlockLogicalConstruct theBlock, List<Statement> results, bool closestBreakContainerExists, bool closestLoopParentExists)
		{
			// 
			// Current member / type: System.Void Telerik.JustDecompiler.Decompiler.StatementDecompilerStep::ProcessCfgBlockLogicalConstruct(Telerik.JustDecompiler.Decompiler.LogicFlow.CFGBlockLogicalConstruct,System.Collections.Generic.List`1<Telerik.JustDecompiler.Ast.Statements.Statement>,System.Boolean,System.Boolean)
			// Exception in: System.Void ProcessCfgBlockLogicalConstruct(Telerik.JustDecompiler.Decompiler.LogicFlow.CFGBlockLogicalConstruct,System.Collections.Generic.List<Telerik.JustDecompiler.Ast.Statements.Statement>,System.Boolean,System.Boolean)
			// Specified argument was out of the range of valid values. (Parameter 'Target of array indexer expression is not an array.')
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		private IfStatement ProcessConditionLogicalConstruct(ConditionLogicalConstruct clc, bool closestBreakContainerExists, IBreaksContainer closestBreakContainerParent, bool closestLoopParentExists)
		{
			V_0 = new BlockStatement();
			V_1 = new BlockStatement();
			if (!closestBreakContainerExists && this.CheckForBreak(clc))
			{
				V_3 = this.breaksContainerToBreakEndPoint.get_Item(closestBreakContainerParent);
				V_4 = this.GetJumpingInstructions(clc, V_3);
				if (clc.get_TrueCFGSuccessor() != V_3)
				{
					if (clc.get_FalseCFGSuccessor() != V_3)
					{
						throw new Exception("Incorrect mark as break child!");
					}
					V_1.AddStatement(new BreakStatement(V_4));
				}
				else
				{
					V_0.AddStatement(new BreakStatement(V_4));
				}
			}
			if (!closestLoopParentExists && this.CheckForContinue(clc))
			{
				V_5 = this.GetInnerMostParentOfType<LoopLogicalConstruct>().get_LoopContinueEndPoint();
				if (clc.get_TrueCFGSuccessor() != V_5 || V_0.get_Statements().get_Count() != 0)
				{
					if (clc.get_FalseCFGSuccessor() != V_5 || V_1.get_Statements().get_Count() != 0)
					{
						throw new Exception("Incorrect mark as continue child!");
					}
					V_1.AddStatement(new ContinueStatement(this.GetJumpingInstructions(clc, V_5)));
				}
				else
				{
					V_0.AddStatement(new ContinueStatement(this.GetJumpingInstructions(clc, V_5)));
				}
			}
			if (!this.gotoEndpointToOrigins.ContainsKey(clc.get_TrueCFGSuccessor()))
			{
				stackVariable9 = false;
			}
			else
			{
				stackVariable9 = this.TryCreateGoTosForConditinalConstructSuccessor(V_0, clc, clc.get_TrueCFGSuccessor());
			}
			if (!this.gotoEndpointToOrigins.ContainsKey(clc.get_FalseCFGSuccessor()))
			{
				stackVariable15 = false;
			}
			else
			{
				stackVariable15 = this.TryCreateGoTosForConditinalConstructSuccessor(V_1, clc, clc.get_FalseCFGSuccessor());
			}
			V_2 = stackVariable15;
			if (!stackVariable9 && !V_2 && V_0.get_Statements().get_Count() == 0 && V_1.get_Statements().get_Count() == 0)
			{
				throw new Exception("Orphaned condition not properly marked as goto!");
			}
			if (V_0.get_Statements().get_Count() == 0)
			{
				stackVariable28 = V_0;
				V_0 = V_1;
				V_1 = stackVariable28;
				clc.Negate(this.typeSystem);
			}
			if (V_1.get_Statements().get_Count() == 0)
			{
				V_1 = null;
			}
			return new IfStatement(clc.get_ConditionExpression(), V_0, V_1);
		}

		private TryStatement ProcessExceptionHandlingLogicalConstruct(ExceptionHandlingLogicalConstruct exceptionHandlingConstruct, bool gotoReachableConstruct)
		{
			V_0 = (BlockStatement)this.ProcessLogicalConstruct(exceptionHandlingConstruct.get_Try(), gotoReachableConstruct).get_Item(0);
			if (exceptionHandlingConstruct as TryFinallyLogicalConstruct != null)
			{
				V_1 = exceptionHandlingConstruct as TryFinallyLogicalConstruct;
				V_2 = new FinallyClause((BlockStatement)this.ProcessLogicalConstruct(V_1.get_Finally(), gotoReachableConstruct).get_Item(0), null);
				return new TryStatement(V_0, null, V_2);
			}
			if (exceptionHandlingConstruct as TryFaultLogicalConstruct != null)
			{
				V_3 = exceptionHandlingConstruct as TryFaultLogicalConstruct;
				V_4 = (BlockStatement)this.ProcessLogicalConstruct(V_3.get_Fault(), gotoReachableConstruct).get_Item(0);
				return new TryStatement(V_0, V_4, null);
			}
			stackVariable14 = new TryStatement();
			stackVariable14.set_Try(V_0);
			V_5 = stackVariable14;
			V_6 = (exceptionHandlingConstruct as TryCatchFilterLogicalConstruct).get_Handlers();
			V_7 = 0;
			while (V_7 < (int)V_6.Length)
			{
				V_8 = V_6[V_7];
				if (V_8 as ExceptionHandlingBlockCatch == null)
				{
					V_13 = V_8 as ExceptionHandlingBlockFilter;
					V_14 = (BlockStatement)this.ProcessLogicalConstruct(V_13.get_Filter(), gotoReachableConstruct).get_Item(0);
					V_15 = (BlockStatement)this.ProcessLogicalConstruct(V_13.get_Handler(), gotoReachableConstruct).get_Item(0);
					V_5.AddToCatchClauses(new CatchClause(V_15, null, null, V_14));
				}
				else
				{
					V_9 = V_8 as ExceptionHandlingBlockCatch;
					V_10 = (BlockStatement)this.ProcessLogicalConstruct(V_9, gotoReachableConstruct).get_Item(0);
					if (this.context.get_MethodContext().get_StackData().get_ExceptionHandlerStartToExceptionVariableMap().TryGetValue(V_9.get_FirstBlock().get_TheBlock().get_First().get_Offset(), out V_11))
					{
						stackVariable77 = new VariableDeclarationExpression(V_11, null);
					}
					else
					{
						stackVariable77 = null;
					}
					V_12 = stackVariable77;
					V_5.AddToCatchClauses(new CatchClause(V_10, V_9.get_CatchType(), V_12, null));
				}
				V_7 = V_7 + 1;
			}
			return V_5;
		}

		private IfStatement ProcessIfLogicalConstruct(IfLogicalConstruct theIf, bool gotoReachableConstruct)
		{
			V_0 = (BlockStatement)this.ProcessLogicalConstruct(theIf.get_Then(), gotoReachableConstruct).get_Item(0);
			V_1 = null;
			if (theIf.get_Else() != null)
			{
				V_1 = (BlockStatement)this.ProcessLogicalConstruct(theIf.get_Else(), gotoReachableConstruct).get_Item(0);
			}
			if (V_1 != null && V_1.get_Statements().get_Count() == 0)
			{
				V_1 = null;
			}
			if (V_1 != null && V_0.get_Statements().get_Count() == 0)
			{
				theIf.Negate(this.typeSystem);
				V_0 = V_1;
				V_1 = null;
			}
			V_2 = new IfStatement(theIf.get_Condition().get_ConditionExpression(), V_0, V_1);
			V_3 = this.HostsGoToStartPoint(theIf.get_Condition());
			if (V_3.get_Count() != 0)
			{
				if (theIf.get_Else() != null)
				{
					throw new Exception("Malformed IF statement.");
				}
				V_4 = new List<Statement>();
				this.AppendGoToFromConditionStartingAt(V_3, theIf.get_Condition().get_FalseCFGSuccessor(), V_4);
				V_2.set_Else(new BlockStatement());
				V_2.get_Else().AddStatement(V_4.get_Item(0));
			}
			return V_2;
		}

		private List<Statement> ProcessLogicalConstruct(ILogicalConstruct theConstruct, bool gotoReachableConstruct)
		{
			if (this.logicalConstructToStatements.ContainsKey(theConstruct))
			{
				return this.logicalConstructToStatements.get_Item(theConstruct);
			}
			V_0 = new List<Statement>();
			this.FindAndMarkGoToExits(theConstruct, gotoReachableConstruct);
			this.parents.Push(theConstruct);
			if (theConstruct as BlockLogicalConstruct == null)
			{
				V_1 = this.GetInnerMostParentOfType<LoopLogicalConstruct>(theConstruct);
				if (V_1 == null)
				{
					stackVariable17 = false;
				}
				else
				{
					stackVariable17 = this.ExistsStatementForConstruct(V_1);
				}
				V_2 = stackVariable17;
				V_3 = this.GetInnerMostParentOfType<IBreaksContainer>(theConstruct);
				if (V_3 == null)
				{
					stackVariable22 = false;
				}
				else
				{
					stackVariable22 = this.ExistsStatementForConstruct(V_3);
				}
				V_4 = stackVariable22;
				if (theConstruct as ExceptionHandlingLogicalConstruct == null)
				{
					if (theConstruct as IfLogicalConstruct == null)
					{
						if (theConstruct as LoopLogicalConstruct == null)
						{
							if (theConstruct as SwitchLogicalConstruct == null)
							{
								if (theConstruct as CFGBlockLogicalConstruct == null)
								{
									if (theConstruct as ConditionLogicalConstruct != null)
									{
										V_0.Add(this.ProcessConditionLogicalConstruct(theConstruct as ConditionLogicalConstruct, V_4, V_3, V_2));
									}
								}
								else
								{
									this.ProcessCfgBlockLogicalConstruct(theConstruct as CFGBlockLogicalConstruct, V_0, V_4, V_2);
								}
							}
							else
							{
								V_0.Add(this.ProcessSwitchLogicalConstruct(theConstruct as SwitchLogicalConstruct, gotoReachableConstruct));
							}
						}
						else
						{
							this.ProcessLoopLogicalConstruct(theConstruct as LoopLogicalConstruct, gotoReachableConstruct, V_0);
						}
					}
					else
					{
						V_0.Add(this.ProcessIfLogicalConstruct(theConstruct as IfLogicalConstruct, gotoReachableConstruct));
					}
				}
				else
				{
					V_0.Add(this.ProcessExceptionHandlingLogicalConstruct(theConstruct as ExceptionHandlingLogicalConstruct, gotoReachableConstruct));
				}
			}
			else
			{
				V_0.Add(this.ProcessBlockLogicalConstruct(theConstruct, gotoReachableConstruct));
			}
			dummyVar0 = this.parents.Pop();
			this.logicalConstructToStatements.Add(theConstruct, V_0);
			V_5 = V_0.GetEnumerator();
			try
			{
				while (V_5.MoveNext())
				{
					V_6 = V_5.get_Current();
					this.statementToLogicalConstruct.set_Item(V_6, theConstruct);
				}
			}
			finally
			{
				((IDisposable)V_5).Dispose();
			}
			return V_0;
		}

		private void ProcessLoopLogicalConstruct(LoopLogicalConstruct theLogicalLoop, bool gotoReachableConstruct, List<Statement> results)
		{
			if (theLogicalLoop.get_LoopBodyBlock() == null)
			{
				V_0 = new BlockStatement();
			}
			else
			{
				V_0 = (BlockStatement)this.ProcessLogicalConstruct(theLogicalLoop.get_LoopBodyBlock(), gotoReachableConstruct).get_Item(0);
			}
			if (theLogicalLoop.get_LoopCondition() == null)
			{
				V_1 = new LiteralExpression(true, this.typeSystem, null);
			}
			else
			{
				V_1 = theLogicalLoop.get_LoopCondition().get_ConditionExpression();
			}
			if (theLogicalLoop.get_LoopType() == 1 || theLogicalLoop.get_LoopType() == LoopType.InfiniteLoop)
			{
				results.Add(new WhileStatement(V_1, V_0));
			}
			else
			{
				if (theLogicalLoop.get_LoopType() == 2)
				{
					results.Add(new DoWhileStatement(V_1, V_0));
				}
			}
			if (theLogicalLoop.get_LoopCondition() != null)
			{
				V_2 = this.HostsGoToStartPoint(theLogicalLoop.get_LoopCondition());
				if (V_2.get_Count() != 0)
				{
					this.AppendGoToFromConditionStartingAt(V_2, theLogicalLoop.get_LoopCondition().get_FalseCFGSuccessor(), results);
				}
			}
			return;
		}

		private SwitchStatement ProcessSwitchLogicalConstruct(SwitchLogicalConstruct theLogicalSwitch, bool gotoReachableConstruct)
		{
			V_0 = new SwitchStatement(theLogicalSwitch.get_SwitchConditionExpression(), (theLogicalSwitch.get_Entry() as ILogicalConstruct).get_FirstBlock().get_TheBlock().get_Last());
			V_1 = new Dictionary<CFGBlockLogicalConstruct, GotoStatement>();
			this.gotoOriginBlockToGoToStatement.Add(theLogicalSwitch.get_FirstBlock(), V_1);
			V_2 = 0;
			V_3 = 0;
			while (V_2 != (int)theLogicalSwitch.get_ConditionCases().Length || V_3 != theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Count())
			{
				V_5 = null;
				V_6 = null;
				if (V_2 >= (int)theLogicalSwitch.get_ConditionCases().Length || V_3 >= theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Count())
				{
					if (V_2 != (int)theLogicalSwitch.get_ConditionCases().Length)
					{
						V_5 = theLogicalSwitch.get_ConditionCases()[V_2];
						V_4 = V_5.get_CaseNumbers();
						V_2 = V_2 + 1;
					}
					else
					{
						V_9 = theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Item(V_3);
						V_4 = V_9.get_Key();
						V_9 = theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Item(V_3);
						V_6 = V_9.get_Value();
						V_3 = V_3 + 1;
					}
				}
				else
				{
					if (theLogicalSwitch.get_ConditionCases()[V_2].get_CaseNumbers().get_Item(0) >= theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Item(V_3).get_Key().get_Item(0))
					{
						V_9 = theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Item(V_3);
						V_4 = V_9.get_Key();
						V_9 = theLogicalSwitch.get_NonDominatedCFGSuccessors().get_Item(V_3);
						V_6 = V_9.get_Value();
						V_3 = V_3 + 1;
					}
					else
					{
						V_5 = theLogicalSwitch.get_ConditionCases()[V_2];
						V_4 = V_5.get_CaseNumbers();
						V_2 = V_2 + 1;
					}
				}
				V_10 = 0;
				while (V_10 < V_4.get_Count() - 1)
				{
					V_11 = V_4.get_Item(V_10);
					stackVariable53 = new ConditionCase();
					stackVariable53.set_Condition(new LiteralExpression((object)V_11, this.typeSystem, null));
					V_0.AddCase(stackVariable53);
					V_10 = V_10 + 1;
				}
				V_7 = new LiteralExpression((object)V_4.get_Item(V_4.get_Count() - 1), this.typeSystem, null);
				V_7.set_ExpressionType(this.context.get_MethodContext().get_Method().get_Module().get_TypeSystem().get_Int32());
				if (V_5 == null)
				{
					V_8 = new BlockStatement();
					if (!this.continuesOriginToEndPoint.TryGetValue(theLogicalSwitch.get_FirstBlock(), out V_13) || V_13 != V_6)
					{
						if (!this.gotoEndpointToOrigins.TryGetValue(V_6, out V_12) || !V_12.Contains(theLogicalSwitch.get_FirstBlock()))
						{
							V_8.AddStatement(new BreakStatement(null));
						}
						else
						{
							V_14 = this.ConstructAndRecordGoToStatementToContext("", this.GetJumpingInstructions(theLogicalSwitch.get_FirstBlock(), V_6));
							V_1.Add(V_6, V_14);
							V_8.AddStatement(V_14);
						}
					}
					else
					{
						V_8.AddStatement(new ContinueStatement(null));
					}
				}
				else
				{
					V_8 = (BlockStatement)this.ProcessLogicalConstruct(V_5, gotoReachableConstruct).get_Item(0);
				}
				if (SwitchHelpers.BlockHasFallThroughSemantics(V_8))
				{
					V_8.AddStatement(new BreakSwitchCaseStatement());
				}
				V_0.AddCase(new ConditionCase(V_7, V_8));
			}
			if (theLogicalSwitch.get_DefaultCase() == null)
			{
				V_16 = new BlockStatement();
				if (!this.continuesOriginToEndPoint.TryGetValue(theLogicalSwitch.get_FirstBlock(), out V_18) || V_18 != theLogicalSwitch.get_DefaultCFGSuccessor())
				{
					if (this.gotoEndpointToOrigins.TryGetValue(theLogicalSwitch.get_DefaultCFGSuccessor(), out V_17) && V_17.Contains(theLogicalSwitch.get_FirstBlock()))
					{
						V_19 = this.ConstructAndRecordGoToStatementToContext("", this.GetJumpingInstructions(theLogicalSwitch.get_FirstBlock(), theLogicalSwitch.get_DefaultCFGSuccessor()));
						V_1.Add(theLogicalSwitch.get_DefaultCFGSuccessor(), V_19);
						V_16.AddStatement(V_19);
					}
				}
				else
				{
					V_16.AddStatement(new ContinueStatement(null));
				}
				if (V_16.get_Statements().get_Count() > 0)
				{
					V_0.AddCase(new DefaultCase(V_16));
				}
			}
			else
			{
				V_15 = (BlockStatement)this.ProcessLogicalConstruct(theLogicalSwitch.get_DefaultCase(), gotoReachableConstruct).get_Item(0);
				if (this.gotoEndpointToOrigins.ContainsKey(theLogicalSwitch.get_DefaultCase().get_FirstBlock()) || V_15.get_Statements().get_Count() > 1 || V_15.get_Statements().get_Count() == 1 && V_15.get_Statements().get_Item(0).get_CodeNodeType() != 9)
				{
					if (SwitchHelpers.BlockHasFallThroughSemantics(V_15))
					{
						V_15.AddStatement(new BreakSwitchCaseStatement());
					}
					V_0.AddCase(new DefaultCase(V_15));
				}
			}
			return V_0;
		}

		private List<Statement> TraverseGoToOnlyReachableStatements(ILogicalConstruct start)
		{
			V_0 = new List<Statement>();
			V_1 = start;
			do
			{
				V_0.AddRange(this.ProcessLogicalConstruct(V_1, true));
				if (V_1.get_FollowNode() != null && this.ExistsStatementForConstruct(V_1.get_FollowNode()))
				{
					break;
				}
				V_1 = V_1.get_FollowNode();
			}
			while (V_1 != null);
			return V_0;
		}

		private bool TryCreateGoTosForConditinalConstructSuccessor(BlockStatement theBlock, ConditionLogicalConstruct clc, CFGBlockLogicalConstruct theSuccessor)
		{
			V_0 = null;
			V_1 = false;
			V_2 = theSuccessor.get_CFGPredecessors().GetEnumerator();
			try
			{
				while (V_2.MoveNext())
				{
					V_3 = V_2.get_Current();
					if (!clc.get_CFGBlocks().Contains(V_3) || !this.gotoEndpointToOrigins.get_Item(theSuccessor).Contains(V_3))
					{
						continue;
					}
					if (V_0 == null)
					{
						V_0 = this.ConstructAndRecordGoToStatementToContext("", this.GetJumpingInstructions(V_3, theSuccessor));
						theBlock.AddStatement(V_0);
					}
					if (!this.gotoOriginBlockToGoToStatement.ContainsKey(V_3))
					{
						this.gotoOriginBlockToGoToStatement.Add(V_3, new Dictionary<CFGBlockLogicalConstruct, GotoStatement>());
					}
					this.gotoOriginBlockToGoToStatement.get_Item(V_3).Add(theSuccessor, V_0);
					V_1 = true;
				}
			}
			finally
			{
				((IDisposable)V_2).Dispose();
			}
			return V_1;
		}
	}
}