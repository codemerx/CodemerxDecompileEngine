using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Telerik.JustDecompiler.Ast;

namespace Telerik.JustDecompiler.Ast.Expressions
{
	public class VariableDeclarationExpression : Expression
	{
		public override IEnumerable<ICodeNode> Children
		{
			get
			{
				return new VariableDeclarationExpression.u003cget_Childrenu003ed__6(-2);
			}
		}

		public override Telerik.JustDecompiler.Ast.CodeNodeType CodeNodeType
		{
			get
			{
				return 27;
			}
		}

		public override TypeReference ExpressionType
		{
			get
			{
				return this.get_Variable().get_VariableType();
			}
			set
			{
				this.get_Variable().set_VariableType(value);
				return;
			}
		}

		public VariableDefinition Variable
		{
			get;
			set;
		}

		public VariableDeclarationExpression(VariableDefinition variable, IEnumerable<Instruction> instructions)
		{
			base(instructions);
			this.set_Variable(variable);
			return;
		}

		public override Expression Clone()
		{
			return new VariableDeclarationExpression(this.get_Variable(), this.instructions);
		}

		public override Expression CloneExpressionOnly()
		{
			return new VariableDeclarationExpression(this.get_Variable(), null);
		}

		public override bool Equals(Expression other)
		{
			if (other as VariableDeclarationExpression == null)
			{
				return false;
			}
			V_0 = other as VariableDeclarationExpression;
			return (object)this.get_Variable().Resolve() == (object)V_0.get_Variable().Resolve();
		}
	}
}