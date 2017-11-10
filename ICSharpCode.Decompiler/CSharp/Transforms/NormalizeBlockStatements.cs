﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace ICSharpCode.Decompiler.CSharp.Transforms
{
	class NormalizeBlockStatements : DepthFirstAstVisitor, IAstTransform
	{
		TransformContext context;

		public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
		{
			base.VisitIfElseStatement(ifElseStatement);
			DoTransform(ifElseStatement.TrueStatement, ifElseStatement);
			DoTransform(ifElseStatement.FalseStatement, ifElseStatement);
		}

		public override void VisitWhileStatement(WhileStatement whileStatement)
		{
			base.VisitWhileStatement(whileStatement);
			InsertBlock(whileStatement.EmbeddedStatement);
		}

		public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
		{
			base.VisitDoWhileStatement(doWhileStatement);
			InsertBlock(doWhileStatement.EmbeddedStatement);
		}

		public override void VisitForeachStatement(ForeachStatement foreachStatement)
		{
			base.VisitForeachStatement(foreachStatement);
			InsertBlock(foreachStatement.EmbeddedStatement);
		}

		public override void VisitForStatement(ForStatement forStatement)
		{
			base.VisitForStatement(forStatement);
			InsertBlock(forStatement.EmbeddedStatement);
		}

		public override void VisitFixedStatement(FixedStatement fixedStatement)
		{
			base.VisitFixedStatement(fixedStatement);
			InsertBlock(fixedStatement.EmbeddedStatement);
		}

		public override void VisitLockStatement(LockStatement lockStatement)
		{
			base.VisitLockStatement(lockStatement);
			InsertBlock(lockStatement.EmbeddedStatement);
		}

		public override void VisitUsingStatement(UsingStatement usingStatement)
		{
			base.VisitUsingStatement(usingStatement);
			DoTransform(usingStatement.EmbeddedStatement, usingStatement);
		}

		void DoTransform(Statement statement, Statement parent)
		{
			if (statement.IsNull) return;
			if (context.Settings.AlwaysUseBraces) {
				InsertBlock(statement);
			} else {
				if (statement is BlockStatement b && b.Statements.Count == 1 && IsAllowed(b.Statements.First(), parent)) {
					statement.ReplaceWith(b.Statements.First().Detach());
				}
			}
		}

		static void InsertBlock(Statement statement)
		{
			if (statement.IsNull) return;
			if (!(statement is BlockStatement)) {
				var b = new BlockStatement();
				statement.ReplaceWith(b);
				b.Add(statement);
			}
		}

		bool IsAllowed(Statement statement, Statement parent)
		{
			switch (statement) {
				case VariableDeclarationStatement vds:
				case IfElseStatement ies:
				case WhileStatement ws:
				case DoWhileStatement dws:
				case SwitchStatement ss:
				case ForeachStatement fes:
				case ForStatement fs:
				case LockStatement ls:
				case FixedStatement fxs:
					return false;
				case UsingStatement us:
					return parent is UsingStatement;
				default:
					return true;
			}
		}

		void IAstTransform.Run(AstNode rootNode, TransformContext context)
		{
			this.context = context;
			rootNode.AcceptVisitor(this);
		}
	}
}