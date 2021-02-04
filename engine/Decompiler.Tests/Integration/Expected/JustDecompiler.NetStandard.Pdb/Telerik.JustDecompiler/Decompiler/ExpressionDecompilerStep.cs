using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Telerik.JustDecompiler.Ast;
using Telerik.JustDecompiler.Ast.Expressions;
using Telerik.JustDecompiler.Ast.Statements;
using Telerik.JustDecompiler.Cil;
using Telerik.JustDecompiler.Decompiler.DefineUseAnalysis;
using Telerik.JustDecompiler.Steps;

namespace Telerik.JustDecompiler.Decompiler
{
	internal class ExpressionDecompilerStep : BaseInstructionVisitor, IDecompilationStep
	{
		private Stack<Expression> expressionStack;

		private readonly Dictionary<int, Expression> offsetToExpression;

		private int currentOffset;

		private readonly HashSet<Expression> used;

		private DecompilationContext context;

		private MethodSpecificContext methodContext;

		private TypeSystem currentTypeSystem;

		private TypeSpecificContext typeContext;

		private readonly ExpressionDecompilerData results;

		private readonly Dictionary<VariableReference, KeyValuePair<int, bool>> exceptionVariables;

		private int dummyVarCounter;

		private readonly HashSet<VariableDefinition> stackVariableAssignmentsToRemove;

		private InstructionBlock currentBlock;

		private Instruction CurrentInstruction
		{
			get
			{
				return this.methodContext.get_ControlFlowGraph().get_OffsetToInstruction().get_Item(this.currentOffset);
			}
		}

		public ExpressionDecompilerStep()
		{
			this.expressionStack = new Stack<Expression>();
			this.stackVariableAssignmentsToRemove = new HashSet<VariableDefinition>();
			base();
			this.offsetToExpression = new Dictionary<int, Expression>();
			this.results = new ExpressionDecompilerData();
			this.used = new HashSet<Expression>();
			this.exceptionVariables = new Dictionary<VariableReference, KeyValuePair<int, bool>>();
			return;
		}

		private void AddDupInstructionsToMapping(InstructionBlock instructionBlock, Instruction instruction, Expression expression)
		{
			if (instruction.get_OpCode().get_Code() != 37)
			{
				V_1 = new List<Instruction>();
				V_2 = instruction;
				while ((object)V_2 != (object)instructionBlock.get_Last() && V_2.get_Next().get_OpCode().get_Code() == 36)
				{
					V_2 = V_2.get_Next();
					V_1.Add(V_2);
				}
				if (V_1.get_Count() > 0)
				{
					expression.MapDupInstructions(V_1);
				}
			}
			return;
		}

		private static void AddRange<T>(IList<T> list, IEnumerable<T> range)
		{
			V_0 = range.GetEnumerator();
			try
			{
				while (V_0.MoveNext())
				{
					V_1 = V_0.get_Current();
					list.Add(V_1);
				}
			}
			finally
			{
				if (V_0 != null)
				{
					V_0.Dispose();
				}
			}
			return;
		}

		private void AddUninlinedStackVariablesToContext()
		{
			V_0 = this.methodContext.get_StackData().get_VariableToDefineUseInfo().GetEnumerator();
			try
			{
				while (V_0.MoveNext())
				{
					V_1 = V_0.get_Current();
					if (V_1.get_Value().get_DefinedAt().get_Count() <= 0)
					{
						continue;
					}
					this.methodContext.get_Variables().Add(V_1.get_Key());
					dummyVar0 = this.methodContext.get_VariablesToRename().Add(V_1.get_Key());
				}
			}
			finally
			{
				((IDisposable)V_0).Dispose();
			}
			return;
		}

		public bool CheckForCastBecauseForeach(Expression expression)
		{
			V_0 = expression as ExplicitCastExpression;
			if (V_0 != null && V_0.get_Expression() as MethodInvocationExpression != null)
			{
				return true;
			}
			return false;
		}

		private ArrayCreationExpression ConvertObjectToArray(ObjectCreationExpression objectCreation)
		{
			V_0 = this.GetElementTypeFromCtor(objectCreation.get_Constructor());
			if (V_0 == null)
			{
				return null;
			}
			stackVariable9 = new ArrayCreationExpression(V_0, null, objectCreation.get_MappedInstructions());
			stackVariable9.set_Dimensions(this.CopyExpressionCollection(objectCreation.get_Arguments()));
			return stackVariable9;
		}

		private ExpressionCollection CopyExpressionCollection(ExpressionCollection arguments)
		{
			V_0 = new ExpressionCollection();
			V_1 = arguments.GetEnumerator();
			try
			{
				while (V_1.MoveNext())
				{
					V_2 = V_1.get_Current();
					V_0.Add(V_2);
				}
			}
			finally
			{
				if (V_1 != null)
				{
					V_1.Dispose();
				}
			}
			return V_0;
		}

		private void CreateExpressions()
		{
			V_0 = this.methodContext.get_ControlFlowGraph().get_Blocks();
			V_1 = 0;
			while (V_1 < (int)V_0.Length)
			{
				V_2 = V_0[V_1];
				this.currentBlock = V_2;
				V_3 = new List<Expression>();
				V_4 = V_2.GetEnumerator();
				try
				{
					while (V_4.MoveNext())
					{
						V_5 = V_4.get_Current();
						this.currentOffset = V_5.get_Offset();
						this.SetInitialStack(V_5);
						this.Visit(V_5);
						if (this.expressionStack.get_Count() == 0)
						{
							continue;
						}
						this.offsetToExpression.Add(V_5.get_Offset(), this.expressionStack.Peek());
						if (V_5.get_OpCode().get_Code() != 37 && this.methodContext.get_StackData().get_InstructionOffsetToAssignedVariableMap().TryGetValue(V_5.get_Offset(), out V_6))
						{
							V_9 = this.expressionStack.Pop();
							if (this.methodContext.get_StackData().get_VariableToDefineUseInfo().get_Item(V_6).get_DefinedAt().get_Count() == 1)
							{
								stackVariable69 = V_9.get_ExpressionType();
							}
							else
							{
								stackVariable69 = null;
							}
							V_6.set_VariableType(stackVariable69);
							this.expressionStack.Push(new BinaryExpression(26, new VariableReferenceExpression(V_6, null), V_9, this.currentTypeSystem, null, false));
							this.AddDupInstructionsToMapping(V_2, V_5, V_9);
						}
						V_3.Add(this.expressionStack.Pop());
					}
				}
				finally
				{
					if (V_4 != null)
					{
						V_4.Dispose();
					}
				}
				if (this.methodContext.get_YieldData() != null)
				{
					this.ProcessYieldBlock(V_2, V_3);
				}
				this.results.get_BlockExpressions().Add(V_2.get_First().get_Offset(), V_3);
				V_1 = V_1 + 1;
			}
			V_11 = this.results.get_BlockExpressions().get_Values().GetEnumerator();
			try
			{
				while (V_11.MoveNext())
				{
					V_12 = V_11.get_Current();
					V_13 = 0;
					while (V_13 < V_12.get_Count())
					{
						V_14 = V_12.get_Item(V_13) as BinaryExpression;
						if (V_14 != null && V_14.get_IsAssignmentExpression() && V_14.get_Left().get_CodeNodeType() == 26 && this.stackVariableAssignmentsToRemove.Contains((V_14.get_Left() as VariableReferenceExpression).get_Variable().Resolve()))
						{
							V_12.RemoveAt(V_13);
						}
						V_13 = V_13 + 1;
					}
				}
			}
			finally
			{
				((IDisposable)V_11).Dispose();
			}
			V_15 = this.methodContext.get_StackData().get_ExceptionHandlerStartToExceptionVariableMap().GetEnumerator();
			try
			{
				while (V_15.MoveNext())
				{
					V_16 = V_15.get_Current();
					this.results.get_ExceptionHandlerStartToVariable().Add(V_16.get_Key(), new VariableReferenceExpression(V_16.get_Value(), null));
				}
			}
			finally
			{
				((IDisposable)V_15).Dispose();
			}
			return;
		}

