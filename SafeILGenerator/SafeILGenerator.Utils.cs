﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Codegen
{
	public partial class SafeILGenerator
	{
		internal ILGenerator ILGenerator;
		SafeTypeStack TypeStack;
		List<SafeLabel> Labels = new List<SafeLabel>();
		bool OverflowCheck = false;
		bool DoEmit = true;
		bool TrackStack = true;
		bool CheckTypes = true;
		public bool DoDebug { get; private set; }

		static public TDelegate Generate<TDelegate>(Action<SafeILGenerator> Generator)
		{
			var MethodInfo = typeof(TDelegate).GetMethod("Invoke");
			var DynamicMethod = new DynamicMethod("", MethodInfo.ReturnType, MethodInfo.GetParameters().Select(Parameter => Parameter.ParameterType).ToArray());
			var ILGenerator = DynamicMethod.GetILGenerator();
			var SafeILGenerator = new SafeILGenerator(ILGenerator, CheckTypes: true, DoDebug: true);
			{
				Generator(SafeILGenerator);
			}
			return (TDelegate)(object)DynamicMethod.CreateDelegate(typeof(TDelegate));
		}

		public SafeILGenerator(ILGenerator ILGenerator, bool CheckTypes, bool DoDebug)
		{
			this.ILGenerator = ILGenerator;
			this.TypeStack = new SafeTypeStack(this);
			this.CheckTypes = CheckTypes;
			this.DoDebug = DoDebug;
		}

		public SafeTypeStack GetCurrentTypeStack()
		{
			return TypeStack;
		}

		public SafeLabel DefineLabel(string Name)
		{
			var Label = new SafeLabel(this, Name);
			Labels.Add(Label);
			return Label;
		}

		public void DoOverflowCheck(Action Action)
		{
			var OldOverflowCheck = OverflowCheck;
			OverflowCheck = true;
			try
			{
				Action();
			}
			finally
			{
				OverflowCheck = OldOverflowCheck;
			}
		}


		public void PendingOpcodes()
		{
			//OpCodes.Endfilter;
			//OpCodes.Endfinally;
			//OpCodes.Initblk;
			//OpCodes.Initobj;
			//OpCodes.Isinst;
			//OpCodes.Newarr;
			//OpCodes.Newobj;
			//OpCodes.Prefix1;
			//OpCodes.Prefix2;
			//OpCodes.Prefix3;
			//OpCodes.Prefix4;
			//OpCodes.Prefix5;
			//OpCodes.Prefix6;
			//OpCodes.Prefix7;
			//OpCodes.Prefixref;
			//OpCodes.Readonly
			//OpCodes.Refanytype
			//OpCodes.Refanyval
			//OpCodes.Sizeof
			//OpCodes.Unaligned
			//OpCodes.Unbox
			//OpCodes.Volatile
			//OpCodes.Stobj
			//OpCodes.Stfld
			//OpCodes.Stind_Ref
			//OpCodes.Stelem_Ref
			//OpCodes.Ldobj;
			//OpCodes.Ldsfld;
			//OpCodes.Ldsflda;
			//OpCodes.Ldtoken;
			//OpCodes.Ldvirtftn;
			//OpCodes.Leave;
			//OpCodes.Leave_S;
			//OpCodes.Localloc;
			//OpCodes.Mkrefany;
			//OpCodes.Ldarga;
			//OpCodes.Ldarga_S;
			//OpCodes.
			throw (new NotImplementedException());
		}

		public LocalBuilder DeclareLocal<TType>(string Name)
		{
			return ILGenerator.DeclareLocal(typeof(TType));
		}

		public void CheckAndFinalize()
		{
			foreach (var Label in Labels)
			{
				if (!Label.Marked) throw(new InvalidOperationException("Label '" + Label + "' not marked"));
			}
			ResetStack();
		}

		public void Comment(string Comment)
		{
		}

		public int StackCount
		{
			get
			{
				return TypeStack.Count;
			}
		}

		public SafeTypeStack CaptureStackInformation(Action Action)
		{
			var OldTypeStack = TypeStack;
			var NewTypeStack = TypeStack.Clone();
			var OldDoEmit = DoEmit;
			TypeStack = NewTypeStack;
			DoEmit = false;
			try
			{
				Action();
				return NewTypeStack;
			}
			finally
			{
				DoEmit = OldDoEmit;
				TypeStack = OldTypeStack;
			}
		}
	}

	public enum SafeUnaryOperator
	{
		Negate,
		Not
	}

	public enum SafePointerAttributes
	{
		Unaligned = 1,
		Volatile = 2,
	}

	public enum SafeBinaryOperator
	{
		AdditionSigned,
		AdditionUnsigned,
		And,
		DivideSigned,
		DivideUnsigned,
		MultiplySigned,
		MultiplyUnsigned,
		Or,
		RemainingSigned,
		RemainingUnsigned,
		ShiftLeft,
		ShiftRightSigned,
		ShiftRightUnsigned,
		SubstractionSigned,
		SubstractionUnsigned,
		Xor,
	}

	public enum SafeBinaryComparison
	{
		Equals,
		NotEquals,
		GreaterOrEqualSigned,
		GreaterOrEqualUnsigned,
		GreaterThanSigned,
		GreaterThanUnsigned,
		LessOrEqualSigned,
		LessOrEqualUnsigned,
		LessThanSigned,
		LessThanUnsigned,
	}

	public enum SafeUnaryComparison
	{
		False,
		True,
	}

	public class SafeTypeStack
	{
		//public List<Type> List;
		private LinkedList<Type> Stack = new LinkedList<Type>();
		private SafeILGenerator SafeILGenerator;

		internal SafeTypeStack(SafeILGenerator SafeILGenerator)
		{
			this.SafeILGenerator = SafeILGenerator;
		}

		public int Count
		{
			get
			{
				return Stack.Count;
			}
		}

		public void Pop(int Count)
		{
			while (Count-- > 0) Pop();
		}

		public Type Pop()
		{
			if (SafeILGenerator.DoDebug)
			{
				Debug.WriteLine(String.Format("## TypeStackClass.Pop: {0}", Stack.First.Value.Name));
			}
			if (Stack.Count > 0)
			{
				try
				{
					return Stack.First.Value;
				}
				finally
				{
					Stack.RemoveFirst();
				}
			}
			else
			{
				Debug.WriteLine("SafeTypeStack.Pop with no elements!");
				return null;
			}
		}

		public void Push(Type Type)
		{
			if (SafeILGenerator.DoDebug)
			{
				Debug.WriteLine(String.Format("## TypeStackClass.Push: {0}", Type.Name));
			}
			Stack.AddFirst(Type);
		}

		public SafeTypeStack Clone()
		{
			var NewTypeStack = new SafeTypeStack(SafeILGenerator);
			NewTypeStack.Stack = new LinkedList<Type>(Stack);
			return NewTypeStack;
		}

		public Type GetLastest()
		{
			return Stack.First.Value;
		}

		public Type[] GetLastestList(int Count)
		{
			return Stack.Take(Count).Reverse().ToArray();
		}
	}

	public class SafeLabel
	{
		private SafeILGenerator SafeILGenerator;
		internal System.Reflection.Emit.Label ReflectionLabel { get; private set; }
		public bool Marked { get; private set; }
		public string Name;

		internal SafeLabel(SafeILGenerator SafeILGenerator, string Name)
		{
			this.SafeILGenerator = SafeILGenerator;
			this.ReflectionLabel = SafeILGenerator.ILGenerator.DefineLabel();
			this.Name = Name;
		}

		public void Mark()
		{
			if (Marked) throw(new InvalidOperationException("Can't mark label twice"));
			SafeILGenerator.ILGenerator.MarkLabel(ReflectionLabel);
			Marked = true;
		}

		public override string ToString()
		{
			return String.Format("Label({0})", Name);
		}
	}
}
