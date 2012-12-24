﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprImm : AstNodeExpr
	{
		public Type Type;
		public object Value;

		public AstNodeExprImm(object Value)
		{
			this.Value = Value;
			//if (Value is RuntimeTypeHandle) throw (new NotImplementedException("Value is RuntimeTypeHandle"));
			if (Value is Type)
			{
				this.Type = typeof(Type);
			}
			else
			{
				this.Type = (Value != null) ? Value.GetType() : typeof(object);
			}
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
		}

		protected override Type UncachedType
		{
			get { return this.Type; }
		}

		public static implicit operator AstNodeExprImm(int Value)
		{
			return new AstNodeExprImm(Value);
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Value", String.Format("{0}", Value) },
				};
			}
		}
	}

	public class AstNodeExprNull : AstNodeExpr
	{
		public readonly Type Type;

		public AstNodeExprNull(Type Type)
		{
			this.Type = Type;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
		}

		protected override Type UncachedType
		{
			get { return this.Type; }
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Type", String.Format("{0}", Type.Name) },
				};
			}
		}
	}
}