		private Expression FixCallTarget(Instruction instruction, Expression target)
		{
			if (target != null)
			{
				if (this.IsPointerType(target))
				{
					target = new UnaryExpression(8, target, null);
				}
				if (target.get_HasType() && target.get_ExpressionType() != null)
				{
					V_0 = (MethodReference)instruction.get_Operand();
					V_1 = V_0.Resolve();
					if (V_0.get_DeclaringType() != null && V_1 != null && V_1.get_DeclaringType() != null && V_1.get_DeclaringType().get_IsInterface())
					{
						V_2 = target.get_ExpressionType().Resolve();
						if (V_2 != null && (object)V_2 != (object)V_1.get_DeclaringType() && !V_2.get_IsInterface())
						{
							stackVariable35 = new ExplicitCastExpression(target, V_0.get_DeclaringType(), null);
							stackVariable35.set_IsExplicitInterfaceCast(true);
							return stackVariable35;
						}
					}
				}
			}
			return target;
		}

		private ArrayCreationExpression GetClosestArrayCreationExpression()
		{
			V_0 = this.currentOffset;
			while (V_0 >= 0)
			{
				if (this.offsetToExpression.ContainsKey(V_0) && this.offsetToExpression.get_Item(V_0) as ArrayCreationExpression != null)
				{
					return this.offsetToExpression.get_Item(V_0) as ArrayCreationExpression;
				}
				V_0 = V_0 - 1;
			}
			return null;
		}

		private TypeReference GetElementTypeFromCtor(MethodReference constructor)
		{
			V_0 = constructor.get_DeclaringType() as ArrayType;
			if (V_0 == null)
			{
				return null;
			}
			return V_0.get_ElementType();
		}

		private int GetYieldReturnAssignmentIndex(List<Expression> blockExpressions)
		{
			V_2 = this.methodContext.get_YieldData().get_FieldsInfo();
			V_0 = V_2.get_CurrentItemField();
			V_1 = 0;
			V_3 = blockExpressions.GetEnumerator();
			try
			{
				while (V_3.MoveNext())
				{
					V_4 = V_3.get_Current();
					if (V_4.get_CodeNodeType() != 24 || !(V_4 as BinaryExpression).get_IsAssignmentExpression())
					{
						V_1 = V_1 + 1;
					}
					else
					{
						V_5 = (V_4 as BinaryExpression).get_Left() as FieldReferenceExpression;
						if (V_5 == null || V_5.get_Field().Resolve() != V_0)
						{
							V_1 = V_1 + 1;
						}
						else
						{
							goto Label0;
						}
					}
				}
			}
			finally
			{
				((IDisposable)V_3).Dispose();
			}
		Label0:
			if (V_1 == blockExpressions.get_Count())
			{
				throw new Exception("No assignment of the current field");
			}
			return V_1;
		}

		private Expression GetYieldReturnItem(List<Expression> blockExpressions)
		{
			V_1 = this.methodContext.get_YieldData().get_FieldsInfo();
			V_0 = V_1.get_CurrentItemField();
			V_2 = blockExpressions.GetEnumerator();
			try
			{
				while (V_2.MoveNext())
				{
					V_3 = V_2.get_Current();
					if (V_3.get_CodeNodeType() != 24 || !(V_3 as BinaryExpression).get_IsAssignmentExpression())
					{
						continue;
					}
					V_4 = (V_3 as BinaryExpression).get_Left() as FieldReferenceExpression;
					if (V_4 == null || V_4.get_Field().Resolve() != V_0)
					{
						continue;
					}
					V_5 = (V_3 as BinaryExpression).get_Right();
					goto Label1;
				}
				goto Label0;
			}
			finally
			{
				((IDisposable)V_2).Dispose();
			}
		Label1:
			return V_5;
		Label0:
			throw new Exception("No assignment of the current field");
		}

		private TypeReference Import(Type type)
		{
			return Utilities.GetCorlibTypeReference(type, this.methodContext.get_Method().get_Module());
		}

		private IEnumerable<Instruction> IncludePrefixIfPresent(Instruction instruction, Code prefixCode)
		{
			if (instruction.get_Previous() == null || instruction.get_Previous().get_OpCode().get_Code() != prefixCode)
			{
				stackVariable3 = new Instruction[1];
				stackVariable3[0] = instruction;
				return stackVariable3;
			}
			stackVariable13 = new Instruction[2];
			stackVariable13[0] = instruction.get_Previous();
			stackVariable13[1] = instruction;
			return stackVariable13;
		}

		private bool IsIntegerType(TypeReference expressionType)
		{
			V_0 = expressionType.get_FullName();
			if (!String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_Byte().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_SByte().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int16().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt16().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int32().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt16().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int64().get_FullName()) && !String.op_Equality(V_0, this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt64().get_FullName()))
			{
				return false;
			}
			return true;
		}

		private bool IsPointerType(Expression expression)
		{
			if (!expression.get_HasType())
			{
				return false;
			}
			V_0 = expression.get_ExpressionType();
			if (V_0 == null)
			{
				return false;
			}
			if (V_0.get_IsPointer())
			{
				return true;
			}
			if (!V_0.get_IsPinned())
			{
				if (V_0.get_IsByReference())
				{
					return true;
				}
				return false;
			}
			V_1 = V_0 as PinnedType;
			if (V_1.get_ElementType().get_IsByReference())
			{
				return true;
			}
			return V_1.get_ElementType().get_IsPointer();
		}

		private bool IsUnmanagedPointerType(Expression ex)
		{
			if (ex == null)
			{
				return false;
			}
			if (!ex.get_HasType())
			{
				return false;
			}
			if (ex.get_ExpressionType().get_IsPointer())
			{
				return true;
			}
			return false;
		}

		public override void OnAdd(Instruction instruction)
		{
			this.PushBinaryExpression(1, instruction);
			return;
		}

		public override void OnAdd_Ovf(Instruction instruction)
		{
			this.PushBinaryExpression(1, instruction);
			return;
		}

		public override void OnAdd_Ovf_Un(Instruction instruction)
		{
			this.PushBinaryExpression(1, instruction);
			return;
		}

		public override void OnAnd(Instruction instruction)
		{
			this.PushBinaryExpression(22, instruction);
			return;
		}

		public override void OnArglist(Instruction instruction)
		{
			V_0 = this.Import(Type.GetTypeFromHandle(// 
			// Current member / type: System.Void Telerik.JustDecompiler.Decompiler.ExpressionDecompilerStep::OnArglist(Mono.Cecil.Cil.Instruction)
			// Exception in: System.Void OnArglist(Mono.Cecil.Cil.Instruction)
			// Specified method is not supported.
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com


		public override void OnBeq(Instruction instruction)
		{
			this.OnCeq(instruction);
			return;
		}

		public override void OnBge(Instruction instruction)
		{
			this.PushBinaryExpression(16, instruction);
			return;
		}

		public override void OnBge_Un(Instruction instruction)
		{
			this.PushBinaryExpression(16, instruction);
			return;
		}

		public override void OnBgt(Instruction instruction)
		{
			this.PushBinaryExpression(15, instruction);
			return;
		}

		public override void OnBgt_Un(Instruction instruction)
		{
			this.PushBinaryExpression(15, instruction);
			return;
		}

		public override void OnBle(Instruction instruction)
		{
			this.PushBinaryExpression(14, instruction);
			return;
		}

		public override void OnBle_Un(Instruction instruction)
		{
			this.PushBinaryExpression(14, instruction);
			return;
		}

		public override void OnBlt(Instruction instruction)
		{
			this.PushBinaryExpression(13, instruction);
			return;
		}

		public override void OnBlt_Un(Instruction instruction)
		{
			this.PushBinaryExpression(13, instruction);
			return;
		}

		public override void OnBne_Un(Instruction instruction)
		{
			this.PushBinaryExpression(10, instruction);
			return;
		}

		public override void OnBox(Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = (TypeReference)instruction.get_Operand();
			if (V_0.get_ExpressionType() == null || !String.op_Inequality(V_0.get_ExpressionType().GetFriendlyFullName(null), V_1.GetFriendlyFullName(null)))
			{
				if (V_0.get_ExpressionType() == null && V_0.get_CodeNodeType() == 26)
				{
					(V_0 as VariableReferenceExpression).get_Variable().set_VariableType(V_1);
				}
			}
			else
			{
				V_0 = new ExplicitCastExpression(V_0, V_1, null);
			}
			stackVariable14 = this.methodContext.get_Method().get_Module().get_TypeSystem();
			stackVariable17 = (TypeReference)instruction.get_Operand();
			stackVariable19 = new Instruction[1];
			stackVariable19[0] = instruction;
			V_0 = new BoxExpression(V_0, stackVariable14, stackVariable17, stackVariable19);
			this.Push(V_0);
			return;
		}

		public override void OnBr(Instruction instruction)
		{
			return;
		}

		public override void OnBreak(Instruction instruction)
		{
			return;
		}

		public override void OnBrfalse(Instruction instruction)
		{
			return;
		}

		public override void OnBrtrue(Instruction instruction)
		{
			return;
		}

		public override void OnCall(Instruction instruction)
		{
			V_0 = (MethodReference)instruction.get_Operand();
			V_2 = this.ProcessArguments(V_0);
			if (V_0.get_Name() == null || !String.op_Equality(V_0.get_Name(), ".ctor"))
			{
				if (V_0.get_HasThis())
				{
					stackVariable11 = this.Pop();
				}
				else
				{
					stackVariable11 = null;
				}
				V_10 = this.FixCallTarget(instruction, stackVariable11);
				V_11 = new MethodInvocationExpression(new MethodReferenceExpression(V_10, (MethodReference)instruction.get_Operand(), null), this.IncludePrefixIfPresent(instruction, 211));
				ExpressionDecompilerStep.AddRange<Expression>(V_11.get_Arguments(), V_2);
				if (!this.TryProcessRuntimeHelpersInitArray(V_11) && !this.TryProcessMultidimensionalIndexing(V_11))
				{
					V_11.set_VirtualCall(false);
					this.Push(V_11);
					if (Utilities.IsComputeStringHashMethod(instruction.get_Operand() as MethodReference))
					{
						this.context.get_MethodContext().get_SwitchByStringData().get_SwitchBlocksStartInstructions().Add(this.currentBlock.get_First().get_Offset());
					}
				}
				return;
			}
			V_3 = V_0.get_DeclaringType().GetElementType();
			if (this.methodContext.get_Method().get_DeclaringType().get_BaseType() != null)
			{
				stackVariable70 = this.methodContext.get_Method().get_DeclaringType().get_BaseType().GetElementType();
			}
			else
			{
				stackVariable70 = null;
			}
			V_4 = stackVariable70;
			V_5 = this.methodContext.get_Method().get_DeclaringType().GetElementType();
			if (!this.methodContext.get_Method().get_IsConstructor() || V_4 == null || !String.op_Equality(V_4.GetFriendlyFullName(null), V_3.GetFriendlyFullName(null)) && !String.op_Equality(V_5.GetFriendlyFullName(null), V_3.GetFriendlyFullName(null)) || instruction.get_Next().IsStoreRegister(out V_1))
			{
				V_9 = this.ProcessConstructor(instruction, V_2);
				this.PushAssignment(new UnaryExpression(8, this.Pop(), null), V_9, null);
				return;
			}
			if (V_0.get_HasThis())
			{
				stackVariable107 = this.Pop();
			}
			else
			{
				stackVariable107 = null;
			}
			V_6 = stackVariable107;
			V_6 = this.FixCallTarget(instruction, V_6);
			V_7 = new MethodReferenceExpression(V_6, (MethodReference)instruction.get_Operand(), null);
			if (!String.op_Equality(V_5.GetFriendlyFullName(null), V_3.GetFriendlyFullName(null)))
			{
				stackVariable130 = new BaseCtorExpression(V_7, this.IncludePrefixIfPresent(instruction, 211));
				stackVariable130.set_InstanceReference(V_6);
				V_8 = stackVariable130;
			}
			else
			{
				stackVariable142 = new ThisCtorExpression(V_7, this.IncludePrefixIfPresent(instruction, 211));
				stackVariable142.set_InstanceReference(V_6);
				V_8 = stackVariable142;
			}
			ExpressionDecompilerStep.AddRange<Expression>(V_8.get_Arguments(), V_2);
			this.Push(V_8);
			return;
		}

		public override void OnCalli(Instruction instruction)
		{
			if (!this.TryGetVariableValueAndMarkForRemoval(this.Pop() as VariableReferenceExpression, out V_0) || V_0.get_CodeNodeType() != 20)
			{
				throw new DecompilationException("Method pointer cannot be resolved to a method definition.");
			}
			V_1 = V_0 as MethodReferenceExpression;
			V_2 = this.ProcessArguments(V_1.get_Method());
			if (V_1.get_Method().get_HasThis())
			{
				V_1.set_Target(this.Pop());
			}
			stackVariable22 = new Instruction[1];
			stackVariable22[0] = instruction;
			V_3 = new MethodInvocationExpression(V_1, stackVariable22);
			V_3.set_Arguments(V_2);
			this.Push(V_3);
			return;
		}

		public override void OnCallvirt(Instruction instruction)
		{
			this.OnCall(instruction);
			V_0 = this.Peek();
			if (V_0 != null && V_0.get_CodeNodeType() == 19)
			{
				V_1 = V_0 as MethodInvocationExpression;
				V_1.set_VirtualCall(true);
				if (instruction.get_Previous() != null && instruction.get_Previous().get_OpCode().get_Code() == 211 && instruction.get_Previous().get_Operand() != null)
				{
					V_1.set_ConstraintType(instruction.get_Previous().get_Operand() as TypeReference);
				}
			}
			return;
		}

		public override void OnCastclass(Instruction instruction)
		{
			this.PushCastExpression((TypeReference)instruction.get_Operand(), instruction);
			return;
		}

		public override void OnCeq(Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = this.Pop();
			stackVariable8 = this.currentTypeSystem;
			stackVariable10 = new Instruction[1];
			stackVariable10[0] = instruction;
			V_2 = new BinaryExpression(9, V_1, V_0, stackVariable8, stackVariable10, false);
			V_2.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Boolean());
			this.Push(V_2);
			return;
		}

		public override void OnCgt(Instruction instruction)
		{
			this.PushBinaryExpression(15, instruction);
			return;
		}

		public override void OnCgt_Un(Instruction instruction)
		{
			this.PushBinaryExpression(15, instruction);
			return;
		}

		public override void OnClt(Instruction instruction)
		{
			this.PushBinaryExpression(13, instruction);
			return;
		}

		public override void OnClt_Un(Instruction instruction)
		{
			this.PushBinaryExpression(13, instruction);
			return;
		}

		public override void OnConstrained(Instruction instruction)
		{
			return;
		}

		public override void OnConv_I(Instruction instruction)
		{
			V_0 = this.expressionStack.Peek();
			if (this.IsPointerType(V_0) || V_0.get_CodeNodeType() == 23 && (V_0 as UnaryExpression).get_Operator() == 9)
			{
				this.Push(this.Pop());
				return;
			}
			V_1 = this.Pop();
			if (V_1.get_HasType() && String.op_Equality(V_1.get_ExpressionType().get_FullName(), this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int32().get_FullName()) || String.op_Equality(V_1.get_ExpressionType().get_FullName(), this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt32().get_FullName()))
			{
				this.Push(V_1);
				return;
			}
			if (!V_1.get_HasType() || !this.IsIntegerType(V_1.get_ExpressionType()))
			{
				stackVariable21 = new Instruction[1];
				stackVariable21[0] = instruction;
				this.Push(new UnaryExpression(9, V_1, stackVariable21));
				return;
			}
			stackVariable38 = new PointerType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Void());
			stackVariable40 = new Instruction[1];
			stackVariable40[0] = instruction;
			this.Push(new ExplicitCastExpression(V_1, stackVariable38, stackVariable40));
			return;
		}

		public override void OnConv_I1(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_SByte(), instruction);
			return;
		}

		public override void OnConv_I2(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int16(), instruction);
			return;
		}

		public override void OnConv_I4(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int32(), instruction);
			return;
		}

		public override void OnConv_I8(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int64(), instruction);
			return;
		}

		public override void OnConv_Ovf_I(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_IntPtr(), instruction);
			return;
		}

		public override void OnConv_Ovf_I_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_IntPtr(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_I1(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_SByte(), instruction);
			return;
		}

		public override void OnConv_Ovf_I1_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_SByte(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_I2(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int16(), instruction);
			return;
		}

		public override void OnConv_Ovf_I2_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int16(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_I4(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int32(), instruction);
			return;
		}

		public override void OnConv_Ovf_I4_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int32(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_I8(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int64(), instruction);
			return;
		}

		public override void OnConv_Ovf_I8_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int64(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_U(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UIntPtr(), instruction);
			return;
		}

		public override void OnConv_Ovf_U_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UIntPtr(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_U1(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Byte(), instruction);
			return;
		}

		public override void OnConv_Ovf_U1_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Byte(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_U2(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt16(), instruction);
			return;
		}

		public override void OnConv_Ovf_U2_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt16(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_U4(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt32(), instruction);
			return;
		}

		public override void OnConv_Ovf_U4_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt32(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_Ovf_U8(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt64(), instruction);
			return;
		}

		public override void OnConv_Ovf_U8_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt64(), instruction);
			this.PushCastExpression(new PointerType(this.currentTypeSystem.get_Void()), null);
			return;
		}

		public override void OnConv_R_Un(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Single(), instruction);
			return;
		}

		public override void OnConv_R4(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Single(), instruction);
			return;
		}

		public override void OnConv_R8(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Double(), instruction);
			return;
		}

		public override void OnConv_U(Instruction instruction)
		{
			this.OnConv_I(instruction);
			return;
		}

		public override void OnConv_U1(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Byte(), instruction);
			return;
		}

		public override void OnConv_U2(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt16(), instruction);
			return;
		}

		public override void OnConv_U4(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt32(), instruction);
			return;
		}

		public override void OnConv_U8(Instruction instruction)
		{
			this.PushCastExpression(this.methodContext.get_Method().get_Module().get_TypeSystem().get_UInt64(), instruction);
			return;
		}

		public override void OnCpobj(Instruction instruction)
		{
			this.OnLdobj(null);
			this.OnStobj(instruction);
			return;
		}

		public override void OnDiv(Instruction instruction)
		{
			this.PushBinaryExpression(7, instruction);
			return;
		}

		public override void OnDiv_Un(Instruction instruction)
		{
			this.PushBinaryExpression(7, instruction);
			return;
		}

		public override void OnDup(Instruction instruction)
		{
			return;
		}

		public override void OnEndfilter(Instruction instruction)
		{
			return;
		}

		public override void OnEndfinally(Instruction instruction)
		{
			return;
		}

		public override void OnInitblk(Instruction instruction)
		{
			return;
		}

		public override void OnInitobj(Instruction instruction)
		{
			V_0 = new UnaryExpression(8, this.Pop(), null);
			V_1 = (TypeReference)instruction.get_Operand();
			V_2 = new ObjectCreationExpression(null, V_1, null, null);
			if (!V_1.get_IsGenericInstance())
			{
				if (V_1.get_IsGenericParameter())
				{
					V_2 = new DefaultObjectExpression((TypeReference)instruction.get_Operand(), null);
				}
			}
			else
			{
				if (V_1.GetFriendlyFullName(null).IndexOf("System.Nullable<") >= 0)
				{
					V_2 = new LiteralExpression(null, this.currentTypeSystem, null);
				}
			}
			stackVariable21 = new Instruction[1];
			stackVariable21[0] = instruction;
			this.PushAssignment(V_0, V_2, stackVariable21);
			return;
		}

		public override void OnIsinst(Instruction instruction)
		{
			stackVariable2 = this.Pop();
			stackVariable5 = (TypeReference)instruction.get_Operand();
			stackVariable7 = new Instruction[1];
			stackVariable7[0] = instruction;
			this.Push(new SafeCastExpression(stackVariable2, stackVariable5, stackVariable7));
			return;
		}

		public override void OnLdarg(Instruction instruction)
		{
			V_0 = (ParameterDefinition)instruction.get_Operand();
			if ((object)V_0 != (object)this.methodContext.get_Method().get_Body().get_ThisParameter())
			{
				this.PushArgumentReference(V_0);
				return;
			}
			stackVariable15 = this.methodContext.get_Method().get_DeclaringType();
			stackVariable17 = new Instruction[1];
			stackVariable17[0] = instruction;
			this.Push(new ThisReferenceExpression(stackVariable15, stackVariable17));
			return;
		}

		public override void OnLdarg_0(Instruction instruction)
		{
			this.PushArgumentReference(0, instruction);
			return;
		}

		public override void OnLdarg_1(Instruction instruction)
		{
			this.PushArgumentReference(1, instruction);
			return;
		}

		public override void OnLdarg_2(Instruction instruction)
		{
			this.PushArgumentReference(2, instruction);
			return;
		}

		public override void OnLdarg_3(Instruction instruction)
		{
			this.PushArgumentReference(3, instruction);
			return;
		}

		public override void OnLdarga(Instruction instruction)
		{
			stackVariable2 = (ParameterDefinition)instruction.get_Operand();
			stackVariable4 = new Instruction[1];
			stackVariable4[0] = instruction;
			V_0 = new ArgumentReferenceExpression(stackVariable2, stackVariable4);
			this.Push(new UnaryExpression(7, V_0, null));
			return;
		}

		public override void OnLdc_I4(Instruction instruction)
		{
			this.PushLiteral(Convert.ToInt32(instruction.get_Operand()), instruction);
			return;
		}

		public override void OnLdc_I4_0(Instruction instruction)
		{
			this.PushLiteral(0, instruction);
			return;
		}

		public override void OnLdc_I4_1(Instruction instruction)
		{
			this.PushLiteral(1, instruction);
			return;
		}

		public override void OnLdc_I4_2(Instruction instruction)
		{
			this.PushLiteral(2, instruction);
			return;
		}

		public override void OnLdc_I4_3(Instruction instruction)
		{
			this.PushLiteral(3, instruction);
			return;
		}

		public override void OnLdc_I4_4(Instruction instruction)
		{
			this.PushLiteral(4, instruction);
			return;
		}

		public override void OnLdc_I4_5(Instruction instruction)
		{
			this.PushLiteral(5, instruction);
			return;
		}

		public override void OnLdc_I4_6(Instruction instruction)
		{
			this.PushLiteral(6, instruction);
			return;
		}

		public override void OnLdc_I4_7(Instruction instruction)
		{
			this.PushLiteral(7, instruction);
			return;
		}

		public override void OnLdc_I4_8(Instruction instruction)
		{
			this.PushLiteral(8, instruction);
			return;
		}

		public override void OnLdc_I4_M1(Instruction instruction)
		{
			this.PushLiteral(-1, instruction);
			return;
		}

		public override void OnLdc_I8(Instruction instruction)
		{
			this.PushLiteral(Convert.ToInt64(instruction.get_Operand()), instruction);
			return;
		}

		public override void OnLdc_R4(Instruction instruction)
		{
			this.PushLiteral(Convert.ToSingle(instruction.get_Operand()), instruction);
			return;
		}

		public override void OnLdc_R8(Instruction instruction)
		{
			this.PushLiteral(Convert.ToDouble(instruction.get_Operand()), instruction);
			return;
		}

		public override void OnLdelem_Any(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_I(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_I1(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_I2(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_I4(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_I8(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_R4(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_R8(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_Ref(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_U1(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_U2(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelem_U4(Instruction instruction)
		{
			this.PushArrayIndexer(false, instruction);
			return;
		}

		public override void OnLdelema(Instruction instruction)
		{
			this.PushArrayIndexer(true, instruction);
			return;
		}

		public override void OnLdfld(Instruction instruction)
		{
			this.PushFieldReference(instruction, this.Pop());
			return;
		}

		public override void OnLdflda(Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = 7;
			if (!this.IsUnmanagedPointerType(V_0))
			{
				V_1 = 7;
			}
			else
			{
				V_1 = 9;
			}
			this.PushFieldReference(instruction, V_0);
			this.Push(new UnaryExpression(V_1, this.Pop(), null));
			return;
		}

		public override void OnLdftn(Instruction instruction)
		{
			stackVariable4 = (MethodReference)instruction.get_Operand();
			stackVariable6 = new Instruction[1];
			stackVariable6[0] = instruction;
			this.Push(new MethodReferenceExpression(null, stackVariable4, stackVariable6));
			return;
		}

		private void OnLdind(TypeReference type, Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = new UnaryExpression(8, V_0, this.IncludePrefixIfPresent(instruction, 208));
			if (V_1.get_HasType() && !String.op_Inequality(V_1.get_ExpressionType().get_FullName(), "System.Boolean") || !String.op_Inequality(V_1.get_ExpressionType().get_FullName(), type.get_FullName()) || !String.op_Inequality(type.get_FullName(), "System.Object"))
			{
				this.Push(V_1);
				return;
			}
			this.Push(new ExplicitCastExpression(V_1, type, null));
			return;
		}

		public override void OnLdind_I(Instruction instruction)
		{
			this.OnLdind(new PointerType(this.currentTypeSystem.get_Void()), instruction);
			return;
		}

		public override void OnLdind_I1(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_SByte(), instruction);
			return;
		}

		public override void OnLdind_I2(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_Int16(), instruction);
			return;
		}

		public override void OnLdind_I4(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_Int32(), instruction);
			return;
		}

		public override void OnLdind_I8(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_Int64(), instruction);
			return;
		}

		public override void OnLdind_R4(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_Single(), instruction);
			return;
		}

		public override void OnLdind_R8(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_Double(), instruction);
			return;
		}

		public override void OnLdind_Ref(Instruction instruction)
		{
			this.OnLdind(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Object(), instruction);
			return;
		}

		public override void OnLdind_U1(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_Byte(), instruction);
			return;
		}

		public override void OnLdind_U2(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_UInt16(), instruction);
			return;
		}

		public override void OnLdind_U4(Instruction instruction)
		{
			this.OnLdind(this.currentTypeSystem.get_UInt32(), instruction);
			return;
		}

		public override void OnLdlen(Instruction instruction)
		{
			stackVariable2 = this.Pop();
			stackVariable7 = this.methodContext.get_Method().get_Module().get_TypeSystem();
			stackVariable9 = new Instruction[1];
			stackVariable9[0] = instruction;
			this.Push(new ArrayLengthExpression(stackVariable2, stackVariable7, stackVariable9));
			return;
		}

		public override void OnLdloc(Instruction instruction)
		{
			this.PushVariableReference((VariableReference)instruction.get_Operand(), instruction);
			return;
		}

		public override void OnLdloc_0(Instruction instruction)
		{
			this.PushVariableReference(0, instruction);
			return;
		}

		public override void OnLdloc_1(Instruction instruction)
		{
			this.PushVariableReference(1, instruction);
			return;
		}

		public override void OnLdloc_2(Instruction instruction)
		{
			this.PushVariableReference(2, instruction);
			return;
		}

		public override void OnLdloc_3(Instruction instruction)
		{
			this.PushVariableReference(3, instruction);
			return;
		}

		public override void OnLdloca(Instruction instruction)
		{
			stackVariable2 = instruction.get_Operand() as VariableReference;
			stackVariable4 = new Instruction[1];
			stackVariable4[0] = instruction;
			V_0 = new VariableReferenceExpression(stackVariable2, stackVariable4);
			this.Push(new UnaryExpression(7, V_0, null));
			return;
		}

		public override void OnLdnull(Instruction instruction)
		{
			this.PushLiteral(null, instruction);
			return;
		}

		public override void OnLdobj(Instruction instruction)
		{
			stackVariable3 = this.Pop();
			if (instruction != null)
			{
				stackVariable8 = this.IncludePrefixIfPresent(instruction, 208);
			}
			else
			{
				stackVariable8 = null;
			}
			this.Push(new UnaryExpression(8, stackVariable3, stackVariable8));
			return;
		}

		public override void OnLdsfld(Instruction instruction)
		{
			this.PushFieldReference(instruction);
			return;
		}

		public override void OnLdsflda(Instruction instruction)
		{
			this.PushFieldReference(instruction);
			this.Push(new UnaryExpression(7, this.Pop(), null));
			return;
		}

		public override void OnLdstr(Instruction instruction)
		{
			this.PushLiteral(instruction.get_Operand(), instruction);
			return;
		}

		public override void OnLdtoken(Instruction instruction)
		{
			V_0 = instruction.get_Operand() as MemberReference;
			if (V_0 == null)
			{
				throw new NotSupportedException();
			}
			stackVariable7 = new Instruction[1];
			stackVariable7[0] = instruction;
			this.Push(new MemberHandleExpression(V_0, stackVariable7));
			return;
		}

		public override void OnLdvirtftn(Instruction instruction)
		{
			V_0 = this.Pop();
			if (this.IsPointerType(V_0))
			{
				stackVariable18 = new Instruction[1];
				stackVariable18[0] = instruction;
				V_0 = new UnaryExpression(8, V_0, stackVariable18);
			}
			stackVariable9 = (MethodReference)instruction.get_Operand();
			stackVariable11 = new Instruction[1];
			stackVariable11[0] = instruction;
			this.Push(new MethodReferenceExpression(V_0, stackVariable9, stackVariable11));
			return;
		}

		public override void OnLeave(Instruction instruction)
		{
			return;
		}

		public override void OnLocalloc(Instruction instruction)
		{
			stackVariable2 = this.Pop();
			stackVariable9 = new PointerType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_IntPtr());
			stackVariable11 = new Instruction[1];
			stackVariable11[0] = instruction;
			this.Push(new StackAllocExpression(stackVariable2, stackVariable9, stackVariable11));
			return;
		}

		public override void OnMkrefany(Instruction instruction)
		{
			stackVariable2 = this.Pop();
			stackVariable5 = (TypeReference)instruction.get_Operand();
			stackVariable7 = new Instruction[1];
			stackVariable7[0] = instruction;
			this.Push(new MakeRefExpression(stackVariable2, stackVariable5, stackVariable7));
			return;
		}

		public override void OnMul(Instruction instruction)
		{
			this.PushBinaryExpression(5, instruction);
			return;
		}

		public override void OnMul_Ovf(Instruction instruction)
		{
			this.PushBinaryExpression(5, instruction);
			return;
		}

		public override void OnMul_Ovf_Un(Instruction instruction)
		{
			this.PushBinaryExpression(5, instruction);
			return;
		}

		public override void OnNeg(Instruction instruction)
		{
			stackVariable3 = this.Pop();
			stackVariable5 = new Instruction[1];
			stackVariable5[0] = instruction;
			this.Push(new UnaryExpression(0, stackVariable3, stackVariable5));
			return;
		}

		public override void OnNewarr(Instruction instruction)
		{
			stackVariable2 = (TypeReference)instruction.get_Operand();
			stackVariable6 = new InitializerExpression(new BlockExpression(null), 2);
			stackVariable8 = new Instruction[1];
			stackVariable8[0] = instruction;
			V_0 = new ArrayCreationExpression(stackVariable2, stackVariable6, stackVariable8);
			V_0.get_Dimensions().Add(this.Pop());
			this.Push(V_0);
			return;
		}

		public override void OnNewobj(Instruction instruction)
		{
			V_0 = (MethodReference)instruction.get_Operand();
			V_1 = this.ProcessConstructor(instruction, this.ProcessArguments(V_0));
			V_2 = this.ConvertObjectToArray(V_1);
			if (V_2 == null)
			{
				V_2 = V_1;
			}
			this.Push(V_2);
			return;
		}

		public override void OnNop(Instruction instruction)
		{
			return;
		}

		public override void OnNot(Instruction instruction)
		{
			stackVariable2 = this.Pop();
			stackVariable4 = new Instruction[1];
			stackVariable4[0] = instruction;
			this.Push(new UnaryExpression(2, stackVariable2, stackVariable4));
			return;
		}

		public override void OnOr(Instruction instruction)
		{
			this.PushBinaryExpression(21, instruction);
			return;
		}

		public override void OnPop(Instruction instruction)
		{
			V_0 = this.Pop();
			stackVariable6 = String.Concat("dummyVar", this.dummyVarCounter.ToString());
			this.dummyVarCounter = this.dummyVarCounter + 1;
			V_1 = null;
			if (V_0.get_HasType())
			{
				V_1 = V_0.get_ExpressionType();
			}
			V_2 = new VariableDefinition(stackVariable6, V_1, this.methodContext.get_Method());
			V_3 = new StackVariableDefineUseInfo();
			dummyVar0 = V_3.get_DefinedAt().Add(instruction.get_Offset());
			this.methodContext.get_StackData().get_VariableToDefineUseInfo().Add(V_2, V_3);
			this.methodContext.get_StackData().get_InstructionOffsetToAssignedVariableMap().Add(instruction.get_Offset(), V_2);
			dummyVar1 = this.methodContext.get_VariablesToRename().Add(V_2);
			stackVariable46 = new Instruction[1];
			stackVariable46[0] = instruction;
			V_4 = new VariableReferenceExpression(V_2, stackVariable46);
			V_5 = new BinaryExpression(26, V_4, V_0, this.currentTypeSystem, null, false);
			this.Push(V_5);
			return;
		}

		public override void OnRefanytype(Instruction instruction)
		{
			this.PushCastExpression(this.Import(Type.GetTypeFromHandle(// 
			// Current member / type: System.Void Telerik.JustDecompiler.Decompiler.ExpressionDecompilerStep::OnRefanytype(Mono.Cecil.Cil.Instruction)
			// Exception in: System.Void OnRefanytype(Mono.Cecil.Cil.Instruction)
			// Specified method is not supported.
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com


		public override void OnRem(Instruction instruction)
		{
			this.PushBinaryExpression(24, instruction);
			return;
		}

		public override void OnRem_Un(Instruction instruction)
		{
			this.PushBinaryExpression(24, instruction);
			return;
		}

		public override void OnRet(Instruction instruction)
		{
			if (this.expressionStack.get_Count() != 0)
			{
				if (!this.methodContext.get_Method().get_ReturnType().get_IsByReference())
				{
					stackVariable9 = this.Pop();
					stackVariable11 = new Instruction[1];
					stackVariable11[0] = instruction;
					V_0 = new ReturnExpression(stackVariable9, stackVariable11);
				}
				else
				{
					stackVariable18 = this.Pop();
					stackVariable20 = new Instruction[1];
					stackVariable20[0] = instruction;
					V_0 = new RefReturnExpression(stackVariable18, stackVariable20);
				}
			}
			else
			{
				stackVariable26 = new Instruction[1];
				stackVariable26[0] = instruction;
				V_0 = new ReturnExpression(null, stackVariable26);
			}
			this.Push(V_0);
			return;
		}

		public override void OnRethrow(Instruction instruction)
		{
			stackVariable3 = new Instruction[1];
			stackVariable3[0] = instruction;
			this.Push(new ThrowExpression(null, stackVariable3));
			return;
		}

		public override void OnShl(Instruction instruction)
		{
			this.PushBinaryExpression(17, instruction);
			return;
		}

		public override void OnShr(Instruction instruction)
		{
			this.PushBinaryExpression(19, instruction);
			return;
		}

		public override void OnShr_Un(Instruction instruction)
		{
			this.PushBinaryExpression(19, instruction);
			return;
		}

		public override void OnSizeof(Instruction instruction)
		{
			stackVariable3 = (TypeReference)instruction.get_Operand();
			stackVariable5 = new Instruction[1];
			stackVariable5[0] = instruction;
			this.Push(new SizeOfExpression(stackVariable3, stackVariable5));
			return;
		}

		public override void OnStarg(Instruction instruction)
		{
			this.PushArgumentReference((ParameterReference)instruction.get_Operand());
			this.PushAssignment(this.Pop(), this.Pop(), null);
			return;
		}

		public override void OnStelem_Any(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_I(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_I1(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_I2(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_I4(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_I8(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_R4(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_R8(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStelem_Ref(Instruction instruction)
		{
			this.PushArrayStore(instruction);
			return;
		}

		public override void OnStfld(Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = this.Pop();
			if (this.IsPointerType(V_1))
			{
				V_1 = new UnaryExpression(8, V_1, null);
			}
			stackVariable16 = new FieldReferenceExpression(V_1, (FieldReference)instruction.get_Operand(), this.IncludePrefixIfPresent(instruction, 208));
			stackVariable16.set_IsSimpleStore(true);
			this.PushAssignment(stackVariable16, V_0, null);
			return;
		}

		public override void OnStind_I(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_I1(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_I2(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_I4(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_I8(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_R4(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_R8(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStind_Ref(Instruction instruction)
		{
			this.OnStobj(instruction);
			return;
		}

		public override void OnStloc(Instruction instruction)
		{
			this.PushVariableAssignement((VariableReference)instruction.get_Operand(), instruction);
			return;
		}

		private void OnStloc(int index, Instruction instruction)
		{
			this.PushVariableAssignement(index, instruction);
			return;
		}

		public override void OnStloc_0(Instruction instruction)
		{
			this.OnStloc(0, instruction);
			return;
		}

		public override void OnStloc_1(Instruction instruction)
		{
			this.OnStloc(1, instruction);
			return;
		}

		public override void OnStloc_2(Instruction instruction)
		{
			this.OnStloc(2, instruction);
			return;
		}

		public override void OnStloc_3(Instruction instruction)
		{
			this.OnStloc(3, instruction);
			return;
		}

		public override void OnStobj(Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = new UnaryExpression(8, this.Pop(), null);
			this.PushAssignment(V_1, V_0, this.IncludePrefixIfPresent(instruction, 208));
			return;
		}

		public override void OnStsfld(Instruction instruction)
		{
			stackVariable9 = new FieldReferenceExpression(null, (FieldReference)instruction.get_Operand(), this.IncludePrefixIfPresent(instruction, 208));
			stackVariable9.set_IsSimpleStore(true);
			this.PushAssignment(stackVariable9, this.Pop(), null);
			return;
		}

		public override void OnSub(Instruction instruction)
		{
			this.PushBinaryExpression(3, instruction);
			return;
		}

		public override void OnSub_Ovf(Instruction instruction)
		{
			this.PushBinaryExpression(3, instruction);
			return;
		}

		public override void OnSub_Ovf_Un(Instruction instruction)
		{
			this.PushBinaryExpression(3, instruction);
			return;
		}

		public override void OnSwitch(Instruction instruction)
		{
			return;
		}

		public override void OnThrow(Instruction instruction)
		{
			stackVariable1 = this.Pop();
			stackVariable3 = new Instruction[1];
			stackVariable3[0] = instruction;
			this.Push(new ThrowExpression(stackVariable1, stackVariable3));
			return;
		}

		public override void OnUnbox(Instruction instruction)
		{
			this.PushCastExpression(new ByReferenceType((TypeReference)instruction.get_Operand()), instruction);
			return;
		}

		public override void OnUnbox_Any(Instruction instruction)
		{
			this.PushCastExpression((TypeReference)instruction.get_Operand(), instruction);
			return;
		}

		public override void OnVolatile(Instruction instruction)
		{
			return;
		}

		public override void OnXor(Instruction instruction)
		{
			this.PushBinaryExpression(23, instruction);
			return;
		}

		private Expression Peek()
		{
			return this.expressionStack.Peek();
		}

		private Expression Pop()
		{
			V_0 = this.expressionStack.Pop();
			dummyVar0 = this.used.Add(V_0);
			if (V_0 as VariableReferenceExpression != null)
			{
				V_1 = (V_0 as VariableReferenceExpression).get_Variable();
				if (this.exceptionVariables.ContainsKey(V_1))
				{
					V_2 = this.exceptionVariables.get_Item(V_1);
					this.exceptionVariables.set_Item(V_1, new KeyValuePair<int, bool>(V_2.get_Key(), true));
				}
			}
			if (V_0 as VariableDeclarationExpression != null)
			{
				V_3 = (V_0 as VariableDeclarationExpression).get_Variable();
				if (this.exceptionVariables.ContainsKey(V_3))
				{
					V_4 = this.exceptionVariables.get_Item(V_3);
					this.exceptionVariables.set_Item(V_3, new KeyValuePair<int, bool>(V_4.get_Key(), true));
				}
			}
			return V_0;
		}

		public BlockStatement Process(DecompilationContext theContext, BlockStatement body)
		{
			this.context = theContext;
			this.methodContext = theContext.get_MethodContext();
			this.currentTypeSystem = this.methodContext.get_Method().get_Module().get_TypeSystem();
			this.typeContext = theContext.get_TypeContext();
			this.CreateExpressions();
			this.methodContext.set_Expressions(this.results);
			(new StackVariablesInliner(this.methodContext, this.offsetToExpression, this.context.get_Language().get_VariablesToNotInlineFinder())).InlineVariables();
			this.AddUninlinedStackVariablesToContext();
			(new TypeInferer(theContext, this.offsetToExpression)).InferTypes();
			dummyVar0 = (new FixBinaryExpressionsStep(this.methodContext.get_Method().get_Module().get_TypeSystem())).Process(theContext, body);
			(new MethodVariablesInliner(this.methodContext, this.context.get_Language().get_VariablesToNotInlineFinder())).InlineVariables();
			(new UsageBasedExpressionFixer(this.methodContext)).FixLiterals();
			dummyVar1 = (new FindAutoBoxesStep()).Process(theContext, body);
			return body;
		}

		private ExpressionCollection ProcessArguments(MethodReference method)
		{
			V_0 = new ExpressionCollection();
			V_1 = method.get_Parameters().get_Count() - 1;
			while (V_1 >= 0)
			{
				V_0.Insert(0, this.Pop());
				V_1 = V_1 - 1;
			}
			return V_0;
		}

		private ObjectCreationExpression ProcessConstructor(Instruction instruction, ExpressionCollection arguments)
		{
			if (instruction.get_OpCode().get_Code() == 110)
			{
				stackVariable8 = this.IncludePrefixIfPresent(instruction, 211);
			}
			else
			{
				stackVariable22 = new Instruction[1];
				stackVariable22[0] = instruction;
				stackVariable8 = stackVariable22;
			}
			V_0 = stackVariable8;
			V_1 = (MethodReference)instruction.get_Operand();
			stackVariable12 = V_1;
			if (V_1 != null)
			{
				stackVariable15 = V_1.get_DeclaringType();
			}
			else
			{
				stackVariable15 = null;
			}
			stackVariable18 = new ObjectCreationExpression(stackVariable12, stackVariable15, null, V_0);
			ExpressionDecompilerStep.AddRange<Expression>(stackVariable18.get_Arguments(), arguments);
			return stackVariable18;
		}

		private void ProcessYieldBlock(InstructionBlock theBlock, List<Expression> blockExpressions)
		{
			V_0 = blockExpressions.get_Count();
			if (this.methodContext.get_YieldData().get_YieldBreaks().Contains(theBlock))
			{
				if (blockExpressions.get_Item(V_0 - 1).get_CodeNodeType() != 57)
				{
					throw new Exception("No return at the end of yield break block");
				}
				V_1 = (blockExpressions.get_Item(V_0 - 1) as ReturnExpression).get_UnderlyingSameMethodInstructions().Last<Instruction>();
				stackVariable63 = new Instruction[1];
				stackVariable63[0] = V_1;
				blockExpressions.set_Item(V_0 - 1, new YieldBreakExpression(stackVariable63));
				return;
			}
			if (this.methodContext.get_YieldData().get_YieldReturns().Contains(theBlock))
			{
				V_2 = this.GetYieldReturnItem(blockExpressions);
				V_3 = this.GetYieldReturnAssignmentIndex(blockExpressions);
				blockExpressions.set_Item(V_3, new YieldReturnExpression(V_2, blockExpressions.get_Item(V_3).get_UnderlyingSameMethodInstructions()));
				V_0 = blockExpressions.get_Count();
				if (blockExpressions.get_Item(V_0 - 1).get_CodeNodeType() == 57)
				{
					blockExpressions.RemoveAt(V_0 - 1);
				}
			}
			return;
		}

		public void Push(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			this.expressionStack.Push(expression);
			return;
		}

		private void PushArgumentReference(ParameterReference parameter)
		{
			stackVariable3 = new Instruction[1];
			stackVariable3[0] = this.get_CurrentInstruction();
			this.Push(new ArgumentReferenceExpression(parameter, stackVariable3));
			return;
		}

		private void PushArgumentReference(int index, Instruction instruction)
		{
			if (this.methodContext.get_Method().get_HasThis())
			{
				if (index == 0)
				{
					stackVariable25 = this.methodContext.get_Method().get_DeclaringType();
					stackVariable27 = new Instruction[1];
					stackVariable27[0] = instruction;
					this.Push(new ThisReferenceExpression(stackVariable25, stackVariable27));
					return;
				}
				index = index - 1;
			}
			stackVariable10 = this.methodContext.get_Method().get_Parameters().get_Item(index);
			stackVariable12 = new Instruction[1];
			stackVariable12[0] = this.get_CurrentInstruction();
			this.Push(new ArgumentReferenceExpression(stackVariable10, stackVariable12));
			return;
		}

		private void PushArrayIndexer(bool isAddress, Instruction instruction)
		{
			V_0 = this.Pop();
			stackVariable3 = this.Pop();
			stackVariable5 = new Instruction[1];
			stackVariable5[0] = instruction;
			V_1 = new ArrayIndexerExpression(stackVariable3, stackVariable5);
			V_1.get_Indices().Add(V_0);
			if (!isAddress)
			{
				this.Push(V_1);
				return;
			}
			this.Push(new UnaryExpression(7, V_1, null));
			return;
		}

		private void PushArrayStore(Instruction instruction)
		{
			V_0 = this.Pop();
			this.PushArrayIndexer(false, instruction);
			V_1 = this.Pop() as ArrayIndexerExpression;
			V_1.set_IsSimpleStore(true);
			V_2 = new BinaryExpression(26, V_1, V_0, this.currentTypeSystem, null, false);
			this.Push(V_2);
			return;
		}

		private void PushAssignment(Expression left, Expression right, IEnumerable<Instruction> instructions = null)
		{
			V_0 = new BinaryExpression(26, left, right, this.currentTypeSystem, instructions, false);
			this.Push(V_0);
			return;
		}

		public void PushBinaryExpression(BinaryOperator op, Instruction instruction)
		{
			V_0 = this.Pop();
			V_1 = this.Pop();
			stackVariable8 = this.currentTypeSystem;
			stackVariable10 = new Instruction[1];
			stackVariable10[0] = instruction;
			V_2 = new BinaryExpression(op, V_1, V_0, stackVariable8, stackVariable10, false);
			if (V_2.get_IsComparisonExpression() || V_2.get_IsLogicalExpression())
			{
				V_2.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Boolean());
			}
			this.Push(V_2);
			return;
		}

		private void PushCastExpression(TypeReference targetType, Instruction instruction)
		{
			// 
			// Current member / type: System.Void Telerik.JustDecompiler.Decompiler.ExpressionDecompilerStep::PushCastExpression(Mono.Cecil.TypeReference,Mono.Cecil.Cil.Instruction)
			// Exception in: System.Void PushCastExpression(Mono.Cecil.TypeReference,Mono.Cecil.Cil.Instruction)
			// Specified argument was out of the range of valid values. (Parameter 'Target of array indexer expression is not an array.')
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		private void PushFieldReference(Instruction instruction)
		{
			this.PushFieldReference(instruction, null);
			return;
		}

		private void PushFieldReference(Instruction instruction, Expression target)
		{
			V_1 = (instruction.get_Operand() as FieldReference).Resolve();
			if (V_1 != null && (object)V_1.get_DeclaringType() == (object)this.methodContext.get_Method().get_DeclaringType() && this.methodContext.get_EnableEventAnalysis() && this.typeContext.GetFieldToEventMap(this.context.get_Language()).TryGetValue(V_1, out V_0))
			{
				this.Push(new EventReferenceExpression(target, V_0, this.IncludePrefixIfPresent(instruction, 208)));
				return;
			}
			if (target != null && this.IsPointerType(target))
			{
				target = new UnaryExpression(8, target, null);
			}
			this.Push(new FieldReferenceExpression(target, (FieldReference)instruction.get_Operand(), this.IncludePrefixIfPresent(instruction, 208)));
			return;
		}

		private void PushLiteral(object value, Instruction instruction)
		{
			stackVariable2 = this.currentTypeSystem;
			stackVariable4 = new Instruction[1];
			stackVariable4[0] = instruction;
			V_0 = new LiteralExpression(value, stackVariable2, stackVariable4);
			if (V_0.get_Value() != null)
			{
				if (V_0.get_Value() as Int32 == 0)
				{
					if (V_0.get_Value() as String == null)
					{
						if (V_0.get_Value() as Single == 0)
						{
							if (V_0.get_Value() as Double == 0)
							{
								if (V_0.get_Value() as Int64 != 0)
								{
									V_0.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int64());
								}
							}
							else
							{
								V_0.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Double());
							}
						}
						else
						{
							V_0.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Single());
						}
					}
					else
					{
						V_0.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_String());
					}
				}
				else
				{
					V_0.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Int32());
				}
			}
			else
			{
				V_0.set_ExpressionType(this.methodContext.get_Method().get_Module().get_TypeSystem().get_Object());
			}
			this.Push(V_0);
			return;
		}

		private void PushVariableAssignement(VariableReference variable, Instruction instruction)
		{
			this.PushVariableAssignement(variable.get_Index(), instruction);
			return;
		}

		private void PushVariableAssignement(int index, Instruction instruction)
		{
			stackVariable7 = this.methodContext.get_Method().get_Body().get_Variables().get_Item(index).Resolve();
			stackVariable9 = new Instruction[1];
			stackVariable9[0] = instruction;
			V_0 = new VariableReferenceExpression(stackVariable7, stackVariable9);
			this.PushAssignment(V_0, this.Pop(), null);
			return;
		}

		private void PushVariableReference(int index, Instruction instruction)
		{
			this.PushVariableReference(this.methodContext.get_Method().get_Body().get_Variables().get_Item(index).Resolve(), instruction);
			return;
		}

		private void PushVariableReference(VariableReference variable, Instruction instruction)
		{
			stackVariable3 = new Instruction[1];
			stackVariable3[0] = instruction;
			this.Push(new VariableReferenceExpression(variable, stackVariable3));
			return;
		}

		private void SetInitialStack(Instruction instruction)
		{
			if (!this.methodContext.get_StackData().get_InstructionOffsetToUsedStackVariablesMap().TryGetValue(instruction.get_Offset(), out V_0))
			{
				this.expressionStack = new Stack<Expression>();
				return;
			}
			stackVariable11 = V_0;
			stackVariable12 = ExpressionDecompilerStep.u003cu003ec.u003cu003e9__20_0;
			if (stackVariable12 == null)
			{
				dummyVar0 = stackVariable12;
				stackVariable12 = new Func<VariableDefinition, Expression>(ExpressionDecompilerStep.u003cu003ec.u003cu003e9.u003cSetInitialStacku003eb__20_0);
				ExpressionDecompilerStep.u003cu003ec.u003cu003e9__20_0 = stackVariable12;
			}
			this.expressionStack = new Stack<Expression>(stackVariable11.Select<VariableDefinition, Expression>(stackVariable12));
			return;
		}

		private bool TryGetArrayIndexingMethodName(MethodReferenceExpression methodRef, out string methodName)
		{
			methodName = methodRef.get_Member().get_Name();
			if (String.op_Equality(methodName, "Get") || String.op_Equality(methodName, "Set"))
			{
				return true;
			}
			return String.op_Equality(methodName, "Address");
		}

		private bool TryGetTargetArray(MethodReferenceExpression methodRef, out Expression target)
		{
			if (methodRef == null || methodRef.get_Member().get_DeclaringType() as ArrayType == null)
			{
				target = null;
				return false;
			}
			target = methodRef.get_Target();
			return true;
		}

		private bool TryGetVariableValueAndMarkForRemoval(VariableReferenceExpression variableRefExpression, out Expression value)
		{
			if (variableRefExpression == null || !this.methodContext.get_StackData().get_VariableToDefineUseInfo().TryGetValue(variableRefExpression.get_Variable().Resolve(), out V_0) || V_0.get_DefinedAt().get_Count() != 1 || V_0.get_UsedAt().get_Count() != 1 || !this.offsetToExpression.TryGetValue(V_0.get_DefinedAt().First<int>(), out value))
			{
				value = null;
				return false;
			}
			V_1 = variableRefExpression.get_Variable().Resolve();
			dummyVar0 = this.methodContext.get_StackData().get_VariableToDefineUseInfo().Remove(V_1);
			dummyVar1 = this.stackVariableAssignmentsToRemove.Add(V_1);
			return true;
		}

		private bool TryProcessMultidimensionalIndexing(MethodInvocationExpression invocation)
		{
			if (invocation.get_MethodExpression() == null || invocation.get_Arguments() == null || invocation.get_Arguments().get_Count() < 1)
			{
				return false;
			}
			V_0 = invocation.get_MethodExpression();
			V_1 = null;
			V_2 = null;
			if (!this.TryGetTargetArray(V_0, out V_1) || !this.TryGetArrayIndexingMethodName(V_0, out V_2))
			{
				return false;
			}
			V_3 = new ArrayIndexerExpression(V_1, invocation.get_InvocationInstructions());
			V_3.set_Indices(invocation.get_Arguments());
			if (!String.op_Equality(V_2, "Set"))
			{
				if (!String.op_Equality(V_2, "Address"))
				{
					this.Push(V_3);
				}
				else
				{
					this.Push(new UnaryExpression(7, V_3, null));
				}
			}
			else
			{
				V_3.set_IsSimpleStore(true);
				V_4 = V_3.get_Indices().get_Item(V_3.get_Indices().get_Count() - 1);
				V_3.get_Indices().RemoveAt(V_3.get_Indices().get_Count() - 1);
				this.PushAssignment(V_3, V_4, null);
			}
			return true;
		}

		private bool TryProcessRuntimeHelpersInitArray(MethodInvocationExpression invocation)
		{
			V_0 = "System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.RuntimeFieldHandle)";
			V_1 = invocation.get_MethodExpression();
			if (V_1 == null || !String.op_Equality(V_1.get_Method().get_FullName(), V_0))
			{
				return false;
			}
			stackVariable11 = this.GetClosestArrayCreationExpression();
			if (stackVariable11 == null)
			{
				throw new Exception("The expression at the top of the expression stack is not ArrayCreationExpression");
			}
			V_2 = new BlockExpression(invocation.get_InvocationInstructions());
			stackVariable11.set_Initializer(new InitializerExpression(V_2, 2));
			if (!this.TryGetVariableValueAndMarkForRemoval(invocation.get_Arguments().get_Item(1) as VariableReferenceExpression, out V_3))
			{
				throw new Exception("Invalid array initialization info");
			}
			stackVariable11.get_Initializer().get_Expressions().Add(V_3);
			return true;
		}
	}
}